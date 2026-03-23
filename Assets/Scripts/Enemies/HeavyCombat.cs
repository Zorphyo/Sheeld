using UnityEngine;
using Core.Interfaces;

[RequireComponent(typeof(EnemyHealth))]
[RequireComponent(typeof(EnemyAnimator))]
public class HeavyCombat : MonoBehaviour
{
    [Header("Stomp — close range")]
    public float stompRange        = 4f;
    public float stompDamage       = 40f;
    public float stompKnockback    = 12f;
    public float stompCooldown     = 3f;
    public float stompRadius       = 4f;

    [Header("Spin — mid range")]
    public float spinRange         = 9f;
    public float spinDamage        = 25f;
    public float spinRadius        = 6f;
    public float spinCooldown      = 7f;

    [Header("Damage Delay (seconds after animation starts)")]
    public float stompDamageDelay  = 0.4f;
    public float spinDamageDelay   = 0.6f;

    private float       lastStompTime  = -99f;
    private float       lastSpinTime   = -99f;
    private EnemyHealth health;
    private EnemyAnimator enemyAnimator;
    private Transform   player;

    // Animator param IDs
    private static readonly int ParamStomp = Animator.StringToHash("Stomp");
    private static readonly int ParamSpin  = Animator.StringToHash("Spin");

    private Animator animator;

    void Awake()
    {
        health        = GetComponent<EnemyHealth>();
        enemyAnimator = GetComponent<EnemyAnimator>();
        animator      = GetComponent<Animator>();
    }

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
    }

    // Called by HeavyBrain each frame when in attack range
    public void EvaluateAttack()
    {
        if (health.isDead || health.isDBNO) return;
        if (player == null)                 return;

        float dist = Vector3.Distance(transform.position, player.position);

        // Stomp takes priority if player is very close and off cooldown
        if (dist <= stompRange && CanStomp())
        {
            TriggerStomp();
            return;
        }

        // Spin if player is within spin range and stomp isn't available
        if (dist <= spinRange && CanSpin())
        {
            TriggerSpin();
        }
    }

    // ── Stomp ─────────────────────────────────────────────────────────────────
    bool CanStomp() => Time.time >= lastStompTime + stompCooldown;

    void TriggerStomp()
    {
        lastStompTime = Time.time;
        animator.SetTrigger(ParamStomp);
        Invoke(nameof(ApplyStompDamage), stompDamageDelay);

        if (DirectorAI.Instance != null)
            DirectorAI.Instance.OnEnemyAttacked();
    }

    void ApplyStompDamage()
    {
        if (health.isDead) return;

        Collider[] hits = Physics.OverlapSphere(transform.position, stompRadius);

        foreach (Collider col in hits)
        {
            // Damage player
            if (col.CompareTag("Player"))
            {
                IDamageable damageable = col.GetComponentInParent<IDamageable>();
                damageable?.TakeDamage(Mathf.RoundToInt(stompDamage));

                // Knockback
                Rigidbody rb = col.GetComponentInParent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 knockDir = (col.transform.position - transform.position).normalized;
                    knockDir.y       = 0.3f;
                    rb.AddForce(knockDir * stompKnockback, ForceMode.Impulse);
                }

                if (DirectorAI.Instance != null)
                    DirectorAI.Instance.OnPlayerHit();
            }
        }
    }

    // ── Spin ──────────────────────────────────────────────────────────────────
    bool CanSpin() => Time.time >= lastSpinTime + spinCooldown;

    void TriggerSpin()
    {
        lastSpinTime = Time.time;
        animator.SetTrigger(ParamSpin);
        Invoke(nameof(ApplySpinDamage), spinDamageDelay);

        if (DirectorAI.Instance != null)
            DirectorAI.Instance.OnEnemyAttacked();
    }

    void ApplySpinDamage()
    {
        if (health.isDead) return;

        Collider[] hits = Physics.OverlapSphere(transform.position, spinRadius);

        foreach (Collider col in hits)
        {
            // Damage player
            if (col.CompareTag("Player"))
            {
                IDamageable damageable = col.GetComponentInParent<IDamageable>();
                damageable?.TakeDamage(Mathf.RoundToInt(spinDamage));

                if (DirectorAI.Instance != null)
                    DirectorAI.Instance.OnPlayerHit();
            }

            // Damage and knock back other enemies caught in spin
            if (col.CompareTag("Enemy") && col.gameObject != gameObject)
            {
                IDamageable damageable = col.GetComponentInParent<IDamageable>();
                damageable?.TakeDamage(Mathf.RoundToInt(spinDamage));

                Rigidbody rb = col.GetComponentInParent<Rigidbody>();
                if (rb != null)
                {
                    Vector3 knockDir = (col.transform.position - transform.position).normalized;
                    rb.AddForce(knockDir * 6f, ForceMode.Impulse);

                    if (DirectorAI.Instance != null)
                        DirectorAI.Instance.OnKnockback();
                }
            }
        }
    }

    // ── Gizmos ────────────────────────────────────────────────────────────────
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stompRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, spinRadius);
    }
}