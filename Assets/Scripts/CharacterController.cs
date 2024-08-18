using Breeze.Core;
using UnityEngine;

public class CharacterController : MonoBehaviour
{
    public enum CharacterType
    {
        monster,
        human,
    }

    [SerializeField] private CharacterType type;
    public BreezeWaypoint waypoint;

    private void Start()
    {
        switch (type)
        {
            case CharacterType.monster:
                GetComponent<BreezeSystem>().Waypoints.Add(waypoint);
                break;
            case CharacterType.human:
                GetComponent<BreezeSystem>().Waypoints.Add(Core.core.bWaypoint);
                break;
        }
    }
}
