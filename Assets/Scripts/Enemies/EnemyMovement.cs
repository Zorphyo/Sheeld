using UnityEngine;
using UnityEngine.AI;

// Thin wrapper around NavMeshAgent that exposes simple movement commands to EnemyBrain
// and a speed value to EnemyAnimator. Requires a NavMeshAgent component and a baked NavMesh.
public class EnemyMovement : MonoBehaviour
{
    private NavMeshAgent agent;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    // Resumes the agent and plots a path to the target (called every frame by EnemyBrain while chasing)
    public void MoveTo(Vector3 targetPosition)
    {
        if (agent == null || agent.isStopped) return;

        agent.SetDestination(targetPosition);
    }
    // Halts the agent in place; does not clear the existing path
    public void StopMoving()
    {
        agent.isStopped = true;
    }

    // Returns world-space speed; used by EnemyAnimator to drive the "Speed" blend parameter
    public float GetSpeed() => agent.velocity.magnitude;
}