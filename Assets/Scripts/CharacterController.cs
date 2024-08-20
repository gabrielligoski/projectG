using Breeze.Core;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : MonoBehaviour
{

    public enum CharacterType
    {
        monster,
        enemy
    }

    [SerializeField] public CharacterType type;

    public float walkSpeed = 1; 
    public float runSpeed = 1;

    public int xpValue;

    public float damageToCore = 30f;

    public Spawner spawn;
    public List<Effect> effects;
    private BreezeSystem bs;
    public BreezeWaypoint waypoint;


    private void OnDestroy()
    {
        if (spawn)
            spawn.spawns.Remove(gameObject);
    }


    public void onDeath() {
        GameMaster.Instance.addXP(xpValue);
    }

    public void updateBreeze() {
        var bs = GetComponent<BreezeSystem>();
        bs.RunSpeed = runSpeed;
        bs.WalkSpeed = walkSpeed;
    }

    public void takeDamage(float amount) {
        switch (type)
        {
            case CharacterType.monster:
                bs.Waypoints.Add(waypoint);
                bs.Waypoints.Add(waypoint);
                break;
            case CharacterType.enemy:
                bs.Waypoints.Add(Core.instance.bWaypoint);
                bs.Waypoints.Add(Core.instance.bWaypoint);
                break;
        }
        //GetComponent<BreezeSystem>().TakeDamage(amount, gameObject, true, false);
    }

    public void applyEffect(Effect effect)
    {
        Debug.Log("slow aplicado");
        effect.Apply(this);
        if(!effects.Contains(effect))
            effects.Add(effect);
        updateBreeze();
    }

    public void removeEffect(Effect effect)
    {
        Debug.Log("slow removido");
        effect.Remove(this);
        effects.Remove(effect);
        updateBreeze();

    }

    private void Start()
    {
        bs = GetComponent<BreezeSystem>();
        updateBreeze();
        effects = new List<Effect>();
        switch (type)
        {
            case CharacterType.monster:
                bs.Waypoints.Add(waypoint);
                bs.Waypoints.Add(waypoint);
                break;
            case CharacterType.enemy:
                bs.Waypoints.Add(Core.instance.bWaypoint);
                bs.Waypoints.Add(Core.instance.bWaypoint);
                break;
        }
    }
}
