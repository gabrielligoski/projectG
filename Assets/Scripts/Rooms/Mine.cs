using System.Collections;
using UnityEngine;

public class Mine : Room
{


    public override RoomType roomType()
    {
        return RoomType.mining;
    }

    IEnumerator generateResource()
    {
        while (true)
        {
            GameMaster.Instance.increaseResource(GameMaster.Instance.mineRatio);
            yield return new WaitForSeconds(GameMaster.Instance.mineRate);
        }
    }

    private void Start()
    {
        StartCoroutine(generateResource());
    }

}
