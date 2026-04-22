using Core.Interfaces;
using UnityEngine;

namespace Traps
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

        [Header("Movement")]
        public float startSpeed = 22f;
        public float minimumSpeed = 18f;
        public float speedRecoveryAcceleration = 30f;

        [Tooltip("How strongly the log spins visually while moving.")]
        public float rollTorque = 20f;

        [Header("Fixed Rolling Direction")]
        [Tooltip("All logs will roll in this same world direction, no matter how they spawned.")]
        public WorldRollDirection rollDirection = WorldRollDirection.PositiveZ;

        [Header("Grounding")]
        public float groundStickForce = 35f;
        public float centerOfMassYOffset = -0.75f;
        public float maxUpwardVelocity = 1.5f;

        [Header("Damage")]
        public int damageAmount = 10;
        public float damageCooldown = 0.5f;

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

            IDamageable damageable = other.GetComponent<IDamageable>();
            if (damageable == null)
                return;

            damageable.TakeDamage(damageAmount);
            lastDamageTime = Time.time;
        }
    }
}