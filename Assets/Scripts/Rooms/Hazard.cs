using System;
using UnityEngine;

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
            Instantiate(pfb, gameObject.transform.position, Quaternion.identity);

    }

}
