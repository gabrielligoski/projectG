using System.Collections;
using UnityEngine;

public class Mine : Room
{

    
    public override RoomType roomType()
    {
        return RoomType.mining;
    }

    IEnumerator generateResource(int rate, int ratio) {
        for(; ; )
        {
            GameMaster.Instance.increaseResource(ratio);
            Debug.Log("generate!");
            yield return new WaitForSeconds(rate);
        }
    }

    private void Start()
    {
        StartCoroutine(generateResource(GameMaster.Instance.mineRate, GameMaster.Instance.mineRatio));
    }

}
