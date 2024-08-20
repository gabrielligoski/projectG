using System.Collections.Generic;
using UnityEngine;

public class GenerateMap : MonoBehaviour
{
    public static (List<List<GameObject>>, GameObject, GameObject) createMap(int size, float roomGap, GameObject coreRoom, GameObject blankRoom, GameObject floor)
    {
        GameObject core = null;
        var map = new List<List<GameObject>>();
        var mapParent = new GameObject("Map").transform;
        for (int i = 0; i < size; i++)
        {
            var row = new List<GameObject>();
            map.Add(row);
            for (int j = 0; j < size; j++)
            {
                var x = i + i * roomGap;
                var z = j + j * roomGap;

                if (i == size / 2 && j == size / 2)
                {
                    core = Instantiate(coreRoom, new Vector3(x, 0, z), Quaternion.identity, mapParent);
                    core.GetComponent<Room>().pos = (i, j);
                    row.Add(core);
                }
                else
                {
                    GameObject newRoom = Instantiate(blankRoom, new Vector3(x, 0, z), Quaternion.identity, mapParent);
                    newRoom.GetComponent<Room>().pos = (i, j);
                    row.Add(newRoom);
                }
            }
        }

        var floorInstance = Instantiate(floor, new Vector3(size / 2, 0, size / 2), Quaternion.identity, mapParent);
        // offset by half a block
        floorInstance.transform.position += new Vector3(.5f, -.5f, .5f);
        // plane is 9x9 by
        floorInstance.transform.localScale = Vector3.one * (size / 9f);
        return (map, floorInstance, core);
    }
}
