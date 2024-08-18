using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class GameMaster : MonoBehaviour
{
    public List<List<GameObject>> map = new List<List<GameObject>>();

    [SerializeField] private int size;
    [SerializeField] private float roomGap;
    [SerializeField] private GameObject coreRoom;
    [SerializeField] private GameObject blankRoom;
    [SerializeField] private GameObject floorPfb;

    public static NavMeshSurface navMeshSurface;

    public static GameObject core;
    public static GameObject floor;

    void Start()
    {
        (map, floor, core) = GenerateMap.createMap(size, roomGap, coreRoom, blankRoom, floorPfb);
        navMeshSurface = floor.GetComponent<NavMeshSurface>();
        navMeshSurface.BuildNavMesh();

    }

}
