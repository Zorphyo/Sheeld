using UnityEngine;

namespace Traps.SwingingTrap
{
    public class TriggerZone : MonoBehaviour
    {
        [SerializeField] private SwingingHammer.SwingingTrap hammerTrap;

        [Header("Activation Rules")]
        [SerializeField] private bool triggerOnPlayer = true;
        [SerializeField] private bool triggerOnEnemy = true;
        [SerializeField] private bool triggerOnAnyCollider = true;

        private void OnTriggerEnter(Collider other)
        {
            TryTrigger(other);
        }

        private void OnTriggerStay(Collider other)
        {
            TryTrigger(other);
        }

        private void TryTrigger(Collider other)
        {
            if (hammerTrap == null)
            {
                Debug.LogWarning("HammerTriggerZone: Hammer trap is not assigned.", this);
                return;
            }

            if (triggerOnAnyCollider)
            {
                hammerTrap.TryActivate();
                return;
            }

            if (triggerOnPlayer && other.CompareTag("Player"))
            {
                hammerTrap.TryActivate();
                return;
            }

            if (triggerOnEnemy && other.CompareTag("Enemy"))
            {
                hammerTrap.TryActivate();
            }
        }
    }
}