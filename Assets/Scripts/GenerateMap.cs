using System.Collections.Generic;
using UnityEngine;

public class GenerateMap : MonoBehaviour
{
    //Amount eh o valor(impar) em que o mapa sera aumentado
    public static void upgradeMap(List<List<GameObject>> map, GameObject mapParent, int currentSize, int maxSize, int amount, float roomGap, GameObject filler) 
    {
        if (currentSize + amount < maxSize) { 
            var mapParentTransformed = mapParent.transform;

            int midTotalMap = maxSize / 2;
            int offsetCurrentSize = currentSize / 2;

            int currentStartingPoint = midTotalMap - offsetCurrentSize;
            int currentEndingPoint = midTotalMap + offsetCurrentSize;

            int newStartingPoint = midTotalMap - offsetCurrentSize-(amount/2);
            int newEndingPoint = midTotalMap + offsetCurrentSize+(amount/2);
            for(int i = newStartingPoint; i <= newEndingPoint; i++)
            {
                for (int j = newStartingPoint; j <= newEndingPoint; j++) 
                { 
                    if(!(i >= currentStartingPoint && i <= currentEndingPoint && j >= currentStartingPoint && j <= currentEndingPoint)) 
                    {

                        var x = (i + i * roomGap) - midTotalMap;
                        var z = (j + j * roomGap) - midTotalMap;

                        GameObject newRoom = Instantiate(filler, new Vector3(x, 0, z), Quaternion.identity, mapParentTransformed);
                        newRoom.GetComponent<Room>().pos = (i, j);
                        map[i][j] = newRoom;
                    }   
                }
            }
        }
    }
    public static (List<List<GameObject>>, GameObject, GameObject) createMap(int maxSize, int size, float roomGap, GameObject mapParent,GameObject coreRoom, GameObject filler, GameObject floor)
    {
        GameObject core = null;
        var map = new List<List<GameObject>>();
        for (int i = 0; i < maxSize; i++) { 
            var row = new List<GameObject>();
            for (int j = 0; j < maxSize; j++) {
                row.Add(null);
            }
            map.Add(row);
        }
        Debug.Log(map.Count);
        int midTotalMap = maxSize / 2;
        int offsetSize = size / 2;
        int startPlayablePoint = midTotalMap-offsetSize;
        int endPlayablePoint = startPlayablePoint+size;
        var mapParentTranformed = mapParent.transform;
        for (int i = startPlayablePoint; i < endPlayablePoint; i++)
        {
            for (int j = startPlayablePoint; j < endPlayablePoint; j++)
            {
                var x = (i + i * roomGap)-midTotalMap;
                var z = (j + j * roomGap)-midTotalMap;

                if (i == midTotalMap && j == midTotalMap)
                {
                    core = Instantiate(coreRoom, new Vector3(x, 0, z), Quaternion.identity, mapParentTranformed);
                    core.GetComponent<Room>().pos = (i, j);
                    map[i][j] = core;
                }
                else
                {
                    GameObject newRoom = Instantiate(filler, new Vector3(x, 0, z), Quaternion.identity, mapParentTranformed);
                    newRoom.GetComponent<Room>().pos = (i, j);
                    map[i][j] = newRoom;
                }
            }
        }

        var floorInstance = Instantiate(floor, new Vector3(0, 0, 0), Quaternion.identity, mapParentTranformed);
        floorInstance.GetComponent<MeshRenderer>().enabled = false;
        // offset by half a block
        floorInstance.transform.position += new Vector3(.5f, -.5f, .5f);
        // plane is 9x9 by
        floorInstance.transform.localScale = Vector3.one * (maxSize / 9f);
        return (map, floorInstance, core);
    }
}
