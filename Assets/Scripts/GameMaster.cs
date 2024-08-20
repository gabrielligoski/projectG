using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.AI;
using System;

public class GameMaster : MonoBehaviour
{
    public List<List<GameObject>> map = new List<List<GameObject>>();
    public GameObject mapParent;
    [SerializeField] private int maxSize;
    [SerializeField] private int mapUpgradeAmount;
    [SerializeField] private int size;
    [SerializeField] private float roomGap;
    [SerializeField] public List<GameObject> rooms = new List<GameObject>();
    [SerializeField] private GameObject floorPfb;

    private PlayerHUD playerHUD = null;

    private string[] levels = { "F", "E", "D", "C", "B", "A" };

    public static NavMeshSurface navMeshSurface;

    public static GameObject core;
    public static GameObject floor;

    public static GameMaster Instance { get; private set; }

    public int xp;

    public int currentLevel;

    public int resource;
    public int maxResource;

    private void Awake()
    {
        mapParent = new GameObject("Map");
        Instance = this;
        resource = 100;
        maxResource = 100;
        xp = 0;
        currentLevel = 0;
    }

    void Start()
    {
        playerHUD = PlayerHUD.Instance;
        PlayerHUD.Instance.UpdateResourceBar(resource, maxResource);
        PlayerHUD.Instance.UpdateRankIcon(levels[currentLevel]);
        var coreRoom = rooms.Find(room => room.GetComponent<Room>().roomType() == Room.RoomType.core);
        var rockRoom = rooms.Find(room => room.GetComponent<Room>().roomType() == Room.RoomType.rock);
        (map, floor, core) = GenerateMap.createMap(maxSize, size, roomGap, mapParent, coreRoom, rockRoom, floorPfb);
        navMeshSurface = floor.GetComponent<NavMeshSurface>();
        navMeshSurface.BuildNavMesh();
    }

    private void useResource(int amount) {
        if (amount > resource)
        {
            throw new Exception("Not enough resource");
        }
        else {
            resource -= amount;
            Debug.Log(resource + "/" + maxResource);
        }
    }

    private void increaseResource(int amount) {
        if (resource + amount < maxResource) {
            resource += amount;
        }
        PlayerHUD.Instance.UpdateResourceBar(resource, maxResource);
    }

    private void upgradeMaxAmount() {
        maxResource = (int)Math.Pow(currentLevel+1,2)*100;
        PlayerHUD.Instance.UpdateResourceBar(resource, maxResource);
    }
    public int calculateLevel() {
        return (int)Math.Floor(Math.Sqrt(xp/(float)100));
    }
    public int xpNeededPerLevel(int level) {
        return (int)Math.Pow(level, 2) * 100;
    }
    public float percentCurrentLevel() {
        var xpNeededNext = xpNeededPerLevel(currentLevel+1);
        return ((xp-xpNeededPerLevel(currentLevel)) / (float)xpNeededNext); 
    }

    public void addXP(int xpAmount) {
        xp += xpAmount;
        PlayerHUD.Instance.UpdateExpBar(percentCurrentLevel());
        Debug.Log(percentCurrentLevel()*100 + "%");
        if(currentLevel < calculateLevel()) {
            currentLevel++;
            var rockRoom = rooms.Find(room => room.GetComponent<Room>().roomType() == Room.RoomType.rock);
            upgradeMaxAmount();
            GenerateMap.upgradeMap(map, mapParent, size, maxSize, mapUpgradeAmount, roomGap, rockRoom);
            size += mapUpgradeAmount;
            string level = currentLevel <= 5 ? levels[currentLevel] : "".PadLeft(currentLevel - 5, 'S');
;           PlayerHUD.Instance.UpdateRankIcon(level);
            Debug.Log("Upou!");
        }


    }

    private bool compareAdjacentsTo(GameObject target, Room.RoomType roomType) {
        (int,int) pos = target.GetComponent<Room>().pos;
        var adj0 = pos.Item1 - 1 >= ((maxSize/2)-(size/2)) ? map[pos.Item1 - 1][pos.Item2].GetComponent<Room>().roomType() == roomType : true;
        var adj1 = pos.Item1 + 1 < ((maxSize/2)+ (size / 2)) ? map[pos.Item1 + 1][pos.Item2].GetComponent<Room>().roomType() == roomType : true;
        var adj2 = pos.Item2 - 1 >= ((maxSize / 2) - (size / 2)) ? map[pos.Item1][pos.Item2-1].GetComponent<Room>().roomType() == roomType : true;
        var adj3 = pos.Item2 + 1 < ((maxSize / 2) + (size / 2)) ? map[pos.Item1][pos.Item2+1].GetComponent<Room>().roomType() == roomType : true;

        return adj0 && adj1 && adj2 && adj3; 
    }

    private bool isPossibleToSwap(GameObject target, Room.RoomType newRoomType)
    {
        if (target.TryGetComponent<Room>(out Room room) && room.roomType() != Room.RoomType.core)
        {
            switch (newRoomType)
            {
                case Room.RoomType.empty:
                    return !compareAdjacentsTo(target, Room.RoomType.rock) && room.roomType() == Room.RoomType.rock;
                default:
                    return room.roomType() == Room.RoomType.empty;
            }

        } else {
            return false;
        }
    }

    public void swapRoom(GameObject target, Room.RoomType newRoomType) {
        if (!target.tag.Contains("Core"))
        {
            try {
                var newRoom = rooms.Find(room => room.GetComponent<Room>().roomType() == newRoomType);
                var cost = newRoom.GetComponent<Room>().cost;
                var targetPos = target.GetComponent<Room>().pos;
                Debug.Log(targetPos);
                if(isPossibleToSwap(target, newRoomType))
                {
                    if (newRoomType == Room.RoomType.empty) {
                        addXP(20);
                    }
                    useResource(cost);
                    map[targetPos.Item1][targetPos.Item2] = Instantiate(newRoom, target.transform.position, Quaternion.identity, target.transform.parent);
                    map[targetPos.Item1][targetPos.Item2].GetComponent<Room>().pos = targetPos;
                    Destroy(target);
                    return;
                } 
                else
                {
                    Debug.Log("Not possible to swap this block");
                }

            } catch (Exception ex)
            {
                Debug.Log(ex);
            }
        }

    }

}
