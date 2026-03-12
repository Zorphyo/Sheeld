using UnityEngine;
using UnityEngine.AI;

public class EnemyBrain : MonoBehaviour
{
    [Header("Detection")]
    [HideInInspector] public Transform player;
    public float detectionRange = 100f;
    public float attackRange = 10f;

    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("NavMesh Spawn")]
    public float navMeshSampleRadius = 5f;

    private EnemyMovement movement;
    private EnemyCombat combat;
    private EnemyHealth health;

    void Awake()
    {
        movement = GetComponent<EnemyMovement>();
        combat = GetComponent<EnemyCombat>();
        health = GetComponent<EnemyHealth>();
        GetComponent<NavMeshAgent>().speed = moveSpeed;
        SnapToNavMesh();
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void SnapToNavMesh()
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, navMeshSampleRadius, NavMesh.AllAreas))
        {
            transform.position = hit.position;
        }
        else
        {
            Debug.LogWarning($"[EnemyBrain] {gameObject.name} could not snap to NavMesh within radius {navMeshSampleRadius}.");
        }
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
        }
        else if (distance <= detectionRange)
        {
            combat.StopAttack();
            movement.MoveTo(player.position);
        }
        else
        {
            movement.StopMoving();
            combat.StopAttack();
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}