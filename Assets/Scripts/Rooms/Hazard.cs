using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class Hazard : Room
{
    [SerializeField] private GameObject pfb;
    [SerializeField] private int quantity;

    public override RoomType roomType()
    {
        return name;
    }


    private void Start()
    {
        // +1 enemy for each 10% increase in dificulty
        for (int i = 0; i < quantity + (Math.Abs(GameMaster.Instance.dificulty - 1) * 10); i++)
        {
            var spawnPos = Random.insideUnitCircle;
            Instantiate(pfb, gameObject.transform.position + new Vector3(spawnPos.x, 0, spawnPos.y), Quaternion.identity);
        }
            

    }

}
