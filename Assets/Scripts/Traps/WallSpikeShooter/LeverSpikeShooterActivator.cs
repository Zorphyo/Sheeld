using System.Collections;
using UnityEngine;
using Core.Interfaces;
using Traps.TrapUsageData;

namespace Traps.WallSpikeShooter
{
    public class LeverSpikeShooterActivator : MonoBehaviour, IInteractable
    {
        [Header("Trap Data")]
        [SerializeField] private TrapType trapType = TrapType.WallSpikeShooter;

        [Header("References")]
        [SerializeField] private Transform leverHandle;
        [SerializeField] private WallSpikeShooter spikeShooter;

        [Header("Interaction")]
        [SerializeField] private string playerTag = "Player";

        [Header("Lever Animation")]
        [SerializeField] private Vector3 pulledLocalEulerOffset = new Vector3(-35f, 0f, 0f);
        [SerializeField] private float pullSpeed = 240f;
        [SerializeField] private float returnSpeed = 180f;
        [SerializeField] private float pulledHoldTime = 0.08f;

        [Header("Behavior")]
        [SerializeField] private bool requirePlayerTrigger = true;
        [SerializeField] private float leverCooldown = 3f;

        private bool playerInRange = false;
        private bool isBusy = false;
        private bool isOnCooldown = false;

        private Vector3 restLocalEuler;
        private Vector3 pulledLocalEuler;

        private void Start()
        {
            if (leverHandle != null)
            {
                restLocalEuler = leverHandle.localEulerAngles;
                pulledLocalEuler = restLocalEuler + pulledLocalEulerOffset;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(playerTag))
            {
                playerInRange = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag(playerTag))
            {
                playerInRange = false;
            }
        }

        public void Interact()
        {
            if (requirePlayerTrigger && !playerInRange)
                return;

            if (isBusy || isOnCooldown)
                return;

            Record(TrapEventType.Interacted);

            StartCoroutine(PullLeverRoutine());
        }

        private IEnumerator PullLeverRoutine()
        {
            if (leverHandle == null)
            {
                Debug.LogWarning("LeverSpikeShooterActivator: leverHandle is not assigned.", this);
                yield break;
            }

            if (spikeShooter == null)
            {
                Debug.LogWarning("LeverSpikeShooterActivator: spikeShooter is not assigned.", this);
                yield break;
            }

            isBusy = true;

            while (Quaternion.Angle(leverHandle.localRotation, Quaternion.Euler(pulledLocalEuler)) > 0.5f)
            {
                leverHandle.localRotation = Quaternion.RotateTowards(
                    leverHandle.localRotation,
                    Quaternion.Euler(pulledLocalEuler),
                    pullSpeed * Time.deltaTime
                );

                yield return null;
            }

            leverHandle.localRotation = Quaternion.Euler(pulledLocalEuler);

            Record(TrapEventType.Triggered);

            spikeShooter.TryShoot();

            if (pulledHoldTime > 0f)
            {
                yield return new WaitForSeconds(pulledHoldTime);
            }

            while (Quaternion.Angle(leverHandle.localRotation, Quaternion.Euler(restLocalEuler)) > 0.5f)
            {
                leverHandle.localRotation = Quaternion.RotateTowards(
                    leverHandle.localRotation,
                    Quaternion.Euler(restLocalEuler),
                    returnSpeed * Time.deltaTime
                );

                yield return null;
            }

            leverHandle.localRotation = Quaternion.Euler(restLocalEuler);

            isBusy = false;
            isOnCooldown = true;

            yield return new WaitForSeconds(leverCooldown);

            isOnCooldown = false;
        }

        private void Record(TrapEventType eventType)
        {
            if (TrapStatsManager.Instance != null)
            {
                TrapStatsManager.Instance.RecordTrapEvent(trapType, eventType);
            }
        }
    }
}