using UnityEngine;
using UnityEngine.AI;
using System.Collections;

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
        // TODO: Remove this upon testing completed
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

        StartCoroutine(RecoverFromSlipAfterGrounded(duration));
    }

    public void Explosion(Vector3 explosionPosition, float force, float radius, float upwardModifier, float duration)
    {
        SetRagdoll(true);

        foreach (Rigidbody rb in ragdollRigidbodies)
        {
            rb.AddExplosionForce(force, explosionPosition, radius, upwardModifier, ForceMode.Impulse);
        }

        StartCoroutine(RecoverFromSlipAfterGrounded(duration));
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

        StartCoroutine(RecoverFromSlipAfterGrounded(duration));
    }

    private IEnumerator RecoverFromSlipAfterGrounded(float minimumDelay)
    {
        // Keeps your existing delay behavior first
        yield return new WaitForSeconds(minimumDelay);

        // Wait until the ragdoll is actually touching the ground
        while (!IsRagdollTouchingGround())
        {
            yield return null;
        }

        // Once touching ground, wait 2 more seconds before standing up
        yield return new WaitForSeconds(2f);

        RecoverFromSlip();
    }

    private bool IsRagdollTouchingGround()
    {
        foreach (Collider col in ragdollColliders)
        {
            if (col == null || !col.enabled)
                continue;

            if (Physics.Raycast(col.bounds.center, Vector3.down, col.bounds.extents.y + 0.15f))
            {
                return true;
            }
        }

        return false;
    }

    private void RecoverFromSlip()
    {
        MoveEnemyRootToRagdollLandingSpot();

        StopRagdollMotion();

        SetRagdoll(false);

        if (agent != null)
        {
            agent.enabled = true;
            agent.Warp(transform.position);
        }
    }

    private void MoveEnemyRootToRagdollLandingSpot()
    {
        Rigidbody referenceBody = GetBestBodyForRecovery();

        if (referenceBody == null)
            return;

        Vector3 targetPosition = referenceBody.position;

        // Try to place enemy on NavMesh near where the ragdoll landed
        if (NavMesh.SamplePosition(targetPosition, out NavMeshHit navHit, 3f, NavMesh.AllAreas))
        {
            targetPosition = navHit.position;
        }
        else
        {
            // Fallback: raycast down to actual floor
            Vector3 rayStart = targetPosition + Vector3.up * 2f;

            if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit groundHit, 10f))
            {
                targetPosition = groundHit.point;
            }
        }

        // Prevent spawning halfway through the floor
        float yOffset = 0.1f;

        if (mainCollider != null)
        {
            yOffset += mainCollider.bounds.extents.y;
        }

        transform.position = targetPosition + Vector3.up * yOffset;

        // Optional: face roughly same direction as the ragdoll body
        Vector3 flatForward = referenceBody.transform.forward;
        flatForward.y = 0f;

        if (flatForward.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(flatForward.normalized);
        }
    }

    private Rigidbody GetBestBodyForRecovery()
    {
        // Prefer hips/pelvis/spine if names exist
        foreach (Rigidbody rb in ragdollRigidbodies)
        {
            string lowerName = rb.name.ToLower();

            if (lowerName.Contains("hips") ||
                lowerName.Contains("pelvis") ||
                lowerName.Contains("spine"))
            {
                return rb;
            }
        }

        // Fallback: use the ragdoll body closest to the average position
        if (ragdollRigidbodies.Length == 0)
            return null;

        return ragdollRigidbodies[0];
    }

    private void StopRagdollMotion()
    {
        foreach (Rigidbody rb in ragdollRigidbodies)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    public void DieAsRagdoll(Vector3 extraForce = default, Vector3 hitPoint = default)
    {
        StopAllCoroutines();

        SetRagdoll(true);

        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        if (animator != null)
            animator.enabled = false;

        if (mainCollider != null)
            mainCollider.enabled = false;

        if (mainRigidbody != null)
        {
            mainRigidbody.linearVelocity = Vector3.zero;
            mainRigidbody.angularVelocity = Vector3.zero;
            mainRigidbody.isKinematic = true;
        }

        if (extraForce != Vector3.zero)
        {
            Rigidbody closest = hitPoint == default
                ? GetBestBodyForRecovery()
                : GetClosestRigidbody(hitPoint);

            if (closest != null)
                closest.AddForceAtPosition(extraForce, closest.worldCenterOfMass, ForceMode.Impulse);
        }
    }

    public void PartialBodyHit(Vector3 force, Vector3 hitPoint, float duration)
    {
        if (isRagdolled)
            return;

        Rigidbody hitBody = GetClosestRigidbody(hitPoint);

        if (hitBody == null)
            return;

        StartCoroutine(PartialBodyHitRoutine(hitBody, force, hitPoint, duration));
    }

    private IEnumerator PartialBodyHitRoutine(Rigidbody hitBody, Vector3 force, Vector3 hitPoint, float duration)
    {
        // Temporarily let only this body part react
        hitBody.isKinematic = false;
        hitBody.detectCollisions = true;

        Collider hitCollider = hitBody.GetComponent<Collider>();
        if (hitCollider != null)
            hitCollider.enabled = true;

        hitBody.AddForceAtPosition(force, hitPoint, ForceMode.Impulse);

        yield return new WaitForSeconds(duration);

        hitBody.linearVelocity = Vector3.zero;
        hitBody.angularVelocity = Vector3.zero;

        hitBody.isKinematic = true;

        if (hitCollider != null)
            hitCollider.enabled = false;
    }
}