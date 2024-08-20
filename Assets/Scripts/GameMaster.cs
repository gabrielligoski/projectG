using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.AI;
using System;
using Random = UnityEngine.Random;

public class GameMaster : MonoBehaviour
{
    public List<List<GameObject>> map = new List<List<GameObject>>();

    [SerializeField] private int safeZoneSize = 2;
    [SerializeField] private int size;
    [SerializeField] private float roomGap;
    [SerializeField] private List<GameObject> rooms = new List<GameObject>();
    [SerializeField] private GameObject floorPfb;

    public static NavMeshSurface navMeshSurface;

    public static GameObject core;
    public static GameObject floor;

    public static GameMaster Instance { get; private set; }

    public int resource;
    public int maxResource;
    public float dificulty = 1f;
    private float goodBlockChance = .05f;
    private float badBlockChance = .01f;
    private List<Room.RoomType> badRooms = new List<Room.RoomType>() {
            Room.RoomType.spider_spawner,
            Room.RoomType.robot_spawner,
            Room.RoomType.bat_spawner,
            Room.RoomType.porcupine_spawner
        };

    private List<Room.RoomType> goodRooms = new List<Room.RoomType>() {
            Room.RoomType.mining
        };

    private void Awake()
    {
        Instance = this;
        resource = 1000;
        maxResource = 1000;
    }

    void Start()
    {
        var coreRoom = rooms.Find(room => room.GetComponent<Room>().roomType() == Room.RoomType.core);
        var rockRoom = rooms.Find(room => room.GetComponent<Room>().roomType() == Room.RoomType.rock);
        (map, floor, core) = GenerateMap.createMap(size, roomGap, coreRoom, rockRoom, floorPfb);
        navMeshSurface = floor.GetComponent<NavMeshSurface>();
        navMeshSurface.BuildNavMesh();
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
            Debug.Log(resource + "/" + maxResource);
        }
    }

    private void increaseResource(int amount)
    {
        if (resource + amount < maxResource)
        {
            resource += amount;
        }
    }

    private void upgradeMaxAmount(int amount)
    {
        maxResource += amount;
    }


    private bool compareAdjacentsTo(GameObject target, Room.RoomType roomType)
    {
        (int, int) pos = target.GetComponent<Room>().pos;
        var adj0 = pos.Item1 - 1 >= 0 ? map[pos.Item1 - 1][pos.Item2].GetComponent<Room>().roomType() == roomType : true;
        var adj1 = pos.Item1 + 1 < map.Count ? map[pos.Item1 + 1][pos.Item2].GetComponent<Room>().roomType() == roomType : true;
        var adj2 = pos.Item2 - 1 >= 0 ? map[pos.Item1][pos.Item2 - 1].GetComponent<Room>().roomType() == roomType : true;
        var adj3 = pos.Item2 + 1 < map[pos.Item1].Count ? map[pos.Item1][pos.Item2 + 1].GetComponent<Room>().roomType() == roomType : true;

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
            badBlockChance = 0.01f;
            return rooms.Find(room => room.GetComponent<Room>().roomType() == badRooms[Random.Range(0, badRooms.Count)]);
        }


        if (Random.value <= goodBlockChance)
        {
            goodBlockChance = 0.05f;
            return rooms.Find(room => room.GetComponent<Room>().roomType() == goodRooms[Random.Range(0, goodRooms.Count)]);
        }

        // raise the chance of something happens
        goodBlockChance += 0.01f / dificulty;
        badBlockChance += 0.01f * dificulty;

        return rooms.Find(room => room.GetComponent<Room>().roomType() == Room.RoomType.empty);

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
                map[targetPos.Item1][targetPos.Item2] = Instantiate(newRoom, target.transform.position, Quaternion.identity, target.transform.parent);
                map[targetPos.Item1][targetPos.Item2].GetComponent<Room>().pos = targetPos;
                Destroy(target);

            }
            catch (Exception ex)
            {
                Debug.Log(ex);
                Debug.Log(newRoomType);
            }
        }
        else
        {
            Debug.Log("Not possible to swap this block");
        }

    }

}
