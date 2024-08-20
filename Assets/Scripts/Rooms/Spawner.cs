using Breeze.Core;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : Room
{
    [SerializeField] private GameObject pfb;
    [SerializeField] private int quantity;
    [SerializeField] private int maxQuantity;
    [SerializeField] private float spawnTimer;

    public List<GameObject> spawns = new List<GameObject>();

    public override RoomType roomType()
    {
        return name;
    }

    private BreezeWaypoint breezeWaypoint;
    private float timer = 0;

    private void Start()
    {
        breezeWaypoint = gameObject.AddComponent<BreezeWaypoint>();
        breezeWaypoint.MaxIdleLength = int.MaxValue;
        breezeWaypoint.MinIdleLength = int.MaxValue;
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer > spawnTimer && spawns.Count < maxQuantity)
        {
            Spawn();
            timer = 0;
        }
    }

    public void Spawn()
    {
        for (int i = 0; i < quantity && spawns.Count < maxQuantity; i++)
        {
            var spawnedInstance = Instantiate(pfb, gameObject.transform.position, Quaternion.identity);
            spawnedInstance.GetComponent<CharacterController>().waypoint = breezeWaypoint;
            spawnedInstance.GetComponent<CharacterController>().spawn = this;
            spawns.Add(spawnedInstance);
        }
    }

}
