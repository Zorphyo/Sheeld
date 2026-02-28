using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class EnemyCombat : MonoBehaviour
{
    [Header("Combat Settings")]
    public int damage = 10;
    public float attackCooldown = 1.5f;
    public float attackRange = 2f;         // How close the enemy must be to attack
    public float damageDelay = 0.5f;       // Time in animation when hit should land
    public Transform player;               // Assign Player Transform in Inspector

    private float lastAttackTime;
    private Animator animator;
    private NavMeshAgent agent;

    void Awake()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        // Safety: check player still exists
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance > attackRange)
        {
            // Chase player if too far
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }
        else
        {
            // Stop moving and attack
            agent.isStopped = true;

            if (Time.time >= lastAttackTime + attackCooldown)
            {
                Attack();
                lastAttackTime = Time.time;
            }
        }
    }

    /// <summary>
    /// Called by EnemyBrain or internal logic
    /// </summary>
    public void Attack()
    {
        if (player == null) return;

        animator.SetTrigger("Attack");

        // Apply damage at the correct frame of the animation
        //Invoke(nameof(DealDamage), damageDelay);
    }

    /// <summary>
    /// Stop attack placeholder for EnemyBrain
    /// </summary>
    public void StopAttack()
    {
        // Currently nothing to cancel, but safe to call
        // Could later stop attack animation or cancel coroutines if needed
    }

    /// <summary>
    /// Deals damage to the player if still in range
    /// </summary>
    /*void DealDamage()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= attackRange)
        {
            Health playerHealth = player.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                Debug.Log($"{name} hit player! Current damage: {damage}");
            }
        }
    }*/

    /// <summary>
    /// Draws attack range in Scene view for debugging
    /// </summary>
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
