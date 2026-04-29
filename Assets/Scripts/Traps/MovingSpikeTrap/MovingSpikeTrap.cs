using System.Collections;
using Traps.TrapUsageData;
using UnityEngine;

namespace Traps.MovingSpikeTrap
{
    public class MovingSpikeTrap : MonoBehaviour
    {
        [Header("Trap Data")]
        [SerializeField] private TrapType trapType = TrapType.MovingSpike;

        [Header("References")]
        [SerializeField] private Transform spikesVisual;
        [SerializeField] private Collider damageZone;
        [SerializeField] private Collider triggerZone;

        [Header("Spike Movement settings")]
        [SerializeField] private float riseDistance = 1.5f;
        [SerializeField] private float riseSpeed = 12f;
        [SerializeField] private float stayUpTime = 2f;
        [SerializeField] private float cooldownTime = 1f;
        [SerializeField] private Vector3 riseDirection = Vector3.forward;

        [Header("Behavior")]
        [SerializeField] private bool canRetrigger = true;

        private Vector3 downPosition;
        private Vector3 upPosition;

        private bool isActive = false;
        private bool isOnCooldown = false;

        private void Start()
        {
            downPosition = spikesVisual.localPosition;
            upPosition = downPosition + riseDirection.normalized * riseDistance;

            if (damageZone != null)
            {
                damageZone.enabled = false;
            }
        }

        public void TryActivate()
        {
            if (!isActive && !isOnCooldown)
            {
                StartCoroutine(ActivateTrap());
            }
        }

        private IEnumerator ActivateTrap()
        {
            isActive = true;

            if (TrapStatsManager.Instance != null)
            {
                TrapStatsManager.Instance.RecordTrapEvent(trapType, TrapEventType.Triggered);
            }

            if (damageZone != null)
            {
                damageZone.enabled = true;
            }

            while (Vector3.Distance(spikesVisual.localPosition, upPosition) > 0.01f)
            {
                spikesVisual.localPosition = Vector3.MoveTowards(
                    spikesVisual.localPosition,
                    upPosition,
                    riseSpeed * Time.deltaTime
                );

                yield return null;
            }

            spikesVisual.localPosition = upPosition;

            yield return new WaitForSeconds(stayUpTime);

            while (Vector3.Distance(spikesVisual.localPosition, downPosition) > 0.01f)
            {
                spikesVisual.localPosition = Vector3.MoveTowards(
                    spikesVisual.localPosition,
                    downPosition,
                    riseSpeed * Time.deltaTime
                );

                yield return null;
            }

            spikesVisual.localPosition = downPosition;

            if (damageZone != null)
            {
                damageZone.enabled = false;
            }

            isActive = false;

            if (canRetrigger)
            {
                isOnCooldown = true;
                yield return new WaitForSeconds(cooldownTime);
                isOnCooldown = false;
            }
        }
    }
}