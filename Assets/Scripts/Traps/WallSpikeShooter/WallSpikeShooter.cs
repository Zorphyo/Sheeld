using UnityEngine;

namespace Traps.WallSpikeShooter
{
    public class WallSpikeShooter : MonoBehaviour
    {
        public enum SpawnRootShootAxis
        {
            Forward,
            Backward,
            Right,
            Left,
            Up,
            Down
        }

        [Header("References")]
        [SerializeField] private GameObject spikeProjectilePrefab;
        [SerializeField] private Transform spawnRoot;
        [SerializeField] private Transform projectileParent;

        [Header("Direction")]
        [Tooltip("Which SpawnRoot axis points out from the wall.")]
        [SerializeField] private SpawnRootShootAxis shootAxis = SpawnRootShootAxis.Right;
        [SerializeField] private Vector3 projectileRotationOffsetEuler = Vector3.zero;

        [Header("Shot Pattern")]
        [SerializeField] private int spikeCount = 18;
        [SerializeField] private float spacing = 0.18f;

        [Tooltip("Maximum outward fan angle for the far-left and far-right spikes.")]
        [SerializeField] private float maxFanYaw = 25f;

        [SerializeField] private float randomYawJitter = 4f;
        [SerializeField] private float randomPitchJitter = 2f;

        [Header("Projectile")]
        [SerializeField] private float projectileSpeed = 25f;

        [Header("Cooldown")]
        [SerializeField] private float cooldownTime = 3f;

        private bool isOnCooldown = false;

        public void TryShoot()
        {
            Debug.Log("WallSpikeShooter: TryShoot called.", this);

            if (isOnCooldown)
            {
                Debug.Log("WallSpikeShooter: Still on cooldown.", this);
                return;
            }

            Shoot();
            StartCoroutine(CooldownRoutine());
        }

        private void Shoot()
        {
            Debug.Log("WallSpikeShooter: Shoot started.", this);
            if (spikeProjectilePrefab == null)
            {
                Debug.LogWarning("WallSpikeShooter: spikeProjectilePrefab is not assigned.", this);
                return;
            }

            if (spawnRoot == null)
            {
                Debug.LogWarning("WallSpikeShooter: spawnRoot is not assigned.", this);
                return;
            }

            Debug.Log("WallSpikeShooter: Shooting " + spikeCount + " spikes.", this);

            for (int i = 0; i < spikeCount; i++)
            {
                SpawnSpike(i);
            }
        }

        private void SpawnSpike(int index)
        {
            float centerIndex = (spikeCount - 1) / 2f;
            float offsetFromCenter = index - centerIndex;

            Vector3 baseDirection = GetShootDirection();
            Vector3 horizontalSpreadAxis = Vector3.Cross(Vector3.up, baseDirection).normalized;

            if (horizontalSpreadAxis.sqrMagnitude < 0.001f)
            {
                horizontalSpreadAxis = spawnRoot.right;
            }

            Vector3 spawnPosition = spawnRoot.position + horizontalSpreadAxis * (offsetFromCenter * spacing);

            float normalizedFromCenter = centerIndex == 0f ? 0f : offsetFromCenter / centerIndex;

            float fanYaw = normalizedFromCenter * maxFanYaw;
            float randomYaw = Random.Range(-randomYawJitter, randomYawJitter);
            float randomPitch = Random.Range(-randomPitchJitter, randomPitchJitter);

            Quaternion fanRotation =
                Quaternion.AngleAxis(fanYaw + randomYaw, Vector3.up) *
                Quaternion.AngleAxis(randomPitch, horizontalSpreadAxis);

            Vector3 finalDirection = fanRotation * baseDirection;
            finalDirection.Normalize();

            Quaternion visualRotation =
                Quaternion.LookRotation(finalDirection, Vector3.up) *
                Quaternion.Euler(projectileRotationOffsetEuler);

            GameObject spike = Instantiate(
                spikeProjectilePrefab,
                spawnPosition,
                visualRotation
            );
            
            Debug.Log("WallSpikeShooter: Spawned spike " + index, spike);

            if (projectileParent != null)
            {
                spike.transform.SetParent(projectileParent);
            }

            IceSpikeProjectile projectile = spike.GetComponent<IceSpikeProjectile>();

            if (projectile == null)
            {
                Debug.LogWarning("WallSpikeShooter: Spawned spike is missing IceSpikeProjectile script.", spike);
                return;
            }

            projectile.Launch(finalDirection, projectileSpeed);
        }

        private Vector3 GetShootDirection()
        {
            switch (shootAxis)
            {
                case SpawnRootShootAxis.Forward:
                    return spawnRoot.forward;
                case SpawnRootShootAxis.Backward:
                    return -spawnRoot.forward;
                case SpawnRootShootAxis.Right:
                    return spawnRoot.right;
                case SpawnRootShootAxis.Left:
                    return -spawnRoot.right;
                case SpawnRootShootAxis.Up:
                    return spawnRoot.up;
                case SpawnRootShootAxis.Down:
                    return -spawnRoot.up;
                default:
                    return spawnRoot.forward;
            }
        }

        private System.Collections.IEnumerator CooldownRoutine()
        {
            isOnCooldown = true;
            yield return new WaitForSeconds(cooldownTime);
            isOnCooldown = false;
        }
    }
}