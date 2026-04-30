using UnityEngine;
using Traps.TrapUsageData;

public class BallistaMountedWeapon : MountedWeapon
{
    [Header("Ballista")]
    public GameObject boltPrefab;
    public Transform firePoint;
    public float boltSpeed = 45f;
    public float cooldown = 70f;

    [Header("Trap Data")]
    [SerializeField] private TrapType trapType = TrapType.Throwable;

    private float lastFireTime = -999f;

    public override void UseWeapon()
    {
        if (Time.time < lastFireTime + cooldown)
        {
            float remaining = (lastFireTime + cooldown) - Time.time;
            Debug.Log($"Ballista cooling down. Remaining: {remaining:F1}s");
            return;
        }

        if (boltPrefab == null || firePoint == null)
        {
            Debug.LogWarning("Ballista missing bolt prefab or fire point.");
            return;
        }

        GameObject boltObj = Instantiate(
            boltPrefab,
            firePoint.position,
            firePoint.rotation
        );

        BallistaBoltProjectile bolt = boltObj.GetComponent<BallistaBoltProjectile>();

        if (bolt != null)
        {
            bolt.Launch(firePoint.forward, boltSpeed, gameObject);
        }

        if (TrapStatsManager.Instance != null)
        {
            TrapStatsManager.Instance.RecordUniqueTrapUsed(gameObject);
            TrapStatsManager.Instance.RecordTrapEvent(trapType, TrapEventType.Triggered);
        }

        lastFireTime = Time.time;
    }
}