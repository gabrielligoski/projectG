using Breeze.Core;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : MonoBehaviour
{

    public enum CharacterType
    {
        monster,
        human,
    }

    [SerializeField] public CharacterType type;

    public float walkSpeed = 2; 
    public float runSpeed = 2; 


    public float damage = 30f;

    public int maxHealth = 50;
    public RoomSpawner spawn;

    public List<Effect> effects;

    private BreezeSystem bs; 

    private void OnDestroy()
    {
        spawn.spawns.Remove(gameObject);
    }

    public BreezeWaypoint waypoint;

    public void updateBreeze() {
        var bs = GetComponent<BreezeSystem>();
        bs.RunSpeed = runSpeed;
        bs.WalkSpeed = walkSpeed;
        bs.MaximumHealth = maxHealth;
    }

    public void applyEffect(Effect effect) {
        effect.Apply(this);
        effects.Add(effect);
    }

    public void removeEffect(Effect effect) { 
        effect.Remove(this);
        effects.Remove(effect);
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
                break;
            case CharacterType.human:
                bs.Waypoints.Add(Core.core.bWaypoint);
                break;
        }
    }
}
