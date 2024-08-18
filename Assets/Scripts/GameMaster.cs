using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

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

    void Start()
    {
        var coreRoom = rooms.Find(room => room.GetComponent<Room>().roomType() == Room.RoomType.core);
        var emptyRoom = rooms.Find(room => room.GetComponent<Room>().roomType() == Room.RoomType.empty);
        (map, floor, core) = GenerateMap.createMap(size, roomGap, coreRoom, emptyRoom, floorPfb);
        navMeshSurface = floor.GetComponent<NavMeshSurface>();
        navMeshSurface.BuildNavMesh();

    }

}
