using Breeze.Core;
using UnityEngine;

public class Core : Room
{
    public float life = 100f;
    public float maxLife = 100f;
    public override RoomType roomType()
    {
        return RoomType.core;
    }
    private PlayerHUD playerHUD = null;

    public static Core instance;
    public BreezeWaypoint bWaypoint;


    private void Awake()
    {
        instance = this;
        playerHUD = PlayerHUD.Instance;
        playerHUD.UpdateLifeBar(life, maxLife);
        bWaypoint = GetComponent<BreezeWaypoint>();
        bWaypoint.NextWaypoint = bWaypoint.gameObject;
        bWaypoint.MaxIdleLength = int.MaxValue;
        bWaypoint.MinIdleLength = int.MaxValue;
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.TryGetComponent<CharacterController>(out CharacterController e)) {
            if(e.type == CharacterController.CharacterType.enemy)
            {
                takeDamage(e.damageToCore);
                Destroy(e.gameObject);
            }
        }
    }

    public void takeDamage(float damageAmount) 
    { 
        life -= damageAmount * 1000000;
        playerHUD.UpdateLifeBar(life, maxLife);
        if(life <= 0)
        {
            Destroy(gameObject);
            playerHUD.showGameOverScreen();
        }
    }

}
