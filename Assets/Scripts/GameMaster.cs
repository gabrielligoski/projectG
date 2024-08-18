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

    public static GameMaster Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        var coreRoom = rooms.Find(room => room.GetComponent<Room>().roomType() == Room.RoomType.core);
        var rockRoom = rooms.Find(room => room.GetComponent<Room>().roomType() == Room.RoomType.rock);
        (map, floor, core) = GenerateMap.createMap(size, roomGap, coreRoom, rockRoom, floorPfb);
        navMeshSurface = floor.GetComponent<NavMeshSurface>();
        navMeshSurface.BuildNavMesh();
    }

    public void swapRoom(GameObject target, Room.RoomType newRoomType) {
        if (!target.tag.Contains("Core"))
        {
            var newRoom = rooms.Find(room => room.GetComponent<Room>().roomType() == newRoomType);
            map.ForEach((row) => {
                int index = row.FindIndex(room => room == target);
                if(index != -1)
                {
                    row[index] = Instantiate(newRoom, target.transform.position, Quaternion.identity, target.transform.parent);
                    Destroy(target);
                    return;
                }
            });
        }

    }

}
