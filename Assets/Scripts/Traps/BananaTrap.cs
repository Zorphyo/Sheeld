using Core.Interfaces;
using UnityEngine;

namespace Traps
{
    public class BananaTrap : MonoBehaviour
    {
        [SerializeField] private int damageAmount = 5;

        [SerializeField] private float slipForce = 8f;
        [SerializeField] private float upwardSlipForce = 2f;

        [SerializeField] private float destroyAfterTriggerDelay = 0.2f;
        [SerializeField] private bool destroyOnTrigger = true;

        private bool hasTriggered = false;

        private void OnTriggerEnter(Collider other)
        {
            if (hasTriggered) return;

            IDamageable damageable = other.GetComponentInParent<IDamageable>();

            if (damageable == null)
                return;

            hasTriggered = true;

            damageable.TakeDamage(damageAmount);

            Vector3 slipDirection = other.transform.forward;
            slipDirection.y = 0f;
            slipDirection.Normalize();

            if (slipDirection == Vector3.zero)
                slipDirection = transform.forward;

            IKnockbackable knockbackable = other.GetComponentInParent<IKnockbackable>();

            if (knockbackable != null)
            {
                knockbackable.Knockback(slipDirection + Vector3.up * upwardSlipForce, slipForce);
            }

            if (destroyOnTrigger)
            {
                Destroy(gameObject, destroyAfterTriggerDelay);
            }
        }
    }
}