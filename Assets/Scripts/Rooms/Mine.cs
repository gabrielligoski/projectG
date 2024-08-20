using UnityEngine;

public class Mine : Room
{
    public override RoomType roomType()
    {
        return RoomType.mining;
    }
}
