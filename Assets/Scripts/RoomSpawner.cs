using Breeze.Core;
using System.Collections.Generic;
using UnityEngine;

public class RoomSpawner : MonoBehaviour
{
    [SerializeField] private string name;
    [SerializeField] private GameObject pfb;
    [SerializeField] private int quantity;
    [SerializeField] private int maxQuantity;
    [SerializeField] private float spawnTimer;

    public List<GameObject> spawns = new List<GameObject>();

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
        for (int i = 0; i < quantity; i++)
        {
            var spawnedInstance = Instantiate(pfb, gameObject.transform.position, Quaternion.identity);
            spawnedInstance.GetComponent<CharacterController>().waypoint = breezeWaypoint;
            spawns.Add(spawnedInstance);
        }
    }

}
