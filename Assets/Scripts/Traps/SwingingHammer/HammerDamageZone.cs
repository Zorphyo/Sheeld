using System.Collections.Generic;
using Core.Interfaces;
using UnityEngine;

namespace Traps.SwingingHammer
{
    public class HammerDamageZone : MonoBehaviour
    {
        [Header("Damage Settings")]
        [SerializeField] private int playerDamage = 25;
        [SerializeField] private int enemyDamage = 40;
        [SerializeField] private float damageInterval = 0.5f;

        private Dictionary<IDamageable, float> nextDamageTime = new Dictionary<IDamageable, float>();

        private void OnTriggerStay(Collider other)
        {
            Debug.Log("DamageZone touching: " + other.name, this);
            
            IDamageable damageable = other.GetComponentInParent<IDamageable>();
            if (damageable == null)
                return;

            if (!nextDamageTime.ContainsKey(damageable))
            {
                nextDamageTime[damageable] = 0f;
            }

            if (Time.time >= nextDamageTime[damageable])
            {
                int damage = other.CompareTag("Enemy") ? enemyDamage : playerDamage;
                damageable.TakeDamage(damage);
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
    }
}