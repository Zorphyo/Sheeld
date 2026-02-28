using UnityEngine;

// High-level AI controller: decides each frame whether the enemy should attack, chase, or idle.
// Delegates actual movement and combat to EnemyMovement and EnemyCombat respectively.
public class EnemyBrain : MonoBehaviour
{
    [Header("Detection")]
    public Transform player;
    public float detectionRange = 10f; // Outer range at which the enemy begins chasing
    public float attackRange = 1.5f;   // Inner range at which the enemy stops and attacks

    private EnemyMovement movement;
    private EnemyCombat combat;

    void Awake()
    {
        movement = GetComponent<EnemyMovement>();
        combat = GetComponent<EnemyCombat>();
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= attackRange)
        {
            // Player is close enough to hit — stop moving and swing
            movement.StopMoving();
            combat.Attack();
        }
        else if (distance <= detectionRange)
        {
            // Player is in sight but out of melee range — chase
            combat.StopAttack();
            movement.MoveTo(player.position);
        }
        else
        {
            // Player is out of range — stand idle
            movement.StopMoving();
            combat.StopAttack();
        }
    }
}