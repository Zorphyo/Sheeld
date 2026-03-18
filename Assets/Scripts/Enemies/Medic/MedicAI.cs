using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class MedicAI : MonoBehaviour
{
    [Header("Settings")]
    public float reviveRadius    = 3f;    // Must be within this distance to revive
    public float reviveTickRate  = 0.5f;  // How often we check if we're in range

    private NavMeshAgent    agent;
    private EnemyHealth     currentTarget;
    private float           reviveCheckTimer;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void OnEnable()
    {
        if (MedicManager.Instance != null)
            MedicManager.Instance.RegisterMedic(this);
    }

    void OnDisable()
    {
        if (MedicManager.Instance != null)
            MedicManager.Instance.UnregisterMedic(this);
    }

    void Update()
    {
        FindTarget();
        MoveToTarget();
        CheckRevive();
    }

    void FindTarget()
    {
        // Drop target if they've been revived or died
        if (currentTarget != null && !currentTarget.isDBNO)
            currentTarget = null;

        // Find a new one if we don't have one
        if (currentTarget == null && MedicManager.Instance != null)
            currentTarget = MedicManager.Instance.GetClosestDBNOEnemy(transform.position);
    }

    void MoveToTarget()
    {
        if (currentTarget == null)
        {
            agent.ResetPath();
            return;
        }

        agent.SetDestination(currentTarget.transform.position);
    }

    void CheckRevive()
    {
        if (currentTarget == null) return;

        reviveCheckTimer -= Time.deltaTime;
        if (reviveCheckTimer > 0f) return;
        reviveCheckTimer = reviveTickRate;

        float dist = Vector3.Distance(transform.position, currentTarget.transform.position);
        if (dist <= reviveRadius)
            currentTarget.ReceiveRevive();
    }

    // Called externally when this medic dies
    public void Die()
    {
        // OnDisable handles UnregisterMedic automatically
        Destroy(gameObject, 3f);
    }
}