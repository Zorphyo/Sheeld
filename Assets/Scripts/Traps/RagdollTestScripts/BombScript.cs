using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BombScript : MonoBehaviour
{
    [Header("Launch Settings")]
    public Transform playerCamera;
    public float forwardForce = 12f;
    public float upwardForce = 4f;
    public float lifetimeBeforeExplosion = 3f;

    [Header("Explosion Settings")]
    public float explosionForce = 80f;
    public float explosionRadius = 20f;
    public float upwardModifier = 3f;
    public float ragdollDuration = 7f;

    [Header("Immediate Damage")]
    public int maxDamage = 60;
    public int minDamage = 10;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        if (playerCamera == null && Camera.main != null)
            playerCamera = Camera.main.transform;

        LaunchBomb();
        StartCoroutine(ExplodeAfterDelay());
    }

    private void LaunchBomb()
    {
        if (playerCamera == null)
        {
            Debug.LogWarning("BombScript: No playerCamera assigned and no Main Camera found.");
            return;
        }

        Vector3 launchDirection = playerCamera.forward;
        launchDirection.y = 0f;
        launchDirection.Normalize();

        Vector3 force = launchDirection * forwardForce + Vector3.up * upwardForce;
        rb.AddForce(force, ForceMode.Impulse);
    }

    private IEnumerator ExplodeAfterDelay()
    {
        yield return new WaitForSeconds(lifetimeBeforeExplosion);

        Explode();

        Destroy(gameObject);
    }

    private void Explode()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);

        HashSet<EnemyRagdollController> damagedEnemies = new HashSet<EnemyRagdollController>();

        foreach (Collider hit in hits)
        {
            EnemyRagdollController enemy = hit.GetComponentInParent<EnemyRagdollController>();

            if (enemy == null)
                continue;

            // Prevent damaging the same enemy multiple times because ragdolls have many colliders.
            if (damagedEnemies.Contains(enemy))
                continue;

            damagedEnemies.Add(enemy);

            // First apply the physical explosion.
            enemy.Explosion(
                transform.position,
                explosionForce,
                explosionRadius,
                upwardModifier,
                ragdollDuration
            );

            // Then apply immediate damage.
            EnemyHealth health = enemy.healthManager;

            if (health == null)
                health = enemy.GetComponent<EnemyHealth>();

            if (health != null)
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                float distancePercent = Mathf.Clamp01(distance / explosionRadius);

                // 1 near center, 0 near edge
                float damagePercent = 1f - distancePercent;

                int damage = Mathf.RoundToInt(Mathf.Lerp(minDamage, maxDamage, damagePercent));

                health.TakeDamage(damage);
            }
        }
    }
}