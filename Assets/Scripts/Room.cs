using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class Room : MonoBehaviour
{
    public (int,int) pos;
    [SerializeField] public string name;

    public int cost;
    public enum RoomType
    {
        none,
        empty,
        rock,
        core,
        robot_spawner,
        orc_spawner,
        lizardman_spawner,
        werewolf_spawner,
        skeleton_spawner,
        spider_spawner,
        bat_spawner,
        porcupine_spawner,
        spike_trap,
        bomb_trap,
        mining
    };
    public abstract RoomType roomType();
    public string description;

}
