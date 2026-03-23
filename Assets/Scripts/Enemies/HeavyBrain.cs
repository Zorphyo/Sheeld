using UnityEngine;
using UnityEngine.AI;

public class HeavyBrain : MonoBehaviour
{
    [Header("Detection")]
    [HideInInspector] public Transform player;
    public float detectionRange      = 80f;
    public float spinRange           = 9f;   // must match HeavyCombat.spinRange
    public float moveSpeed           = 10f;  // slower than basic

    [Header("NavMesh Spawn")]
    public float navMeshSampleRadius = 5f;

    private EnemyMovement movement;
    private HeavyCombat   combat;
    private EnemyHealth   health;
    private NavMeshAgent  agent;

    void Awake()
    {
        movement    = GetComponent<EnemyMovement>();
        combat      = GetComponent<HeavyCombat>();
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

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= spinRange)
        {
            // In attack range — stop and let HeavyCombat decide which attack
            movement.StopMoving();
            combat.EvaluateAttack();
        }
        else if (distance <= detectionRange)
        {
            // Chase player
            movement.MoveTo(player.position);
        }
        else
        {
            movement.StopMoving();
        }
    }

    void SnapToNavMesh()
    {
        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit,
                navMeshSampleRadius, NavMesh.AllAreas))
            transform.position = hit.position;
        else
            Debug.LogWarning($"[HeavyBrain] {gameObject.name} could not snap to NavMesh.");
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, spinRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}