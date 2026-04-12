using UnityEngine;
using Core.Interfaces;

[RequireComponent(typeof(EnemyHealth))]
[RequireComponent(typeof(EnemyAnimator))]
public class SpeedsterCombat : MonoBehaviour
{
    [Header("Attack Settings")]
    public int   damage          = 15;
    public float attackCooldown  = 2f;
    public float damageDelay     = 0.2f;   // short delay — fast strike

    private float     lastAttackTime = -99f;
    private bool      justAttacked;
    private Transform player;
    private Animator  animator;
    private EnemyHealth health;

    private static readonly int ParamAttack = Animator.StringToHash("Attack");

    // Read by SpeedsterBrain to know when to retreat
    public bool JustAttacked
    {
        get
        {
            if (justAttacked) { justAttacked = false; return true; }
            return false;
        }
    }

    void Awake()
    {
        animator = GetComponent<Animator>();
        health   = GetComponent<EnemyHealth>();
    }

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    public void TryAttack()
    {
        if (player == null)                              return;
        if (Time.time < lastAttackTime + attackCooldown) return;

        lastAttackTime = Time.time;
        justAttacked   = true;

        animator.SetTrigger(ParamAttack);
        Invoke(nameof(DealDamage), damageDelay);

        if (DirectorAI.Instance != null)
            DirectorAI.Instance.OnEnemyAttacked();
    }

    void DealDamage()
    {
        if (player == null || health.isDead) return;

        IDamageable damageable = player.GetComponent<IDamageable>();
        damageable?.TakeDamage(damage);

        if (DirectorAI.Instance != null)
            DirectorAI.Instance.OnPlayerHit();
    }
}