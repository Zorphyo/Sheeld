using UnityEngine;

// High-level AI controller: decides each frame whether the enemy should attack, chase, or idle.
// Delegates actual movement and combat to EnemyMovement and EnemyCombat respectively.
public class EnemyBrain : MonoBehaviour
{
    [Header("Detection")]
    [HideInInspector] public Transform player;
    public float detectionRange = 100f; // Outer range at which the enemy begins chasing
    public float attackRange = 10f;   // Inner range at which the enemy stops and attacks

    private EnemyMovement movement;
    private EnemyCombat combat;

    void Awake()
    {
        movement = GetComponent<EnemyMovement>();
        combat = GetComponent<EnemyCombat>();
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

void Update()
{
    if (player == null) return;

    float distance = Vector3.Distance(transform.position, player.position);
    
    Debug.Log($"[EnemyBrain] Distance to player: {distance:F2} | AttackRange: {attackRange}");

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
}