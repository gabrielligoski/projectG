using System.Collections.Generic;
using System.ComponentModel;
using Breeze.Core;
using UnityEngine;
using static Room;

public class Map : MonoBehaviour
{
    [SerializeField] private int MapSize;
    [SerializeField] public List<GameObject> tileTypes = new List<GameObject>();
    [SerializeField] private bool safeZone;
    [SerializeField] private int safeZoneSize;
    [SerializeField] private float roomGap;
    private int currentSize;
    public (int,int) coreCoordinates {get; private set;}
    public GameObject[,] tiles;
    private GameObject map;

    public int getMapSize(){
        return MapSize;
    }

    private void createTile(RoomType type, int p1, int p2){
        tiles[p1,p2] = Instantiate(getTileByType(type), new Vector3(p1, 0, p2), Quaternion.identity, map.transform);
        tiles[p1,p2].GetComponent<Room>().pos = (p1,p2);
    }

    private Map createMap(){
        map = gameObject;
        tiles = new GameObject[MapSize,MapSize];
        int middle=MapSize/2;
        coreCoordinates = (middle,middle);
        return this;
    }
    
    void Awake(){
        createMap().createCore().createSafezone().createBoundary();
    }


    public GameObject getTileByType(RoomType type){
        foreach(GameObject tile in tileTypes){
            tile.TryGetComponent(out Room room);
            if(room.roomType() == type){
                return tile;
            }
        }
        return null;
    }
    private Map createCore(){
        int p1 = coreCoordinates.Item1;
        int p2 = coreCoordinates.Item2;
        createTile(RoomType.core, p1, p2);
        return this;
    }

    private Map createSafezone(){
        int safeZoneStartPoint = coreCoordinates.Item1-safeZoneSize; 
        int safeZoneEndingPoint = coreCoordinates.Item1+safeZoneSize; 
        for(int i = safeZoneStartPoint; i <= safeZoneEndingPoint; i++){
            for(int j = safeZoneStartPoint; j <= safeZoneEndingPoint; j++){
                if(i!=coreCoordinates.Item1 || j!=coreCoordinates.Item2)
                    createTile(RoomType.empty, i, j);
            }
        }
        return this;
    }

    private Map createBoundary(){
        int safeZoneStartPoint = coreCoordinates.Item1-safeZoneSize; 
        int safeZoneEndingPoint = coreCoordinates.Item1+safeZoneSize;
        for(int i = safeZoneStartPoint-1; i<=safeZoneEndingPoint+1; i++){
            if(tiles[i,safeZoneStartPoint-1] == null){
                createTile(RoomType.rock, i, safeZoneStartPoint-1);
            }
            if(tiles[i,safeZoneEndingPoint+1] == null){
                createTile(RoomType.rock, i, safeZoneEndingPoint+1);
            }
            if(tiles[safeZoneStartPoint-1,i] == null){
                createTile(RoomType.rock, safeZoneStartPoint-1, i);
            }
            if(tiles[safeZoneEndingPoint+1,i] == null){
                createTile(RoomType.rock, safeZoneEndingPoint+1, i);
            }
        }
        
        return this;
    }
    
    public Map swapTile(int x, int y, GameObject newTile){
        Destroy(tiles[x,y]);
        tiles[x,y] = Instantiate(newTile, new Vector3(x, 0, y), Quaternion.identity, map.transform);
        tiles[x,y].GetComponent<Room>().pos = (x,y);

        return this;
    } 

    public Vector3 getCorePosition(){
        return tiles[coreCoordinates.Item1,coreCoordinates.Item2].transform.position;
    }

    public BreezeWaypoint getCoreWaypoint(){
        return tiles[coreCoordinates.Item1,coreCoordinates.Item2].GetComponent<BreezeWaypoint>();
    }

    public Map openPath(int x, int y){
        var floor = getTileByType(RoomType.empty);
        var wall = getTileByType(RoomType.rock);


        tiles[x,y].TryGetComponent(out Room room);
        
        //Check if the target is a wall
        if(room.roomType() == RoomType.rock){
            swapTile(x,y,floor);
        }

        //Check if adjacents is null inbound
        bool left = x-1 >= 0 && tiles[x-1,y] == null;
        bool right = x+1 < MapSize && tiles[x+1,y] == null;
        bool bottom = y-1 >= 0 && tiles[x,y-1] == null;
        bool top = y+1 < MapSize && tiles[x,y+1] == null;

        if(left)
            swapTile(x-1,y,wall);
        if(right)
            swapTile(x+1,y,wall);
        if(bottom)
            swapTile(x,y-1,wall);
        if(top)
            swapTile(x,y+1,wall);
        
        return this;
    }

    // public static (List<List<GameObject>>, GameObject, GameObject) createMap(int maxSize, int size, float roomGap, GameObject mapParent,GameObject coreRoom, GameObject filler, GameObject floor)
    // {
    //     GameObject core = null;
    //     var map = new List<List<GameObject>>();
    //     var mapParentTranformed = mapParent.transform;
    //     for (int i = 0; i < maxSize; i++) { 
    //         var row = new List<GameObject>();
    //         for (int j = 0; j < maxSize; j++) {
    //             row.Add(null);
    //         }
    //         map.Add(row);
    //     }
    //     int midTotalMap = maxSize / 2;
    //     int offsetSize = size / 2;
    //     int startPlayablePoint = midTotalMap-offsetSize;
    //     int endPlayablePoint = startPlayablePoint+size;
    //     for (int i = startPlayablePoint; i < endPlayablePoint; i++)
    //     {
    //         for (int j = startPlayablePoint; j < endPlayablePoint; j++)
    //         {
    //             var x = (i + i * roomGap)-midTotalMap;
    //             var z = (j + j * roomGap)-midTotalMap;

    //             if (i == midTotalMap && j == midTotalMap)
    //             {
    //                 core = Instantiate(coreRoom, new Vector3(x, 0, z), Quaternion.identity, mapParentTranformed);
    //                 core.GetComponent<Room>().pos = (i, j);
    //                 map[i][j] = core;
    //             }
    //             else
    //             {
    //                 GameObject newRoom = Instantiate(filler, new Vector3(x, 0, z), Quaternion.identity, mapParentTranformed);
    //                 newRoom.GetComponent<Room>().pos = (i, j);
    //                 map[i][j] = newRoom;
    //             }
    //         }
    //     }

    //     var floorInstance = Instantiate(floor, new Vector3(0, 0, 0), Quaternion.identity, mapParentTranformed);
    //     floorInstance.GetComponent<MeshRenderer>().enabled = false;
    // //     // offset by half a block
    //     floorInstance.transform.position += new Vector3(.5f, -.5f, .5f);
    // //     // plane is 9x9 by
    //     floorInstance.transform.localScale = Vector3.one * (maxSize / 9f);
    //     return (map, floorInstance, core);
    // }
}
