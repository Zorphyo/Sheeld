using Core.Interfaces;
using UnityEngine;

// Tracks the enemy's health and decides DBNO vs death.
public class EnemyHealth : MonoBehaviour, IDamageable
{
    public int maxHealth = 100;
    private int currentHealth;

    private EnemyAnimator enemyAnimator;
    private EnemyMovement movement;
    private EnemyCombat enemyCombat;

    // Set by WaveManager or MedicManager
    public bool medicPresent = false;

    void Start()
    {
        currentHealth = maxHealth;
        enemyAnimator = GetComponent<EnemyAnimator>();
        movement = GetComponent<EnemyMovement>();
        enemyCombat = GetComponent<EnemyCombat>();

        Debug.Log(gameObject.name + " spawned with health: " + currentHealth);
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;

        Debug.Log(gameObject.name + " took " + amount + " damage, health now: " + currentHealth);

        if (currentHealth <= 0)
        {
            HandleZeroHealth();
        }
    }

    void HandleZeroHealth()
    {
        if (medicPresent)
        {
            EnterDBNO();
        }
        else
        {
            Die();
        }
    }

    void EnterDBNO()
    {
        if (movement != null)
            movement.StopMoving();

        enemyAnimator.EnterDBNO();
    }

    void Die()
    {
        Debug.Log(gameObject.name + " died!");

        if (enemyAnimator != null)
            enemyAnimator.Die();

        if (movement != null)
            movement.StopMoving();

        Destroy(gameObject, 3f);
    }
}