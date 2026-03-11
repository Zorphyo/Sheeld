using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class EnemyCombat : MonoBehaviour
{
    [Header("Combat Settings")]
    public int damage = 10;
    public float attackCooldown = 1.5f;
    public float damageDelay = 0.5f;
    [HideInInspector] public Transform player;

    private float lastAttackTime;
    private Animator animator;
    private NavMeshAgent agent;
    private EnemyBrain brain;

    void Awake()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        brain = GetComponent<EnemyBrain>();
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    public void Attack()
    {
        if (player == null) return;
        if (Time.time < lastAttackTime + attackCooldown) return;

        animator.SetTrigger("Attack");
        lastAttackTime = Time.time;
        Invoke(nameof(DealDamage), damageDelay);
    }

    public void StopAttack() { }

    void DealDamage()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        if (distance > brain.attackRange) return; // Uses EnemyBrain as single source of truth

        PlayerStats ps = player.GetComponent<PlayerStats>();
        if (ps != null)
            ps.TakeDamage(damage);
    }
}