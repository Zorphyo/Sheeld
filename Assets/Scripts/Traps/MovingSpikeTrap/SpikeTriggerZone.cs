using UnityEngine;

namespace Traps.MovingSpikeTrap
{
    /*
     SpikeTriggerZone
     ----------------
     This script detects when anything enters the trigger zone.

     That means the trap can be activated by:
     - the player
     - enemies
     - thrown objects
     - physics props
     - anything else with a collider that enters the trigger
 */

    public class SpikeTriggerZone : MonoBehaviour
    {
        [SerializeField] private MovingSpikeTrap movingSpikeTrap;

        private void OnTriggerEnter(Collider other)
        {
                movingSpikeTrap.TryActivate();
        }
    }
}