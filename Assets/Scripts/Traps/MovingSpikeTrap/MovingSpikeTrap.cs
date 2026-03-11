using System.Collections;
using UnityEngine;

/*
   MovingSpikeTrap
    ----------
    This script controls the spike trap behavior.

    Responsibilities:
    - Moves spikes up and down
    - Enables damage only while spikes are up
    - Handles cooldown and retrigger logic

    It does not detect the player directly.
    That is handled by a separate TriggerZone script.
*/

namespace Traps.MovingSpikeTrap
{
    public class MovingSpikeTrap : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform spikesVisual;      // the object that moves up/down (spike mesh only)
        [SerializeField] private Collider damageZone;         // trigger collider for damage
        [SerializeField] private Collider triggerZone;        // triggers the spikes up

        [Header("Spike Movement settings")]
        [SerializeField] private float riseDistance = 1.5f;   // how far spikes move upward
        [SerializeField] private float riseSpeed = 12f;       // how fast they go up/down
        [SerializeField] private float stayUpTime = 2f;       // how long spikes stay up
        [SerializeField] private float cooldownTime = 1f;     // time before trap can trigger again

        [Header("Behavior")]
        [SerializeField] private bool canRetrigger = true;
        
        // Internal state variables
        private Vector3 downPosition;                       // original local position
        private Vector3 upPosition;                         // target local position when raised

        private bool isActive = false;                      // true while trap is currently firing
        private bool isOnCooldown = false;                  // true while waiting to retrigger
        
        private void Start()
        {
            // Save the starting (down) position
            downPosition = spikesVisual.localPosition;
            
            // Calculate where the spikes should move to when raised
            upPosition = downPosition + Vector3.forward * riseDistance;
            
            // Make sure damage is disabled at start
            if (damageZone != null)
            {
                damageZone.enabled = false;
                
            }
        }
        
        /*
        TryActivate()
        --------------
        Called by the TriggerZone script when a player steps on the trap.

        This function checks whether the trap is allowed to activate,
        and if so, starts the activation coroutine.
        */
        public void TryActivate()
        {
            if (!isActive && !isOnCooldown)
            {
                StartCoroutine(ActivateTrap());
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isActive || isOnCooldown)
                return;

            if (!other.CompareTag("Player") && !other.CompareTag("Enemy"))
                return;

            StartCoroutine(ActivateTrap());
        }

        private IEnumerator ActivateTrap()
        {
            isActive = true;

            // Turn on damage only during active trap
            if (damageZone != null)
            {
                damageZone.enabled = true;
            }

            // Move spikes up quickly
            while (Vector3.Distance(spikesVisual.localPosition, upPosition) > 0.01f)
            {
                spikesVisual.localPosition = Vector3.MoveTowards(
                    spikesVisual.localPosition,
                    upPosition,
                    riseSpeed * Time.deltaTime
                );

                yield return null;
            }

            spikesVisual.localPosition = upPosition;

            // Stay up for inspector-controlled duration
            yield return new WaitForSeconds(stayUpTime);

            // Move spikes back down
            while (Vector3.Distance(spikesVisual.localPosition, downPosition) > 0.01f)
            {
                spikesVisual.localPosition = Vector3.MoveTowards(
                    spikesVisual.localPosition,
                    downPosition,
                    riseSpeed * Time.deltaTime
                );

                yield return null;
            }

            spikesVisual.localPosition = downPosition;

            // Turn off damage when spikes are down
            if (damageZone != null)
            {
                damageZone.enabled = false;
            }

            isActive = false;

            if (canRetrigger)
            {
                isOnCooldown = true;
                yield return new WaitForSeconds(cooldownTime);
                isOnCooldown = false;
            }
        }
    }
}