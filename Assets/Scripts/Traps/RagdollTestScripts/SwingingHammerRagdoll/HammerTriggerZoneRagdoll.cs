using UnityEngine;

namespace Traps.SwingingHammerRagdoll
{
    public class HammerTriggerZoneRagdoll : MonoBehaviour
    {
        [SerializeField] private SwingingHammerRagdoll hammerTrap;

        [Header("Activation Rules")]
        [SerializeField] private bool triggerOnPlayer = true;
        [SerializeField] private bool triggerOnEnemy = true;
        [SerializeField] private bool triggerOnAnyCollider = true;

        private void OnTriggerEnter(Collider other)
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