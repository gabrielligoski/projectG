using System.Collections.Generic;
using UnityEngine;

public class GameMaster : MonoBehaviour
{
    public List<List<GameObject>> map = new List<List<GameObject>>();

    [SerializeField] private int size;
    [SerializeField] private float roomGap;
    [SerializeField] private GameObject coreRoom;
    [SerializeField] private GameObject blankRoom;

    void Start()
    {
        map = GenerateMap.createMap(size, roomGap, coreRoom, blankRoom);
    }

}
