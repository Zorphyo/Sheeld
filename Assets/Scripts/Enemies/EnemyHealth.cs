using Core.Interfaces;
using UnityEngine;

// Tracks the enemy's health and decides DBNO vs death.
public class EnemyHealth : MonoBehaviour, IDamageable
{
    public int maxHealth = 100;
    private float hitCooldown = 0.2f;
    private float lastHitTime = -1f;
    private int currentHealth;
    private int previousHealth;
    private EnemyAnimator enemyAnimator;
    private EnemyMovement movement;
    private EnemyCombat enemyCombat;

    [HideInInspector] public bool isDead = false;
    [HideInInspector] public bool isDBNO = false;


    // Set by WaveManager or MedicManager
    public bool medicPresent = false;

    void Start()
    {
        currentHealth = maxHealth;
        previousHealth = maxHealth;
        enemyAnimator = GetComponent<EnemyAnimator>();
        movement = GetComponent<EnemyMovement>();
        enemyCombat = GetComponent<EnemyCombat>();

        Debug.Log(gameObject.name + " spawned with health: " + currentHealth);
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        Debug.Log(gameObject.name + " took " + amount + " damage, health now: " + currentHealth);

        if (enemyAnimator != null && currentHealth < previousHealth && currentHealth > 0)
        {
            if (Time.time >= lastHitTime + hitCooldown)
            {
                enemyAnimator.TakeDamage();
                lastHitTime = Time.time;
            }
        }

        previousHealth = currentHealth;

        if (currentHealth <= 0)
            HandleZeroHealth();
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
        isDBNO = true;
        if (movement != null)
            movement.StopMoving();
        enemyAnimator.EnterDBNO();
    }

    void Die()
    {
        isDead = true;
        Debug.Log(gameObject.name + " died!");
        if (enemyAnimator != null)
            enemyAnimator.Die();
        if (movement != null)
            movement.StopMoving();
        Destroy(gameObject, 3f);
    }
}