using Core.Interfaces;
using UnityEngine;
using Traps.TrapUsageData;

namespace Traps.LavaSpout
{
    public class FireballProjectile : MonoBehaviour
    {
        [Header("Trap Data")]
        [Tooltip("Used for trap usage data logging.")]
        public TrapType trapType = TrapType.LavaSpout;

        [Header("Damage")]
        public int damage = 10;

        [Header("Lifetime")]
        public float maxLifetime = 6f;

        private void Start()
        {
            Destroy(gameObject, maxLifetime);
        }

        private void OnCollisionEnter(Collision collision)
        {
            HandleHit(collision.gameObject);
            Destroy(gameObject);
        }

        private void OnTriggerEnter(Collider other)
        {
            HandleHit(other.gameObject);
            Destroy(gameObject);
        }

        private void HandleHit(GameObject hitObject)
        {
            IDamageable damageable = hitObject.GetComponentInParent<IDamageable>();

            if (damageable == null)
                return;

            if (hitObject.CompareTag("Player"))
            {
                Record(TrapEventType.HitPlayer);
            }
            else if (hitObject.CompareTag("Enemy"))
            {
                Record(TrapEventType.HitEnemy);
            }

            damageable.TakeDamage(damage);

            if (hitObject.CompareTag("Player"))
            {
                Record(TrapEventType.DamagedPlayer);
            }
            else if (hitObject.CompareTag("Enemy"))
            {
                Record(TrapEventType.DamagedEnemy);
            }
        }
    
        public void SetScale(float scale)
        {
            damage = Mathf.RoundToInt(damage * scale);
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