using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using System;
using Random = UnityEngine.Random;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class GameMaster : MonoBehaviour
{
    public List<List<GameObject>> map = new List<List<GameObject>>();
    [SerializeField] private int safeZoneSize = 2;
    public GameObject mapParent;
    [SerializeField] private int maxSize;
    [SerializeField] private int mapUpgradeAmount;
    [SerializeField] public int size;
    [SerializeField] private float roomGap;
    [SerializeField] public List<GameObject> rooms = new List<GameObject>();
    [SerializeField] public List<RobotSpawner> robotSpawners = new List<RobotSpawner>();
    [SerializeField] private GameObject floorPfb;
    [SerializeField] private GameObject breakBlockVFX;
    [SerializeField] private AudioClip sfxClip;

    public bool hasStartedWaves;

    private PlayerHUD playerHUD = null;

    public string[] levels = { "F", "E", "D", "C", "B", "A" };

    public static NavMeshSurface navMeshSurface;

    public static GameObject core;
    public static GameObject floor;

    public static GameMaster Instance { get; private set; }

    public int xp;

    public int currentLevel;
    public int mineRate;
    public int mineRatio;
    public int resource;
    public int maxResource;
    public float dificulty = 1f;
    private float goodBlockChance = .05f;
    private float badBlockChance = .05f;
    private List<Room.RoomType> badRooms = new List<Room.RoomType>() {
            Room.RoomType.spider_spawner,
            Room.RoomType.robot_spawner,
            Room.RoomType.bat_spawner,
            Room.RoomType.porcupine_spawner
        };

    private List<Room.RoomType> goodRooms = new List<Room.RoomType>() {
            Room.RoomType.mining
        };


    public void restartGame()
    {
        robotSpawners.ForEach(r => Destroy(r));
        robotSpawners.Clear();
        Destroy(mapParent);
        startGame();
    }

    private void startGame()
    {
        mapParent = new GameObject("Map");
        resource = 100;
        maxResource = 100;
        xp = 0;
        currentLevel = 0;
        size = 21;
        
        var coreRoom = rooms.Find(room => room.GetComponent<Room>().roomType() == Room.RoomType.core);
        var rockRoom = rooms.Find(room => room.GetComponent<Room>().roomType() == Room.RoomType.rock);
        (map, floor, core) = GenerateMap.createMap(maxSize, size, roomGap, mapParent, coreRoom, rockRoom, floorPfb);
        navMeshSurface = floor.GetComponent<NavMeshSurface>();
        navMeshSurface.BuildNavMesh();

        Camera.main.GetComponent<CameraFollow>().StartCamera();

        playerHUD = PlayerHUD.Instance;
    }

    private void Awake()
    {
        Instance = this;
        resource = 100;
        maxResource = 100;
        xp = 0;
        currentLevel = 0;
    }

    void Start()
    {
        startGame();
    }

    private void useResource(int amount)
    {
        if (amount > resource)
        {
            throw new Exception("Not enough resource");
        }
        else
        {
            resource -= amount;
        }
    }

    public void increaseResource(int amount)
    {
        if (resource + amount < maxResource)
        {
            resource += amount;
        }
        else
        {
            resource = maxResource;
        }
        PlayerHUD.Instance.UpdateResourceBar(resource, maxResource);
    }

    private void upgradeMaxAmount()
    {
        mineRatio++;
        maxResource = (int)Math.Pow(currentLevel + 1, 2) * 100;
        PlayerHUD.Instance.UpdateResourceBar(resource, maxResource);
    }

    public int calculateLevel()
    {
        return (int)Math.Floor(Math.Sqrt(xp / (float)100));
    }
    public int xpNeededPerLevel(int level)
    {
        return (int)Math.Pow(level, 2) * 100;
    }
    public float percentCurrentLevel()
    {
        var xpNeededNext = xpNeededPerLevel(currentLevel + 1);
        return ((xp - xpNeededPerLevel(currentLevel)) / (float)xpNeededNext);
    }

    public void addXP(int xpAmount)
    {
        xp += xpAmount;
        dificulty = 1 + xp / 500;
        PlayerHUD.Instance.UpdateExpBar(percentCurrentLevel());
        if (currentLevel < calculateLevel())
        {
            currentLevel++;
            var rockRoom = rooms.Find(room => room.GetComponent<Room>().roomType() == Room.RoomType.rock);
            upgradeMaxAmount();
            GenerateMap.upgradeMap(map, mapParent, size, maxSize, mapUpgradeAmount, roomGap, rockRoom);
            size += mapUpgradeAmount;
            string level = currentLevel <= 5 ? levels[currentLevel] : "".PadLeft(currentLevel - 5, 'S');
            PlayerHUD.Instance.UpdateRankIcon(level);
        }
    }

    private bool compareAdjacentsTo(GameObject target, Room.RoomType roomType)
    {
        (int, int) pos = target.GetComponent<Room>().pos;
        var adj0 = pos.Item1 - 1 >= ((maxSize / 2) - (size / 2)) ? map[pos.Item1 - 1][pos.Item2].GetComponent<Room>().roomType() == roomType : true;
        var adj1 = pos.Item1 + 1 < ((maxSize / 2) + (size / 2)) ? map[pos.Item1 + 1][pos.Item2].GetComponent<Room>().roomType() == roomType : true;
        var adj2 = pos.Item2 - 1 >= ((maxSize / 2) - (size / 2)) ? map[pos.Item1][pos.Item2 - 1].GetComponent<Room>().roomType() == roomType : true;
        var adj3 = pos.Item2 + 1 < ((maxSize / 2) + (size / 2)) ? map[pos.Item1][pos.Item2 + 1].GetComponent<Room>().roomType() == roomType : true;

        return adj0 && adj1 && adj2 && adj3;
    }

    private bool isPossibleToSwap(GameObject target, Room.RoomType newRoomType)
    {
        if (target.TryGetComponent<Room>(out Room room) && room.roomType() != Room.RoomType.core)
        {
            switch (newRoomType)
            {
                case Room.RoomType.empty:
                    if (target.TryGetComponent(out Room r) && r.roomType() == Room.RoomType.bomb_trap)
                    {
                        return true;
                    }
                    return !compareAdjacentsTo(target, Room.RoomType.rock) && room.roomType() == Room.RoomType.rock;
                default:
                    return room.roomType() == Room.RoomType.empty;
            }

        }
        else
        {
            return false;
        }
    }

    private GameObject chooseRandomBlock((int, int) xz)
    {
        var (coreX, coreZ) = core.GetComponent<Room>().pos;
        bool isInSafeZone = coreX - safeZoneSize <= xz.Item1 && coreX + safeZoneSize >= xz.Item1 && coreZ - safeZoneSize <= xz.Item2 && coreZ + safeZoneSize >= xz.Item2;

        if (!isInSafeZone && Random.value <= badBlockChance)
        {
            badBlockChance = 0.025f;
            return rooms.Find(room => room.GetComponent<Room>().roomType() == badRooms[Random.Range(0, badRooms.Count)]);
        }


        if (Random.value <= goodBlockChance)
        {
            goodBlockChance = 0.05f;
            return rooms.Find(room => room.GetComponent<Room>().roomType() == goodRooms[Random.Range(0, goodRooms.Count)]);
        }

        // raise the chance of something happens
        goodBlockChance += 0.01f / dificulty;
        badBlockChance += 0.05f * dificulty;

        return rooms.Find(room => room.GetComponent<Room>().roomType() == Room.RoomType.empty);

    }

    public IEnumerator SpawnWaves()
    {
        while (true)
        {
            yield return new WaitForSeconds(30);
            Dictionary<string, int> robotsToSpawn = new Dictionary<string, int>();

            robotsToSpawn.Add("dummy_1", (int)((dificulty - .9f) * 10));
            if (dificulty > 1.5f)
                robotsToSpawn.Add("dummy_2", (int)((dificulty - 1.5f) * 5));
            if (dificulty > 2)
                robotsToSpawn.Add("dummy_3", (int)(dificulty - 1));
            robotSpawners.ForEach(spawner => spawner.Spawn(robotsToSpawn));
        }
    }


    public void swapRoom(GameObject target, Room.RoomType newRoomType)
    {
        if (isPossibleToSwap(target, newRoomType))
        {
            try
            {
                var newRoom = rooms.Find(room => room.GetComponent<Room>().roomType() == newRoomType);

                var targetPos = target.GetComponent<Room>().pos;
                if (target.GetComponent<Room>().roomType() == Room.RoomType.rock)
                    newRoom = chooseRandomBlock(targetPos);

                var cost = newRoom.GetComponent<Room>().cost;
                useResource(cost);
                PlayerHUD.Instance.UpdateResourceBar(resource, maxResource);
                map[targetPos.Item1][targetPos.Item2] = Instantiate(newRoom, target.transform.position, Quaternion.identity, target.transform.parent);
                Instantiate(breakBlockVFX, target.transform.position, Quaternion.identity, null);
                SFXManager.Instance.playSFXClip(sfxClip, transform, 1f, 0.05f);
                map[targetPos.Item1][targetPos.Item2].GetComponent<Room>().pos = targetPos;
                Destroy(target);

            }
            catch (Exception ex)
            {
                Debug.Log(ex);
                Debug.Log(newRoomType);
            }
        }

    }

}
