using UnityEngine;
using UnityEngine.AI;

public class SpeedsterBrain : MonoBehaviour
{
    [Header("Detection")]
    [HideInInspector] public Transform player;
    public float detectionRange      = 120f;
    public float attackRange         = 3f;

    [Header("Movement")]
    public float moveSpeed           = 38f;
    public float retreatSpeed        = 42f;
    public float retreatDistance     = 12f;
    public float retreatDuration     = 1.5f;

    [Header("Back Attack")]
    public float circleRadius        = 6f;
    public float backAngleThreshold  = 130f;
    public float repositionSpeed     = 38f;
    public float steeringStrength    = 0.6f;  // 0 = straight at player, 1 = pure orbit

    [Header("NavMesh Spawn")]
    public float navMeshSampleRadius = 5f;

    private EnemyMovement   movement;
    private SpeedsterCombat combat;
    private EnemyHealth     health;
    private NavMeshAgent    agent;

    private SpeedsterState currentState = SpeedsterState.Repositioning;
    private float          retreatTimer;
    private Vector3        retreatDestination;

    public enum SpeedsterState { Repositioning, Attacking, Retreating }

    // ── Unity lifecycle ───────────────────────────────────────────────────────
    void Awake()
    {
        movement    = GetComponent<EnemyMovement>();
        combat      = GetComponent<SpeedsterCombat>();
        health      = GetComponent<EnemyHealth>();
        agent       = GetComponent<NavMeshAgent>();
        agent.speed = moveSpeed;
        SnapToNavMesh();
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (player == null) return;
        if (health != null && (health.isDead || health.isDBNO)) return;

        switch (currentState)
        {
            case SpeedsterState.Repositioning: TickRepositioning(); break;
            case SpeedsterState.Attacking:     TickAttacking();     break;
            case SpeedsterState.Retreating:    TickRetreating();    break;
        }
    }

    // ── Repositioning — sprint at player, steer toward back ───────────────────
    void TickRepositioning()
    {
        float dist = Vector3.Distance(transform.position, player.position);

        if (dist > detectionRange)
        {
            movement.StopMoving();
            return;
        }

        // Already behind and close enough — commit to attack
        if (IsBehindPlayer() && dist <= attackRange * 3f)
        {
            currentState = SpeedsterState.Attacking;
            agent.speed  = moveSpeed;
            return;
        }

        // Blend between sprinting at player and steering toward their back
        // Far away = mostly direct, close = more arc toward back
        Vector3 dirToPlayer  = (player.position - transform.position).normalized;
        Vector3 behindTarget = player.position + (-player.forward * circleRadius);
        Vector3 dirToBehind  = (behindTarget - transform.position).normalized;

        float   t       = Mathf.Clamp01(1f - (dist / detectionRange));
        Vector3 moveDir = Vector3.Lerp(dirToPlayer, dirToBehind,
                                        Mathf.Lerp(steeringStrength, 1f, t)).normalized;

        Vector3 destination = transform.position + moveDir * 3f;

        if (NavMesh.SamplePosition(destination, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            agent.SetDestination(hit.position);

        agent.speed = repositionSpeed;
    }

    // ── Attacking — close in and strike ───────────────────────────────────────
    void TickAttacking()
    {
        float dist = Vector3.Distance(transform.position, player.position);

        if (dist <= attackRange)
        {
            movement.StopMoving();
            combat.TryAttack();

            if (combat.JustAttacked)
                EnterRetreat();

            return;
        }

        // Still closing in — keep sprinting
        agent.speed = moveSpeed;
        movement.MoveTo(player.position);

        // Only abort if we're well off the back arc and not close to attacking
        if (!IsBehindPlayer() && dist > attackRange * 4f)
            currentState = SpeedsterState.Repositioning;
    }

    // ── Retreating — run away after hitting ───────────────────────────────────
    void TickRetreating()
    {
        retreatTimer -= Time.deltaTime;

        float dist = Vector3.Distance(transform.position, retreatDestination);

        if (dist > 1f)
        {
            agent.speed = retreatSpeed;
            movement.MoveTo(retreatDestination);
        }

        if (retreatTimer <= 0f)
        {
            currentState = SpeedsterState.Repositioning;
            agent.speed  = repositionSpeed;
        }
    }

    void EnterRetreat()
    {
        currentState = SpeedsterState.Retreating;
        retreatTimer = retreatDuration;

        Vector3 awayDir       = (transform.position - player.position).normalized;
        Vector3 retreatTarget = transform.position + awayDir * retreatDistance;

        if (NavMesh.SamplePosition(retreatTarget, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            retreatDestination = hit.position;
        else
            retreatDestination = transform.position;
    }

    // ── Back position math ────────────────────────────────────────────────────
    bool IsBehindPlayer()
    {
        Vector3 playerForward  = player.forward;
        Vector3 dirToSpeedster = (transform.position - player.position).normalized;
        float   angle          = Vector3.Angle(playerForward, dirToSpeedster);
        return angle >= backAngleThreshold;
    }

    // ── Utility ───────────────────────────────────────────────────────────────
    void SnapToNavMesh()
    {
        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit,
                navMeshSampleRadius, NavMesh.AllAreas))
            transform.position = hit.position;
        else
            Debug.LogWarning($"[SpeedsterBrain] {gameObject.name} could not snap to NavMesh.");
    }

    void OnDrawGizmosSelected()
    {
        if (player == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(player.position - player.forward * circleRadius, 0.4f);
    }
}