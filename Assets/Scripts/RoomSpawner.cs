using System.Collections.Generic;
using UnityEngine;

public class RoomSpawner : MonoBehaviour
{
    [SerializeField] private string name;
    [SerializeField] private GameObject pfb;
    [SerializeField] private int quantity;
    [SerializeField] private float spawnTimer;

    public List<GameObject> spawns = new List<GameObject>();

    private float timer = 0;

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer > spawnTimer)
        {
            Spawn();
            timer = 0;
        }
    }

    public void Spawn()
    {
        for (int i = 0; i < quantity; i++)
        {
            spawns.Add(Instantiate(pfb, gameObject.transform.position, Quaternion.identity));
        }
    }

}
