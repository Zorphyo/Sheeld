using Core.Interfaces;
using UnityEngine;
using Traps.TrapUsageData;

namespace Traps.WallSpikeShooter
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class RagdollIceSpikeProjectile : MonoBehaviour
    {
        private GameObject trapRoot;

        [Header("Trap Data")]
        [SerializeField] private TrapType trapType = TrapType.WallSpikeShooter;

        [Header("Movement")]
        [SerializeField] private float defaultSpeed = 25f;
        [SerializeField] private float lifetime = 5f;

        [Header("Damage")]
        [SerializeField] private int damageAmount = 15;
        [SerializeField] private bool destroyOnHit = true;

        [Header("Enemy Ragdoll")]
        [SerializeField] private float maxRagdollForce = 120f;
        [SerializeField] private float minRagdollForce = 20f;
        [SerializeField] private float upwardForce = 15f;
        [SerializeField] private float maxForceDistance = 3f;
        [SerializeField] private float minForceDistance = 18f;
        [SerializeField] private float ragdollDuration = 4f;

        private Vector3 launchPosition;
        private Vector3 launchDirection;

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

        public void Launch(Vector3 direction, float speed, GameObject root)
        {
            trapRoot = root;

            if (rb == null)
                rb = GetComponent<Rigidbody>();

            // Force the projectile direction to be mostly horizontal.
            Vector3 flatDirection = new Vector3(direction.x, 0f, direction.z);

            // If the passed direction is straight up/down, use the trap root's forward instead.
            if (flatDirection.sqrMagnitude < 0.001f && trapRoot != null)
            {
                flatDirection = new Vector3(
                    trapRoot.transform.forward.x,
                    0f,
                    trapRoot.transform.forward.z
                );
            }

            // Final fallback.
            if (flatDirection.sqrMagnitude < 0.001f)
            {
                flatDirection = new Vector3(
                    transform.forward.x,
                    0f,
                    transform.forward.z
                );
            }

            flatDirection.Normalize();

            direction = flatDirection;

            launchPosition = transform.position;
            launchDirection = direction;

            rb.linearVelocity = direction * speed;
            hasBeenLaunched = true;

            Destroy(gameObject, lifetime);
        }
        public void Launch(Vector3 direction, float speed)
        {
            Launch(direction, speed, null);
        }

        private void Start()
        {
            if (!hasBeenLaunched)
            {
                Launch(transform.forward, defaultSpeed, trapRoot);
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

                    if (trapRoot != null && TrapStatsManager.Instance != null)
                    {
                        TrapStatsManager.Instance.RecordUniqueTrapUsed(trapRoot);
                    }

                    EnemyRagdollController ragdoll = other.GetComponentInParent<EnemyRagdollController>();

                    if (ragdoll != null)
                    {
                        float distanceFromWall = Vector3.Distance(launchPosition, transform.position);

                        float distancePercent = Mathf.InverseLerp(
                            maxForceDistance,
                            minForceDistance,
                            distanceFromWall
                        );

                        float forceAmount = Mathf.Lerp(maxRagdollForce, minRagdollForce, distancePercent);

                        Vector3 force = launchDirection * forceAmount + Vector3.up * upwardForce;

                        ragdoll.Knockback(force, transform.position, ragdollDuration);
                    }
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