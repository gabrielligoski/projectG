using System.Collections.Generic;
using UnityEngine;

public class GenerateMap : MonoBehaviour
{
    public static List<List<GameObject>> createMap(int size, float roomGap, GameObject coreRoom, GameObject blankRoom)
    {
        var map = new List<List<GameObject>>();
        var mapParent = new GameObject("Map");
        for (int i = 0; i < size; i++)
        {
            var row = new List<GameObject>();
            map.Add(row);
            for (int j = 0; j < size; j++)
            {
                var x = i + i * roomGap;
                var z = j + j * roomGap;

                if (i == size / 2 && j == size / 2)
                    row.Add(Instantiate(coreRoom, new Vector3(x, 0, z), Quaternion.identity, mapParent.transform));
                else
                    row.Add(Instantiate(blankRoom, new Vector3(x, 0, z), Quaternion.identity, mapParent.transform));
            }
        }

        return map;
    }
}
