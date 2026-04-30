using Core.Interfaces;
using UnityEngine;
using Traps.TrapUsageData;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class BallistaBoltProjectile : MonoBehaviour
{
    [Header("Trap Data")]
    [SerializeField] private TrapType trapType = TrapType.Throwable;

    [Header("Projectile")]
    public float lifetime = 6f;
    public bool destroyOnHit = true;

    [Header("Damage")]
    public int damageAmount = 40;

    [Header("Ragdoll")]
    public float maxRagdollForce = 180f;
    public float minRagdollForce = 60f;
    public float upwardForce = 10f;
    public float maxForceDistance = 5f;
    public float minForceDistance = 30f;
    public float ragdollDuration = 5f;

    private Rigidbody rb;
    private GameObject trapRoot;
    private Vector3 launchPosition;
    private Vector3 launchDirection;
    private bool hasHit = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.useGravity = false;
        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        Collider col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    public void Launch(Vector3 direction, float speed, GameObject root)
    {
        trapRoot = root;

        if (rb == null)
            rb = GetComponent<Rigidbody>();

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            direction = transform.forward;

        direction.Normalize();

        launchPosition = transform.position;
        launchDirection = direction;

        rb.linearVelocity = direction * speed;

        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit)
            return;

        IDamageable damageable = other.GetComponentInParent<IDamageable>();

        if (damageable == null)
        {
            if (!other.isTrigger && destroyOnHit)
            {
                hasHit = true;
                Destroy(gameObject);
            }

            return;
        }

        if (other.CompareTag("Enemy"))
        {
            Record(TrapEventType.HitEnemy);

            EnemyRagdollController ragdoll = other.GetComponentInParent<EnemyRagdollController>();

            if (ragdoll != null)
            {
                float distanceFromBallista = Vector3.Distance(launchPosition, transform.position);

                float distancePercent = Mathf.InverseLerp(
                    maxForceDistance,
                    minForceDistance,
                    distanceFromBallista
                );

                float forceAmount = Mathf.Lerp(maxRagdollForce, minRagdollForce, distancePercent);

                Vector3 force = launchDirection * forceAmount + Vector3.up * upwardForce;

                ragdoll.Knockback(force, transform.position, ragdollDuration);
            }

            damageable.TakeDamage(damageAmount);
            Record(TrapEventType.DamagedEnemy);
        }
        else if (other.CompareTag("Player"))
        {
            Record(TrapEventType.HitPlayer);
            damageable.TakeDamage(damageAmount);
            Record(TrapEventType.DamagedPlayer);
        }

        if (TrapStatsManager.Instance != null && trapRoot != null)
        {
            TrapStatsManager.Instance.RecordUniqueTrapUsed(trapRoot);
        }

        hasHit = true;

        if (destroyOnHit)
            print("Ballista bolt hit " + other.name + ", destroying.");
        Destroy(gameObject);
    }

    private void Record(TrapEventType eventType)
    {
        if (TrapStatsManager.Instance != null)
        {
            TrapStatsManager.Instance.RecordTrapEvent(trapType, eventType);
        }
    }
}