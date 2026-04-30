using UnityEngine;
using UnityEngine.AI;
using Core.Interfaces;
public class EnemyHealth : MonoBehaviour, IDamageable
{
    [Header("Health")]
    public int maxHealth = 100;

    [Header("DBNO")]
    public float bleedoutTime = 10f;
    public float reviveHealthPct = 0.5f;
    public bool canEnterDBNO = true;   // uncheck this on the medic prefab

    private float hitCooldown = 0.2f;
    private float lastHitTime = -1f;
    private int currentHealth;

    private EnemyAnimator enemyAnimator;
    private EnemyMovement movement;
    private EnemyRagdollController ragdollController;

    [HideInInspector] public bool medicPresent = false;
    [HideInInspector] public bool isDead = false;
    [HideInInspector] public bool isDBNO = false;

    // Bleedout
    private float bleedoutTimer;
    private bool bleedoutActive;

    // UI Health Bar
    [Header("Health Bar")]
    public GameObject healthBarPrefab;
    public Transform headPoint;
    private EnemyHealthBar healthBarUI;

    void OnEnable()
    {
        currentHealth = maxHealth;
        enemyAnimator = GetComponent<EnemyAnimator>();
        movement = GetComponent<EnemyMovement>();
        ragdollController = GetComponent<EnemyRagdollController>();

        if (MedicManager.Instance != null)
            MedicManager.Instance.RegisterEnemy(this);

        if (headPoint == null)
        {
            Transform found = transform.Find("HeadObject");
            if (found != null)
                headPoint = found;
        }

        if (healthBarPrefab != null)
        {
            GameObject hb = Instantiate(healthBarPrefab);

            healthBarUI = hb.GetComponent<EnemyHealthBar>();

            var follow = hb.GetComponent<EnemyHealthFollow>();
            follow.target = headPoint != null ? headPoint : transform;

            healthBarUI.SetHealthBar(currentHealth, maxHealth);

            hb.SetActive(false);
        }
    }

    void OnDestroy()
    {
        if (MedicManager.Instance != null)
            MedicManager.Instance.UnregisterEnemy(this);
    }

    void Update()
    {
        TickBleedout();
    }

    // ── Damage ────────────────────────────────────────────────────────────────
    public void TakeDamage(int amount)
    {
        if (isDead || isDBNO) return;
        if (amount <= 0) return;

        currentHealth -= amount;

        healthBarUI?.SetHealthBar(currentHealth, maxHealth);
        if (healthBarUI != null)
        {
            healthBarUI.gameObject.SetActive(currentHealth < maxHealth);
        }

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ReportDamage(amount);
        }
        Debug.Log($"{gameObject.name} took {amount} damage, health now: {currentHealth}");

        if (currentHealth <= 0)
        {
            HandleZeroHealth();
            return;
        }

        if (enemyAnimator != null && Time.time >= lastHitTime + hitCooldown)
        {
            enemyAnimator.TakeDamage();
            lastHitTime = Time.time;
        }
    }

    // In EnemyHealth — replace HandleZeroHealth
    void HandleZeroHealth()
    {
        bool medicAlive = MedicManager.Instance != null && MedicManager.Instance.IsMedicAlive;

        Debug.Log($"[EnemyHealth] {gameObject.name} hit zero. medicAlive: {medicAlive}, canEnterDBNO: {canEnterDBNO}");

        if (medicAlive && canEnterDBNO)
            EnterDBNO();
        else
            Die();
    }

    // ── DBNO ──────────────────────────────────────────────────────────────────
    void EnterDBNO()
    {
        isDBNO = true;
        bleedoutTimer = bleedoutTime;
        bleedoutActive = true;

        movement?.StopMoving();
        enemyAnimator?.EnterDBNO();

        Debug.Log($"{gameObject.name} entered DBNO — bleedout in {bleedoutTime}s");
    }

    void TickBleedout()
    {
        if (!bleedoutActive || !isDBNO) return;

        bleedoutTimer -= Time.deltaTime;

        if (bleedoutTimer <= 0f)
        {
            Debug.Log($"{gameObject.name} bled out.");
            Die();
        }
    }

    // Called by MedicAI when it reaches this enemy
    public void ReceiveRevive()
    {
        if (!isDBNO || isDead) return;

        isDBNO = false;
        bleedoutActive = false;
        currentHealth = Mathf.RoundToInt(maxHealth * reviveHealthPct);

        enemyAnimator?.Revive();

        healthBarUI?.SetHealthBar(currentHealth, maxHealth);

        if (healthBarUI != null)
        {
            healthBarUI.gameObject.SetActive(currentHealth < maxHealth);
        }

        Debug.Log($"{gameObject.name} revived at {currentHealth} hp");
    }

    // ── Death ─────────────────────────────────────────────────────────────────
    void Die()
    {
        if (isDead) return;

        isDead = true;
        bleedoutActive = false;

        Debug.Log($"{gameObject.name} died.");

        if (healthBarUI != null)
        {
            Destroy(healthBarUI.gameObject);
        }

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ReportEnemyDeath(gameObject.name);
        }

        // Kill agent immediately so it stops fighting the death animation
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        /* Replaced with ragdoll
        // Stop rigidbody interference if one exists
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null && !rb.isKinematic)
        {
            rb.linearVelocity = Vector3.zero;  // zero it first
            rb.isKinematic = true;          // then lock it
        }

        
        enemyAnimator?.Die();
        movement?.StopMoving();
        Destroy(gameObject, 3f);
        */

        movement?.StopMoving();

        if (ragdollController != null)
        {
            ragdollController.DieAsRagdoll();
        }
        else
        {
            enemyAnimator?.Die();
        }

        Destroy(gameObject, 8f);
    }
}