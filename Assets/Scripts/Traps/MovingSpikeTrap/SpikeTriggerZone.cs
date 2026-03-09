using UnityEngine;

namespace Traps.MovingSpikeTrap
{
    public class SpikeTriggerZone : MonoBehaviour
    {
        [SerializeField] private MovingSpikeTrap movingSpikeTrap;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                movingSpikeTrap.TryActivate();
            }
        }
    }
}