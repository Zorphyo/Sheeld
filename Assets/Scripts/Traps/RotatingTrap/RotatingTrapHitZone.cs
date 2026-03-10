using System.Collections.Generic;
using UnityEngine;
using Core.Interfaces;

namespace Traps
{
    /*
        RotatingTrapHitZone
        -------------------
        Attach this to the rotating log's DamageZone trigger collider.

        Responsibilities:
        - Apply damage to anything implementing IDamageable
        - Apply knockback to anything implementing IKnockbackable
        - Support repeated hits over time with a configurable interval
    */
    public class RotatingTrapHitZone : MonoBehaviour
    {
        [Header("Damage Settings")]
        [SerializeField] private int damageAmount = 10;
        [SerializeField] private float damageInterval = 0.5f;

        [Header("Knockback Settings")]
        [SerializeField] private float knockbackForce = 8f;

        // assign the spinning log root so knockback direction can be computed from the log outward
        [SerializeField] private Transform trapRoot;

        private Dictionary<Component, float> nextHitTime = new Dictionary<Component, float>();

        private void OnTriggerStay(Collider other)
        {
            IDamageable damageable = other.GetComponentInParent<IDamageable>();
            IKnockbackable knockbackable = other.GetComponentInParent<IKnockbackable>();

            // If the target supports neither damage nor knockback, ignore it
            if (damageable == null && knockbackable == null)
                return;

            // Use the target component we found as dictionary key
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

                    knockbackable.ApplyKnockback(direction, knockbackForce);
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
    }
}