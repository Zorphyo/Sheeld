using System.Collections.Generic;
using UnityEngine;
using Core.Interfaces;

namespace Traps.StationarySpikeTrap
{
    /*
        StationarySpikeDamage
        ---------------------
        Damages anything touching this stationary spike trap,
        as long as it implements IDamageable.

        This supports repeated damage over time while
        the object remains in contact with the spikes.
    */

    public class StationarySpikeDamage : MonoBehaviour
    {
        [Header("Damage Settings")]
        [SerializeField] private int damageAmount = 10;
        [SerializeField] private float damageInterval = 0.5f;

        private Dictionary<IDamageable, float> nextDamageTime = new Dictionary<IDamageable, float>();

        private void OnCollisionStay(Collision collision)
        {
            IDamageable damageable = collision.collider.GetComponentInParent<IDamageable>();

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

        private void OnCollisionExit(Collision collision)
        {
            IDamageable damageable = collision.collider.GetComponentInParent<IDamageable>();

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