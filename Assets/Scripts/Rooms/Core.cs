using Breeze.Core;
using UnityEngine;

public class Core : Room
{
    public override RoomType roomType()
    {
        return RoomType.core;
    }

    public static Core core;
    public BreezeWaypoint bWaypoint;


    private void Awake()
    {
        core = this;
        bWaypoint = GetComponent<BreezeWaypoint>();
    }

}
