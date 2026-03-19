using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyHealth))]
public class MedicAI : MonoBehaviour
{
    private Animator animator;   // ← inside the class — correct
    private static readonly int ParamIsHealing = Animator.StringToHash("isHealing");

    [Header("Revive Settings")]
    public float reviveRadius = 3f;
    public float reviveTickRate = 0.5f;

    // Add this field at the top of MedicAI
    private float reviveAnimDuration = 2f; // match this to your healing clip length
    private float reviveAnimTimer;
    private bool reviveAnimPlaying;

    [Header("Flee Settings")]
    public float fleeRadius = 12f;   // how far to try to stay from player
    public float fleeUpdateRate = 1f;    // how often to recalculate flee destination
    public float behindEnemyOffset = 2f;    // how far behind the chosen shield enemy to stand

    private NavMeshAgent agent;
    private EnemyHealth health;
    private Transform player;

    private EnemyHealth currentTarget;
    private bool isReviving;
    private float reviveCheckTimer;
    private float fleeUpdateTimer;

    [Header("Trap Awareness")]
    public float trapCheckRadius = 4f;   // how far to check around the DBNO enemy
    public float safePosSearchRadius = 6f; // how far to search for a safe healing spot
    public LayerMask trapLayer;            // set this to your trap layer in Inspector

    // ── Unity lifecycle ───────────────────────────────────────────────────────
    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<EnemyHealth>();
        animator = GetComponent<Animator>();

        if (MedicManager.Instance != null)
            MedicManager.Instance.RegisterMedic(this);
    }

    void OnDisable()
    {
        if (MedicManager.Instance != null)
            MedicManager.Instance.UnregisterMedic(this);
    }

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    void Update()
    {
        if (health.isDead || player == null) return;

        if (isReviving)
        {
            UpdateRevive();
        }
        else
        {
            FindTarget();

            if (currentTarget != null)
                MoveToRevive();
            else
                UpdateFlee();
        }
    }

    // ── Revive ────────────────────────────────────────────────────────────────
    void FindTarget()
    {
        if (MedicManager.Instance == null) return;

        // Only seek a new target if we don't already have one
        if (currentTarget != null && currentTarget.isDBNO) return;

        currentTarget = MedicManager.Instance.GetClosestDBNOEnemy(transform.position);
    }

    void MoveToRevive()
    {
        if (currentTarget == null || !currentTarget.isDBNO)
        {
            currentTarget = null;
            return;
        }

        // Find a safe spot near the DBNO enemy rather than standing on top of them
        Vector3 safeHealPos = FindSafeHealPosition(currentTarget.transform.position);
        float dist = Vector3.Distance(transform.position, safeHealPos);

        if (dist <= reviveRadius)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
            isReviving = true;
        }
        else
        {
            agent.isStopped = false;
            agent.SetDestination(safeHealPos);
        }
    }

    void UpdateRevive()
    {
        if (currentTarget == null || !currentTarget.isDBNO)
        {
            isReviving = false;
            reviveAnimPlaying = false;
            currentTarget = null;
            agent.isStopped = false;
            animator.SetBool(ParamIsHealing, false);
            return;
        }

        // First entry — start the animation
        if (!reviveAnimPlaying)
        {
            reviveAnimPlaying = true;
            reviveAnimTimer = reviveAnimDuration;
            animator.SetBool(ParamIsHealing, true);
            return;
        }

        // Count down the animation
        reviveAnimTimer -= Time.deltaTime;
        if (reviveAnimTimer > 0f) return;

        // Animation finished — actually revive
        animator.SetBool(ParamIsHealing, false);
        currentTarget.ReceiveRevive();
        isReviving = false;
        reviveAnimPlaying = false;
        currentTarget = null;
        agent.isStopped = false;
    }

    // ── Flee ──────────────────────────────────────────────────────────────────
    void UpdateFlee()
    {
        fleeUpdateTimer -= Time.deltaTime;
        if (fleeUpdateTimer > 0f) return;
        fleeUpdateTimer = fleeUpdateRate;

        Vector3 fleeDestination = FindFleePosition();
        agent.SetDestination(fleeDestination);
    }

    Vector3 FindFleePosition()
    {
        // Try to get behind a living enemy — use them as a shield
        GameObject shield = FindShieldEnemy();

        if (shield != null)
        {
            // Position ourselves on the far side of the shield enemy from the player
            Vector3 playerToShield = (shield.transform.position - player.position).normalized;
            Vector3 candidate = shield.transform.position + playerToShield * behindEnemyOffset;

            if (NavMesh.SamplePosition(candidate, out NavMeshHit shieldHit, 5f, NavMesh.AllAreas))
                return shieldHit.position;
        }

        // Fallback — run directly away from player
        Vector3 fleeDir = (transform.position - player.position).normalized;
        Vector3 fleeCandidate = transform.position + fleeDir * fleeRadius;

        if (NavMesh.SamplePosition(fleeCandidate, out NavMeshHit fleeHit, 5f, NavMesh.AllAreas))
            return fleeHit.position;

        return transform.position;
    }

    GameObject FindShieldEnemy()
    {
        if (DirectorAI.Instance == null) return null;

        GameObject bestShield = null;
        float bestScore = float.MinValue;

        foreach (GameObject enemy in DirectorAI.Instance.Roster.LiveEnemies)
        {
            // Skip nulls, self, and medics
            if (enemy == null) continue;
            if (enemy == gameObject) continue;
            if (enemy.GetComponent<MedicAI>() != null) continue;

            EnemyHealth eh = enemy.GetComponent<EnemyHealth>();
            if (eh == null || eh.isDead || eh.isDBNO) continue;

            // Score = how well this enemy sits between us and the player
            // Higher score = better shield
            Vector3 toEnemy = (enemy.transform.position - player.position).normalized;
            Vector3 toMedic = (transform.position - player.position).normalized;
            float alignment = Vector3.Dot(toEnemy, toMedic);

            // Prefer enemies that are far from the player
            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            float score = alignment + (dist * 0.05f);

            if (score > bestScore)
            {
                bestScore = score;
                bestShield = enemy;
            }
        }

        return bestShield;
    }

    bool IsTrapNearby(Vector3 position)
    {
        Collider[] hits = Physics.OverlapSphere(position, trapCheckRadius);
        foreach (Collider col in hits)
            if (col.CompareTag("Trap")) return true;
        return false;
    }

    Vector3 FindSafeHealPosition(Vector3 targetPosition)
    {
        // If no trap nearby just return the target position directly
        if (!IsTrapNearby(targetPosition)) return targetPosition;

        // Try positions in a circle around the DBNO enemy to find a trap-free spot
        int attempts = 12;
        float angleStep = 360f / attempts;

        for (int i = 0; i < attempts; i++)
        {
            float angle = i * angleStep;
            Vector3 offset = new Vector3(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                0f,
                Mathf.Sin(angle * Mathf.Deg2Rad)) * safePosSearchRadius;

            Vector3 candidate = targetPosition + offset;

            if (IsTrapNearby(candidate)) continue;

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 3f, NavMesh.AllAreas))
                return hit.position;
        }

        // No safe spot found — return original and hope for the best
        Debug.LogWarning("[MedicAI] No safe healing position found near DBNO enemy.");
        return targetPosition;
    }

    // Called externally if needed
    public void Die()
    {
        // OnDisable handles UnregisterMedic automatically
        Destroy(gameObject, 3f);
    }
}