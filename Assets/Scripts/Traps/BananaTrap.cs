using Core.Interfaces;
using UnityEngine;
using Traps.TrapUsageData;

namespace Traps
{
    public class BananaTrap : MonoBehaviour
    {
        [Header("Trap Data")]
        [SerializeField] private TrapType trapType = TrapType.Banana;

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

            Record(TrapEventType.Triggered);

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

            Vector3 slipDirection = other.transform.forward;
            slipDirection.y = 0f;
            slipDirection.Normalize();

            if (slipDirection == Vector3.zero)
                slipDirection = transform.forward;

            IKnockbackable knockbackable = other.GetComponentInParent<IKnockbackable>();

            if (knockbackable != null)
            {
                knockbackable.Knockback(
                    slipDirection + Vector3.up * upwardSlipForce,
                    slipForce
                );
            }

            if (destroyOnTrigger)
            {
                Destroy(gameObject, destroyAfterTriggerDelay);
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