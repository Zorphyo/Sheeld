using System.Collections.Generic;
using Core.Interfaces;
using UnityEngine;

namespace Traps.MovingSpikeTrap
{
    /*
        SpikeDamageZone
        ----------------
        Damages any object that implements IDamageable.

        This makes the trap work for:
        - Player
        - Enemies
        - Bosses
        - Future damageable objects

        No hard dependency on PlayerStats or EnemyHealth
    */

    public class SpikeDamageZone : MonoBehaviour
    {
        [Header("Damage Settings")]
        [SerializeField] private int damageAmount = 10;
        [SerializeField] private float damageInterval = 0.5f;

        private Dictionary<IDamageable, float> nextDamageTime = new Dictionary<IDamageable, float>();

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
                damageable.TakeDamage(damageAmount);
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