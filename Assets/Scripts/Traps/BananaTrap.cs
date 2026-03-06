using UnityEngine;

public class BananaTrap : MonoBehaviour
{
    [SerializeField] private float destroyAfterTriggerDelay = 0.2f;
    [SerializeField] private bool destroyOnTrigger = true;

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

        // Later we can do enemy here too
        // if (other.CompareTag("Enemy")) { ... }
    }
}
