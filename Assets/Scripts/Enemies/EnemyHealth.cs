using Core.Interfaces;
using UnityEngine;

// Tracks the enemy's health, handles incoming damage, and destroys the GameObject on death.
// Call TakeDamage() from any external source (e.g. player weapon, projectile, AoE).
public class EnemyHealth : MonoBehaviour, IDamageable
{
    public int maxHealth = 100;
    private int currentHealth; // Decremented by TakeDamage(); never restored unless a heal system is added

    void Start()
    {
        currentHealth = maxHealth;
        Debug.Log(gameObject.name + " spawned with health: " + currentHealth);
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        Debug.Log(gameObject.name + " took " + amount + " damage, health now: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die(); // Health exhausted — trigger death immediately
        }
    }

    // Destroys the GameObject, which WaveManager detects via the null-check in aliveEnemies
    void Die()
    {
        Debug.Log(gameObject.name + " died!");
        Destroy(gameObject);
    }
}