using Core.Interfaces;
using Traps.TrapUsageData;
using UnityEngine;

namespace Traps.RollingTraps
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class RollingLog : MonoBehaviour
    {
        public enum WorldRollDirection
        {
            PositiveX,
            NegativeX,
            PositiveZ,
            NegativeZ
        }

        public enum LocalLongAxis
        {
            LocalX,
            LocalY,
            LocalZ
        }

        [Header("Trap Data")]
        [SerializeField] private TrapType trapType = TrapType.RollingTrap;

        [Header("Movement")]
        public float startSpeed = 18f;
        public float minimumSpeed = 14f;
        public float speedRecoveryAcceleration = 20f;
        public float rollTorque = 8f;

        [Header("Direction")]
        public WorldRollDirection rollDirection = WorldRollDirection.NegativeX;

        [Tooltip("Which LOCAL axis points along the length of the log/capsule.")]
        public LocalLongAxis logLongAxis = LocalLongAxis.LocalY;

        [Header("Orientation Fix")]
        public bool forceCorrectLogOrientationOnSpawn = true;

        [Header("Grounding")]
        public float groundStickForce = 45f;
        public float centerOfMassYOffset = -0.5f;
        public float maxUpwardVelocity = 1f;

        [Header("Damage")]
        public int damageAmount = 10;
        public float damageCooldown = 0.5f;

        [Header("Despawn")]
        public float fallbackLifeTime = 20f;
        public float minSpeedForStuckCheck = 1f;
        public float stuckTimeBeforeDestroy = 3f;

        private Rigidbody rb;
        private Vector3 travelDirection;
        private Vector3 rollAxis;
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
            // Move sideways using the log's red X arrow.
            travelDirection = transform.right;
            travelDirection.y = 0f;
            travelDirection.Normalize();

            // Roll around the log's long axis, which is green/Y.
            rollAxis = transform.up.normalized;

            Launch();

            if (fallbackLifeTime > 0f)
            {
                Destroy(gameObject, fallbackLifeTime);
            }
        }

        private void SetTravelDirection()
        {
            switch (rollDirection)
            {
                case WorldRollDirection.PositiveX:
                    travelDirection = Vector3.right;
                    break;

                case WorldRollDirection.NegativeX:
                    travelDirection = Vector3.left;
                    break;

                case WorldRollDirection.PositiveZ:
                    travelDirection = Vector3.forward;
                    break;

                case WorldRollDirection.NegativeZ:
                    travelDirection = Vector3.back;
                    break;
            }
        }

        private void ForceLogLongAxisToMatchRollAxis()
        {
            Vector3 currentLongAxis = GetCurrentWorldLongAxis();

            if (currentLongAxis == Vector3.zero)
                return;

            Quaternion correction = Quaternion.FromToRotation(currentLongAxis, rollAxis);
            transform.rotation = correction * transform.rotation;
        }

        private Vector3 GetCurrentWorldLongAxis()
        {
            switch (logLongAxis)
            {
                case LocalLongAxis.LocalX:
                    return transform.right;

                case LocalLongAxis.LocalY:
                    return transform.up;

                case LocalLongAxis.LocalZ:
                    return transform.forward;

                default:
                    return transform.up;
            }
        }

        private void Launch()
        {
            rb.linearVelocity = new Vector3(
                travelDirection.x * startSpeed,
                rb.linearVelocity.y,
                travelDirection.z * startSpeed
            );

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

            rb.linearVelocity = new Vector3(
                desiredFlatVelocity.x,
                velocity.y,
                desiredFlatVelocity.z
            );
        }

        private void ApplyRollingTorque()
        {
            rb.AddTorque(rollAxis * rollTorque, ForceMode.Acceleration);
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
            TryDamage(collision.gameObject);
        }

        private void OnCollisionStay(Collision collision)
        {
            TryDamage(collision.gameObject);
        }

        private void TryDamage(GameObject other)
        {
            if (Time.time - lastDamageTime < damageCooldown)
                return;

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

            if (other.CompareTag("Player"))
            {
                Record(TrapEventType.DamagedPlayer);
            }
            else if (other.CompareTag("Enemy"))
            {
                Record(TrapEventType.DamagedEnemy);
            }

            lastDamageTime = Time.time;
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