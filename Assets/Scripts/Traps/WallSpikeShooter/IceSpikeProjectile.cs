using Core.Interfaces;
using UnityEngine;
using Traps.TrapUsageData;

namespace Traps.WallSpikeShooter
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class IceSpikeProjectile : MonoBehaviour
    {
        [Header("Trap Data")]
        [SerializeField] private TrapType trapType = TrapType.WallSpikeShooter;

        [Header("Movement")]
        [SerializeField] private float defaultSpeed = 25f;
        [SerializeField] private float lifetime = 5f;

        [Header("Damage")]
        [SerializeField] private int damageAmount = 15;
        [SerializeField] private bool destroyOnHit = true;

        private Rigidbody rb;
        private bool hasHit = false;
        private bool hasBeenLaunched = false;

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

        public void Launch(Vector3 direction, float speed)
        {
            if (rb == null)
                rb = GetComponent<Rigidbody>();

            direction.y = Mathf.Clamp(direction.y, -0.35f, 0.35f);

            if (direction.sqrMagnitude < 0.001f)
            {
                direction = transform.forward;
            }

            direction.Normalize();

            rb.linearVelocity = direction * speed;
            hasBeenLaunched = true;

            Destroy(gameObject, lifetime);
        }

        private void Start()
        {
            if (!hasBeenLaunched)
            {
                Launch(transform.forward, defaultSpeed);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (hasHit)
                return;

            IDamageable damageable = other.GetComponentInParent<IDamageable>();

            if (damageable != null)
            {
                if (other.CompareTag("Player"))
                {
                    Record(TrapEventType.HitPlayer);
                    Record(TrapEventType.DamagedPlayer);
                }
                else if (other.CompareTag("Enemy"))
                {
                    Record(TrapEventType.HitEnemy);
                    Record(TrapEventType.DamagedEnemy);
                }

                damageable.TakeDamage(damageAmount);

                hasHit = true;

                if (destroyOnHit)
                    Destroy(gameObject);

                return;
            }

            if (!other.isTrigger)
            {
                hasHit = true;

                if (destroyOnHit)
                    Destroy(gameObject);
            }
        }

        private void Record(TrapEventType eventType)
        {
            if (TrapStatsManager.Instance != null)
            {
                TrapStatsManager.Instance.RecordTrapEvent(trapType, eventType);
            }
        }
    }
}