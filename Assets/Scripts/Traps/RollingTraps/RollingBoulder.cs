using Core.Interfaces;
using Traps.TrapUsageData;
using UnityEngine;

namespace Traps.RollingTraps
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class RollingBoulder : MonoBehaviour
    {
        [Header("Trap Data")]
        [SerializeField] private TrapType trapType = TrapType.RollingTrap;

        [Header("Movement")]
        public float startSpeed = 22f;
        public float minimumSpeed = 18f;
        public float speedRecoveryAcceleration = 30f;

        [Tooltip("How strongly the boulder spins visually while moving.")]
        public float rollTorque = 20f;

        [Header("Targeting")]
        public bool aimTowardPlayerOnStart = true;
        public float randomYawOffset = 8f;

        [Header("Grounding")]
        public float groundStickForce = 35f;
        public float centerOfMassYOffset = -0.75f;
        public float maxUpwardVelocity = 1.5f;

        [Header("Damage")]
        public int damageAmount = 10;
        public float damageCooldown = 0.5f;

        [Header("Enemy Ragdoll")]
        public bool ragdollEnemies = true;
        public float enemyRagdollDuration = 4f;
        public float enemyForwardForce = 45f;
        public float enemyUpwardForce = 20f;

        [Header("Despawn")]
        public float fallbackLifeTime = 20f;
        public float minSpeedForStuckCheck = 1f;
        public float stuckTimeBeforeDestroy = 3f;

        private Rigidbody rb;
        private Vector3 travelDirection;
        private float lastDamageTime = -999f;
        private float stuckTimer = 0f;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();

            rb.linearDamping = 0f;
            rb.angularDamping = 0.05f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.centerOfMass = new Vector3(0f, centerOfMassYOffset, 0f);
        }

        private void Start()
        {
            SetTravelDirection();
            Launch();

            if (fallbackLifeTime > 0f)
            {
                Destroy(gameObject, fallbackLifeTime);
            }
        }

        private void SetTravelDirection()
        {
            Vector3 direction = transform.forward;

            if (aimTowardPlayerOnStart)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");

                if (player != null)
                {
                    direction = player.transform.position - transform.position;
                    direction.y = 0f;

                    if (direction.sqrMagnitude < 0.001f)
                    {
                        direction = transform.forward;
                    }
                }
            }

            direction.y = 0f;
            direction.Normalize();

            float yawOffset = Random.Range(-randomYawOffset, randomYawOffset);
            direction = Quaternion.Euler(0f, yawOffset, 0f) * direction;
            direction.Normalize();

            travelDirection = direction;

            if (travelDirection.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(travelDirection, Vector3.up);
            }
        }

        private void Launch()
        {
            Vector3 velocity = rb.linearVelocity;
            velocity.x = travelDirection.x * startSpeed;
            velocity.z = travelDirection.z * startSpeed;
            rb.linearVelocity = velocity;

            ApplyRollingTorque();
        }

        private void FixedUpdate()
        {
            MaintainGroundContact();
            MaintainForwardSpeed();
            ApplyRollingTorque();
            HandleStuckCheck();
        }

        private void MaintainGroundContact()
        {
            rb.AddForce(Vector3.down * groundStickForce, ForceMode.Acceleration);

            Vector3 velocity = rb.linearVelocity;

            if (velocity.y > maxUpwardVelocity)
            {
                velocity.y = maxUpwardVelocity;
                rb.linearVelocity = velocity;
            }
        }

        private void MaintainForwardSpeed()
        {
            Vector3 velocity = rb.linearVelocity;
            Vector3 flatVelocity = new Vector3(velocity.x, 0f, velocity.z);

            float forwardSpeed = Vector3.Dot(flatVelocity, travelDirection);

            if (forwardSpeed < minimumSpeed)
            {
                rb.AddForce(travelDirection * speedRecoveryAcceleration, ForceMode.Acceleration);
            }

            Vector3 desiredFlatVelocity = travelDirection * Mathf.Max(forwardSpeed, minimumSpeed);
            rb.linearVelocity = new Vector3(desiredFlatVelocity.x, velocity.y, desiredFlatVelocity.z);
        }

        private void ApplyRollingTorque()
        {
            Vector3 torqueAxis = Vector3.Cross(Vector3.up, travelDirection).normalized;
            rb.AddTorque(torqueAxis * rollTorque, ForceMode.Acceleration);
        }

        private void HandleStuckCheck()
        {
            Vector3 flatVelocity = rb.linearVelocity;
            flatVelocity.y = 0f;

            if (flatVelocity.magnitude < minSpeedForStuckCheck)
            {
                stuckTimer += Time.fixedDeltaTime;

                if (stuckTimer >= stuckTimeBeforeDestroy)
                {
                    Destroy(gameObject);
                }
            }
            else
            {
                stuckTimer = 0f;
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            TryDamage(collision);
        }

        private void OnCollisionStay(Collision collision)
        {
            TryDamage(collision);
        }

        private void TryDamage(Collision collision)
        {
            if (Time.time - lastDamageTime < damageCooldown)
                return;

            GameObject other = collision.gameObject;

            IDamageable damageable = other.GetComponentInParent<IDamageable>();
            if (damageable == null)
                return;

            if (other.CompareTag("Player"))
            {
                Record(TrapEventType.HitPlayer);
            }
            else if (other.CompareTag("Enemy"))
            {
                Record(TrapEventType.HitEnemy);
            }

            damageable.TakeDamage(damageAmount);

            if (other.CompareTag("Enemy"))
            {
                TryRagdollEnemy(other, collision);
                Record(TrapEventType.DamagedEnemy);
            }
            else if (other.CompareTag("Player"))
            {
                Record(TrapEventType.DamagedPlayer);
            }

            lastDamageTime = Time.time;
        }

        private void TryRagdollEnemy(GameObject other, Collision collision)
        {
            if (!ragdollEnemies)
                return;

            EnemyRagdollController ragdoll = other.GetComponentInParent<EnemyRagdollController>();

            if (ragdoll == null)
                return;

            Vector3 hitPoint = collision.contactCount > 0
                ? collision.GetContact(0).point
                : other.transform.position;

            Vector3 force = travelDirection * enemyForwardForce;
            force += Vector3.up * enemyUpwardForce;

            ragdoll.Knockback(force, hitPoint, enemyRagdollDuration);
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