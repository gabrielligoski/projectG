using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.AI;
using System;

public class GameMaster : MonoBehaviour
{
    public List<List<GameObject>> map = new List<List<GameObject>>();

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

    private void Awake()
    {
        Instance = this;
        resource = 100;
        maxResource = 100;
    }

    void Start()
    {
        var coreRoom = rooms.Find(room => room.GetComponent<Room>().roomType() == Room.RoomType.core);
        var rockRoom = rooms.Find(room => room.GetComponent<Room>().roomType() == Room.RoomType.rock);
        (map, floor, core) = GenerateMap.createMap(size, roomGap, coreRoom, rockRoom, floorPfb);
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
    }

    private void upgradeMaxAmount(int amount) {
        maxResource += amount;
    }


    private bool compareAdjacentsTo(GameObject target, Room.RoomType roomType) {
        (int,int) pos = target.GetComponent<Room>().pos;
        var adj0 = pos.Item1 - 1 >= 0 ? map[pos.Item1 - 1][pos.Item2].GetComponent<Room>().roomType() == roomType : true;
        var adj1 = pos.Item1 + 1 < map.Count ? map[pos.Item1 + 1][pos.Item2].GetComponent<Room>().roomType() == roomType : true;
        var adj2 = pos.Item2 - 1 >= 0 ? map[pos.Item1][pos.Item2-1].GetComponent<Room>().roomType() == roomType : true;
        var adj3 = pos.Item2 + 1 < map[pos.Item1].Count ? map[pos.Item1][pos.Item2+1].GetComponent<Room>().roomType() == roomType : true;

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
                if(isPossibleToSwap(target, newRoomType))
                {
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
