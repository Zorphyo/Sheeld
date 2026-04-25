using Core.Interfaces;
using UnityEngine;

namespace Traps.WallSpikeShooter
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class IceSpikeProjectile : MonoBehaviour
    {
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
                Debug.LogWarning("IceSpikeProjectile: Invalid launch direction.", this);
                direction = transform.forward;
            }

            direction.Normalize();

            rb.linearVelocity = direction * speed;
            hasBeenLaunched = true;

            Destroy(gameObject, lifetime);
        }

        private void Start()
        {
            // Safety fallback if someone manually places a spike in the scene.
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
    }
}