using System.Collections.Generic;
using UnityEngine;

public class RobotSpawner : Room
{
    [SerializeField] private Dictionary<string, GameObject> robotPfb;

    public List<GameObject> spawns = new List<GameObject>();

    public override RoomType roomType()
    {
        return RoomType.robot_spawner;
    }


    public void Spawn(Dictionary<string, int> robotsToSpawn)
    {
        foreach (var robot in robotsToSpawn)
            for (int i = 0; i < robot.Value; i++)
                Instantiate(robotPfb[robot.Key], gameObject.transform.position, Quaternion.identity);

    }

}
