using UnityEngine;
using UnityEngine.TextCore.Text;

public class Health : MonoBehaviour
{
    public float CurrentHealth;
    public float maxHealth = 100;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CurrentHealth = maxHealth;
    }
    // Update is called once per frameS
    void takeDamage(float damageAmount)
    {
        CurrentHealth -= damageAmount;
        if (CurrentHealth <= 0)
        {
            Destroy(gameObject);
        }
    }
}
