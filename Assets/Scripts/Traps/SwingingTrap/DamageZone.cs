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

        private Dictionary<IDamageable, float> nextDamageTime = new Dictionary<IDamageable, float>();

        private void OnTriggerEnter(Collider other)
        {
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
        }

        private void OnTriggerStay(Collider other)
        {
            IDamageable damageable = other.GetComponentInParent<IDamageable>();

            if (damageable == null)
                return;

            if (!nextDamageTime.ContainsKey(damageable))
            {
                nextDamageTime[damageable] = 0f;
            }

            if (Time.time >= nextDamageTime[damageable])
            {
                if (other.CompareTag("Enemy"))
                {
                    damageable.TakeDamage(enemyDamage);
                    Record(TrapEventType.DamagedEnemy);
                }
                else if (other.CompareTag("Player"))
                {
                    damageable.TakeDamage(playerDamage);
                    Record(TrapEventType.DamagedPlayer);
                }

                nextDamageTime[damageable] = Time.time + damageInterval;
            }
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

        private void Record(TrapEventType eventType)
        {
            if (TrapStatsManager.Instance != null)
            {
                TrapStatsManager.Instance.RecordTrapEvent(trapType, eventType);
            }
        }
    }
}