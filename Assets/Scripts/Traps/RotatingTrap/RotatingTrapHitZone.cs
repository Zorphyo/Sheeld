using System.Collections.Generic;
using UnityEngine;
using Core.Interfaces;
using Traps.TrapUsageData;

namespace Traps
{
    public class RotatingTrapHitZone : MonoBehaviour
    {
        [Header("Trap Data")]
        [SerializeField] private TrapType trapType = TrapType.RotatingTrap;

        [Header("Damage Settings")]
        [SerializeField] private int damageAmount = 10;
        [SerializeField] private float damageInterval = 0.5f;

        [Header("Knockback Settings")]
        [SerializeField] private float knockbackForce = 8f;

        [SerializeField] private Transform trapRoot;

        private Dictionary<Component, float> nextHitTime = new Dictionary<Component, float>();

        private void OnTriggerEnter(Collider other)
        {
            IDamageable damageable = other.GetComponentInParent<IDamageable>();
            IKnockbackable knockbackable = other.GetComponentInParent<IKnockbackable>();

            if (damageable == null && knockbackable == null)
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
            IKnockbackable knockbackable = other.GetComponentInParent<IKnockbackable>();

            if (damageable == null && knockbackable == null)
                return;

            Component key = null;

            if (damageable is Component damageComponent)
            {
                key = damageComponent;
            }
            else if (knockbackable is Component knockbackComponent)
            {
                key = knockbackComponent;
            }

            if (key == null)
                return;

            if (!nextHitTime.ContainsKey(key))
            {
                nextHitTime[key] = 0f;
            }

            if (Time.time >= nextHitTime[key])
            {
                if (damageable != null)
                {
                    damageable.TakeDamage(damageAmount);

                    if (other.CompareTag("Player"))
                    {
                        Record(TrapEventType.DamagedPlayer);
                    }
                    else if (other.CompareTag("Enemy"))
                    {
                        Record(TrapEventType.DamagedEnemy);
                    }
                }

                if (knockbackable != null)
                {
                    Vector3 direction;

                    if (trapRoot != null)
                    {
                        direction = other.transform.position - trapRoot.position;
                    }
                    else
                    {
                        direction = other.transform.position - transform.position;
                    }

                    direction.y = 0f;
                    direction.Normalize();

                    knockbackable.Knockback(direction, knockbackForce);
                }

                nextHitTime[key] = Time.time + damageInterval;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            IDamageable damageable = other.GetComponentInParent<IDamageable>();
            IKnockbackable knockbackable = other.GetComponentInParent<IKnockbackable>();

            Component key = null;

            if (damageable is Component damageComponent)
            {
                key = damageComponent;
            }
            else if (knockbackable is Component knockbackComponent)
            {
                key = knockbackComponent;
            }

            if (key != null && nextHitTime.ContainsKey(key))
            {
                nextHitTime.Remove(key);
            }
        }

        private void OnDisable()
        {
            nextHitTime.Clear();
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