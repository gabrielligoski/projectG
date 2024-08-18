using UnityEngine;

[System.Serializable]
public abstract class Room : MonoBehaviour
{
    public int cost;
    public enum RoomType
    {
        none,
        empty,
        rock,
        core,
        spawner,
        trap,
        tower,
        mining,
        lair,
        hazard
    };
    public abstract RoomType roomType();
    public string description;

}
