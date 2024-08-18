using UnityEngine;

public class Empty : Room
{
    public override RoomType roomType()
    {
        return RoomType.empty;
    }
}
