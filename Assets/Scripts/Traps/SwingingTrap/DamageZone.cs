using System.Collections.Generic;
using Core.Interfaces;
using Traps.TrapUsageData;
using UnityEngine;

namespace Traps.SwingingTrap
{
    public class DamageZone : MonoBehaviour
    {
        [Header("Trap Data")]
        [SerializeField] private TrapType trapType = TrapType.SwingingTrap;

        [Header("Damage Settings")]
        [SerializeField] private int playerDamage = 25;
        [SerializeField] private int enemyDamage = 40;
        [SerializeField] private float damageInterval = 0.5f;

        [Header("Enemy Ragdoll Settings")]
        [SerializeField] private bool ragdollEnemies = true;
        [SerializeField] private float ragdollForce = 18f;
        [SerializeField] private float upwardForce = 4f;
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
            if (other.CompareTag("Player"))
            {
                Record(TrapEventType.HitPlayer);
            }
            else if (other.CompareTag("Enemy"))
            {
                Record(TrapEventType.HitEnemy);
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

            Vector3 forceDirection = (other.transform.position - transform.position).normalized;

            if (forceDirection == Vector3.zero)
            {
                forceDirection = transform.forward;
            }

            Vector3 finalForce = (forceDirection * ragdollForce) + (Vector3.up * upwardForce);

            ragdoll.Knockback(finalForce, hitPoint, ragdollDuration);
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