using UnityEngine;
using UnityEngine.AI;

public class EnemyRagdollController : MonoBehaviour
{
    [Header("Main Components")]
    public Animator animator;
    public NavMeshAgent agent;
    public Collider mainCollider;
    public Rigidbody mainRigidbody;
    public EnemyHealth healthManager;

    [Header("Ragdoll Parts")]
    public Rigidbody[] ragdollRigidbodies;
    public Collider[] ragdollColliders;

    public bool isRagdolled;

    private void Awake()
    {
        ragdollRigidbodies = GetComponentsInChildren<Rigidbody>();
        ragdollColliders = GetComponentsInChildren<Collider>();

        ragdollRigidbodies = System.Array.FindAll(
            ragdollRigidbodies,
            rb => rb != mainRigidbody
        );

        ragdollColliders = System.Array.FindAll(
            ragdollColliders,
            col => col != mainCollider
        );

        SetRagdoll(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Launch(transform.forward * 25f + Vector3.up * 150f, transform.position + Vector3.up);
        }
    }

    public void SetRagdoll(bool active)
    {
        isRagdolled = active;

        if (animator != null)
            animator.enabled = !active;

        if (agent != null)
            agent.enabled = !active;

        if (mainCollider != null)
            mainCollider.enabled = !active;

        if (mainRigidbody != null)
            mainRigidbody.isKinematic = active;

        foreach (Rigidbody rb in ragdollRigidbodies)
        {
            rb.isKinematic = !active;
            rb.detectCollisions = active;
        }

        foreach (Collider col in ragdollColliders)
        {
            col.enabled = active;
        }
    }

    public void Launch(Vector3 force, Vector3 hitPoint)
    {
        SetRagdoll(true);

        Rigidbody closest = GetClosestRigidbody(hitPoint);

        if (closest != null)
        {
            closest.AddForceAtPosition(force, hitPoint, ForceMode.Impulse);
        }
        else
        {
            foreach (Rigidbody rb in ragdollRigidbodies)
                rb.AddForce(force, ForceMode.Impulse);
        }
    }

    private Rigidbody GetClosestRigidbody(Vector3 point)
    {
        Rigidbody closest = null;
        float closestDistance = Mathf.Infinity;

        foreach (Rigidbody rb in ragdollRigidbodies)
        {
            float distance = Vector3.Distance(rb.worldCenterOfMass, point);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = rb;
            }
        }

        return closest;
    }

    public void Knockback(Vector3 force, Vector3 hitPoint, float duration)
    {
        Launch(force, hitPoint);

        Invoke(nameof(RecoverFromSlip), duration);
    }

    public void Explosion(Vector3 explosionPosition, float force, float radius, float upwardModifier)
    {
        SetRagdoll(true);

        foreach (Rigidbody rb in ragdollRigidbodies)
        {
            rb.AddExplosionForce(force, explosionPosition, radius, upwardModifier, ForceMode.Impulse);
        }
    }

    public void Slip(float duration)
    {
        SetRagdoll(true);

        if (agent != null)
            agent.enabled = false;
        Launch((-transform.forward) * 25f + Vector3.up * 30f, transform.position + Vector3.up);
        /*
        if (animator != null)
            animator.SetTrigger("Slip");
        */

        Invoke(nameof(RecoverFromSlip), duration);
    }

    private void RecoverFromSlip()
    {
        SetRagdoll(false);

        if (agent != null)
            agent.enabled = true;
    }
}