using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class RobotSpawner : Room
{
    public List<GameObject> robotPfb = new List<GameObject>();

    public override RoomType roomType()
    {
        return RoomType.robot_spawner;
    }

    private void Start()
    {
        GameMaster.Instance.robotSpawners.Add(this);
        if (!GameMaster.Instance.hasStartedWaves)
        {
            StartCoroutine(GameMaster.Instance.SpawnWaves());
            GameMaster.Instance.hasStartedWaves = true;
        }
    }

    public void Spawn(Dictionary<string, int> robotsToSpawn)
    {
        foreach (var robot in robotsToSpawn)
            for (int i = 0; i < robot.Value; i++)
            {
                var spawnPos = Random.insideUnitCircle;
                Instantiate(robotPfb.Find(rb => rb.name.Equals(robot.Key)), gameObject.transform.position + new Vector3(spawnPos.x, 0, spawnPos.y), Quaternion.identity);
            }

    }

}
