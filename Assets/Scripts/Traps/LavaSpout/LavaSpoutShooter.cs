using UnityEngine;
using Traps.TrapUsageData;

namespace Traps.LavaSpout
{
    public class LavaSpoutShooter : MonoBehaviour
    {
        [Header("Trap Data")]
        [Tooltip("Used for trap usage data logging.")]
        public TrapType trapType = TrapType.LavaSpout;

        [Header("References")]
        public GameObject fireballPrefab;
        public Transform fireballSpawnPoint;

        [Header("Shooting Timing")]
        public float shootInterval = 1.2f;
        public float firstShotDelay = 0.5f;

        [Header("Launch Force")]
        public float upwardForce = 12f;
        public float outwardForce = 5f;
        public float randomSideForce = 2f;
        
        [Header("Size Variation")]
        public float minScale = 0.8f;
        public float maxScale = 1.4f;

        [Header("Randomness")]
        public bool randomizeDirection = true;

        private float nextShootTime;

        private void Start()
        {
            nextShootTime = Time.time + firstShotDelay;
        }

        private void Update()
        {
            if (Time.time >= nextShootTime)
            {
                ShootFireball();
                nextShootTime = Time.time + shootInterval;
            }
        }

        private void ShootFireball()
        {
            if (fireballPrefab == null || fireballSpawnPoint == null)
            {
                Debug.LogWarning("LavaSpoutShooter: Missing fireballPrefab or fireballSpawnPoint.");
                return;
            }

            GameObject fireball = Instantiate(
                fireballPrefab,
                fireballSpawnPoint.position,
                Quaternion.identity
            );
            
            float randomScale = Random.Range(minScale, maxScale);
            fireball.transform.localScale = Vector3.one * randomScale;
            
            // apply damage scaling 
            FireballProjectile proj = fireball.GetComponent<FireballProjectile>();
            if (proj != null)
            {
                proj.trapType = trapType;
                proj.SetScale(randomScale);
            }

            Rigidbody rb = fireball.GetComponent<Rigidbody>();
            
            if (rb != null)
            {
                rb.mass *= randomScale; // bigger = heavier
            }

            if (rb == null)
            {
                Debug.LogWarning("LavaSpoutShooter: Fireball prefab needs a Rigidbody.");
                return;
            }

            Vector3 upward = Vector3.up * upwardForce;

            Vector3 outward = transform.forward * outwardForce;

            if (randomizeDirection)
            {
                Vector3 randomFlatDirection = new Vector3(
                    Random.Range(-1f, 1f),
                    0f,
                    Random.Range(-1f, 1f)
                ).normalized;

                outward = randomFlatDirection * outwardForce;
            }

            Vector3 sideRandom = new Vector3(
                Random.Range(-randomSideForce, randomSideForce),
                0f,
                Random.Range(-randomSideForce, randomSideForce)
            );

            Vector3 launchVelocity = upward + outward + sideRandom;

            rb.linearVelocity = launchVelocity;
        }
    }
}