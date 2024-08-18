using Unity.APIComparison.Framework.Changes;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float damage = 30f;
    public RoomSpawner spawn;

    private void OnDestroy()
    {
        spawn.spawns.Remove(gameObject);
    }
}
