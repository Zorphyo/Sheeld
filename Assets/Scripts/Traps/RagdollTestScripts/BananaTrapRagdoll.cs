using Traps.TrapUsageData;
using UnityEngine;

public class BananaTrapRagdoll : MonoBehaviour
{
    [SerializeField] private float destroyAfterTriggerDelay = 0.2f;
    [SerializeField] private bool destroyOnTrigger = true;
    [SerializeField] public float slipDuration = 3f;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;

        // Player
        if (other.CompareTag("Player"))
        {
            hasTriggered = true;

            // call a trip behavior on the player
            // TODO: implement trip behavior on player 
            /*PlayerTrip playerTrip = other.GetComponent<PlayerTrip>();
            if (playerTrip != null)
            {
                playerTrip.Trip();
            }
            
            else
            {
                Debug.LogWarning("Player entered banana trap, but no PlayerTrip component was found.");
            }
            */
            if (destroyOnTrigger)
            {
                Destroy(gameObject, destroyAfterTriggerDelay);
            }
        }

        if (other.CompareTag("Enemy")) {
            if (TrapStatsManager.Instance != null)
            {
                TrapStatsManager.Instance.RecordUniqueTrapUsed(gameObject);
            }

            EnemyRagdollController enemy = other.GetComponentInParent<EnemyRagdollController>();

            if (enemy == null)
                return;

            enemy.Slip(slipDuration);
            Destroy(gameObject, destroyAfterTriggerDelay);
        }
    }
}
