using Breeze.Core;
using UnityEngine;

public class Core : Room
{
    public float life = 100f;
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

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.TryGetComponent<CharacterController>(out CharacterController e)) {
            if(e.type == CharacterController.CharacterType.human)
            {
                takeDamage(e.damage);
                Destroy(e.gameObject);
            }
        }
    }

    public void takeDamage(float damageAmount) 
    { 
        life -= damageAmount;
        if(life <= 0)
        {
            Destroy(gameObject);
        }
    }

}
