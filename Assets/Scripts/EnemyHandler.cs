using UnityEngine;
using UnityEngine.AI;

public class EnemyHandler : MonoBehaviour
{
    [SerializeField] private string name;
    [SerializeField] private int life;
    [SerializeField] private int damage;
    [SerializeField] private float atkSpd;
    [SerializeField] private float spd;

    private NavMeshAgent agent;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.SetDestination(GameMaster.core.transform.position);
    }

    public void TakeDamage(int damage)
    {
        // TODO call hit animation
        life -= damage;

        // TODO call death animation
        if (life <= 0)
            Destroy(gameObject);
    }

}
