using System.Collections.Generic;
using Core.Interfaces;
using Traps.TrapUsageData;
using UnityEngine;

namespace Traps.MovingSpikeTrap
{
    public class SpikeDamageZone : MonoBehaviour
    {
        [Header("Trap Data")]
        [SerializeField] private TrapType trapType = TrapType.MovingSpike;

        [Header("Damage Settings")]
        [SerializeField] private int playerDamage = 20;
        [SerializeField] private int enemyDamage = 50;
        [SerializeField] private float damageInterval = 0.5f;

        [Header("Enemy Ragdoll Settings")]
        [SerializeField] private bool ragdollEnemies = true;
        [SerializeField] private float upwardForce = 28f;
        [SerializeField] private float sideForce = 6f;
        [SerializeField] private float ragdollDuration = 1.5f;

        private Dictionary<IDamageable, float> nextDamageTime = new Dictionary<IDamageable, float>();

        private void OnTriggerEnter(Collider other)
        {
            TryRecordHit(other);
        }

        private void OnTriggerStay(Collider other)
        {
            TryDamage(other);
        }

        private void OnTriggerExit(Collider other)
        {
            IDamageable damageable = other.GetComponentInParent<IDamageable>();

            if (damageable == null)
                return;

            if (nextDamageTime.ContainsKey(damageable))
            {
                nextDamageTime.Remove(damageable);
            }
        }

        private void OnDisable()
        {
            nextDamageTime.Clear();
        }

        private void TryRecordHit(Collider other)
        {
            if (other.CompareTag("Enemy"))
            {
                Record(TrapEventType.HitEnemy);
            }
            else if (other.CompareTag("Player"))
            {
                Record(TrapEventType.HitPlayer);
            }
        }

        private void TryDamage(Collider other)
        {
            IDamageable damageable = other.GetComponentInParent<IDamageable>();

            if (damageable == null)
                return;

            if (!nextDamageTime.ContainsKey(damageable))
            {
                nextDamageTime[damageable] = 0f;
            }

            if (Time.time < nextDamageTime[damageable])
                return;

            if (other.CompareTag("Enemy"))
            {
                damageable.TakeDamage(enemyDamage);
                TryRagdollEnemy(other);
                Record(TrapEventType.DamagedEnemy);
            }
            else if (other.CompareTag("Player"))
            {
                damageable.TakeDamage(playerDamage);
                Record(TrapEventType.DamagedPlayer);
            }

            nextDamageTime[damageable] = Time.time + damageInterval;
        }

        private void TryRagdollEnemy(Collider other)
        {
            if (!ragdollEnemies)
                return;

            EnemyRagdollController ragdoll = other.GetComponentInParent<EnemyRagdollController>();

            if (ragdoll == null)
                return;

            Vector3 hitPoint = other.ClosestPoint(transform.position);

            Vector3 sideDirection = (other.transform.position - transform.position).normalized;

            if (sideDirection == Vector3.zero)
            {
                sideDirection = transform.forward;
            }

            Vector3 force = (Vector3.up * upwardForce) + (sideDirection * sideForce);

            ragdoll.Knockback(force, hitPoint, ragdollDuration);
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