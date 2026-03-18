using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    // Agent rotation is disabled — EnemyLocomotion handles all turning
    private NavMeshAgent agent;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false; // hand rotation to EnemyLocomotion
    }

    public void MoveTo(Vector3 targetPosition)
    {
        if (!agent.isOnNavMesh || !agent.enabled) return;
        agent.isStopped = false;
        agent.SetDestination(targetPosition);
    }

    public void StopMoving()
    {
        if (!agent.isOnNavMesh || !agent.enabled) return;
        agent.isStopped = true;
    }

    public float GetSpeed() => agent.velocity.magnitude;
}