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

    public float walkSpeed; 
    public float runSpeed; 


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
        Debug.Log("slow aplicado");
        effect.Apply(this);
        effects.Add(effect);
        updateBreeze();
    }

    public void removeEffect(Effect effect) {
        Debug.Log("slow removido");
        effect.Remove(this);
        effects.Remove(effect);
        updateBreeze();

    }

    private void Start()
    {
        runSpeed = 5;
        walkSpeed = 5;
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
