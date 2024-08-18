using Breeze.Core;
using UnityEngine;

public class AIDestination : MonoBehaviour
{
    public enum CharacterType
    {
        monster,
        human,
    }

    [SerializeField] private CharacterType type;

    //public static BreezeWaypoint coreWaypoint;

    private void Start()
    {
        if (type == CharacterType.human)
        {
            var coreWaypoint = new BreezeWaypoint();
            //coreWaypoint = 
            GetComponent<BreezeSystem>().Waypoints.Add(coreWaypoint);
        }
    }
}
