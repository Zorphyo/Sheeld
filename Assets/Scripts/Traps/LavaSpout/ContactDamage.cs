using Core.Interfaces;
using UnityEngine;
using Traps.TrapUsageData;

namespace Traps.LavaSpout
{
    public class ContactDamage : MonoBehaviour
    {
        [Header("Trap Data")]
        [Tooltip("Used for trap usage data logging.")]
        public TrapType trapType = TrapType.LavaSpout;

        public int damage = 5;
        public float damageCooldown = 1f;

        private float nextDamageTime;

        private void OnTriggerStay(Collider other)
        {
            if (Time.time < nextDamageTime)
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

            damageable.TakeDamage(damage);
            nextDamageTime = Time.time + damageCooldown;

            if (other.CompareTag("Player"))
            {
                Record(TrapEventType.DamagedPlayer);
            }
            else if (other.CompareTag("Enemy"))
            {
                Record(TrapEventType.DamagedEnemy);
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