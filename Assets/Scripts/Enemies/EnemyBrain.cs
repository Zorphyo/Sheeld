using UnityEngine;
using UnityEngine.AI;

public class EnemyBrain : MonoBehaviour
{
    [Header("Detection")]
    [HideInInspector] public Transform player;
    public float detectionRange = 100f;
    public float attackRange = 10f;

    [Header("Movement")]
    public float moveSpeed = 20f;

    [Header("Flanking")]
    [Range(0f, 1f)]
    public float flankChance = 0.4f;
    public float flankAngleMin = 60f;
    public float flankAngleMax = 120f;
    public float flankRadius = 8f;
    public float flankRePickTime = 4f;

    [Header("Trap Avoidance")]
    [Range(0f, 1f)]
    public float trapAvoidChance = 0.7f;
    public float trapDetectRadius = 3f;
    public float trapDetectDistance = 4f;
    public float avoidOffset = 4f;

    [Header("NavMesh Spawn")]
    public float navMeshSampleRadius = 5f;

    private EnemyMovement movement;
    private EnemyCombat combat;
    private EnemyHealth health;
    private NavMeshAgent agent;

    private bool isFlanking;
    private float flankTimer;
    private Vector3 flankDestination;

    // ── Unity lifecycle ───────────────────────────────────────────────────────
    void Awake()
    {
        movement = GetComponent<EnemyMovement>();
        combat = GetComponent<EnemyCombat>();
        health = GetComponent<EnemyHealth>();
        agent = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
        SnapToNavMesh();
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        DecideRole();
    }

    void Update()
    {
        if (player == null) return;
        if (health != null && (health.isDead || health.isDBNO)) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= attackRange)
        {
            movement.StopMoving();
            combat.Attack();
            return;
        }

        if (distance <= detectionRange)
        {
            combat.StopAttack();

            Vector3 destination = isFlanking
                ? GetFlankDestination()
                : player.position;

            destination = ApplyTrapAvoidance(destination);
            movement.MoveTo(destination);
        }
        else
        {
            movement.StopMoving();
            combat.StopAttack();
        }
    }

    // ── Role assignment ───────────────────────────────────────────────────────
    void DecideRole()
    {
        int aliveCount = DirectorAI.Instance != null
            ? DirectorAI.Instance.Roster.TotalCount
            : 0;

        isFlanking = aliveCount >= 3 && Random.value < flankChance;
        flankTimer = 0f;
    }

    public void ReevaluateRole() => DecideRole();

    // ── Flank destination ─────────────────────────────────────────────────────
    Vector3 GetFlankDestination()
    {
        flankTimer -= Time.deltaTime;

        if (flankTimer <= 0f)
        {
            flankTimer = flankRePickTime;
            flankDestination = PickFlankPoint();
        }

        return flankDestination;
    }

    Vector3 PickFlankPoint()
    {
        float angle = Random.Range(flankAngleMin, flankAngleMax);
        if (Random.value > 0.5f) angle = -angle;

        Vector3 dirToEnemy = (transform.position - player.position).normalized;
        Vector3 flankDir = Quaternion.Euler(0f, angle, 0f) * dirToEnemy;
        Vector3 candidatePos = player.position + flankDir * flankRadius;

        if (NavMesh.SamplePosition(candidatePos, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            return hit.position;

        return player.position;
    }

    // ── Trap avoidance ────────────────────────────────────────────────────────
    Vector3 ApplyTrapAvoidance(Vector3 intendedDestination)
    {
        Vector3 dirToTarget = (intendedDestination - transform.position).normalized;
        Vector3 checkOrigin = transform.position + dirToTarget * trapDetectDistance;
        Collider[] hits = Physics.OverlapSphere(checkOrigin, trapDetectRadius);

        foreach (Collider col in hits)
        {
            if (!col.CompareTag("Trap")) continue;
            if (Random.value > trapAvoidChance) continue;

            Vector3 avoidDir = Vector3.Cross(dirToTarget, Vector3.up).normalized;
            if (Random.value > 0.5f) avoidDir = -avoidDir;

            Vector3 avoidTarget = transform.position + avoidDir * avoidOffset;

            if (NavMesh.SamplePosition(avoidTarget, out NavMeshHit navHit, 5f, NavMesh.AllAreas))
                return navHit.position;
        }

        return intendedDestination;
    }

    // ── Utility ───────────────────────────────────────────────────────────────
    void SnapToNavMesh()
    {
        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit,
                navMeshSampleRadius, NavMesh.AllAreas))
            transform.position = hit.position;
        else
            Debug.LogWarning($"[EnemyBrain] {gameObject.name} could not snap to NavMesh within radius {navMeshSampleRadius}.");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(
            transform.position + transform.forward * trapDetectDistance,
            trapDetectRadius);
    }
}