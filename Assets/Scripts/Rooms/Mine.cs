using System.Collections;
using UnityEngine;

public class Mine : Room
{

    public int rate = 5;
    public override RoomType roomType()
    {
        return RoomType.mining;
    }

    IEnumerator generateResource(int rate) {
        for(; ; )
        {
            GameMaster.Instance.increaseResource(rate);
            Debug.Log("generate!");
            yield return new WaitForSeconds(1);
        }
    }

    private void Start()
    {
        StartCoroutine(generateResource(rate));
    }

}
