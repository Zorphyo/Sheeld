using System.Collections;
using Core.Interfaces;
using Traps.TrapUsageData;
using UnityEngine;

namespace Traps.RollingTraps
{
    public class LeverTrapActivator : MonoBehaviour, IInteractable
    {
        public enum SpawnRotationMode
        {
            UseSpawnPointFullRotation,
            UseSpawnPointYawOnly,
            UsePrefabRotationOnly
        }

        [Header("Trap Data")]
        [SerializeField] private TrapType trapType = TrapType.RollingTrap;

        [Header("References")]
        [SerializeField] private Transform leverHandle;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private GameObject spawnedTrapPrefab;
        [SerializeField] private Transform spawnedTrapParent;

        [Header("Interaction")]
        [SerializeField] private string playerTag = "Player";
        [SerializeField] private float reuseCooldown = 4f;

        [Header("Lever Animation")]
        [Tooltip("How much the handle rotates locally when pulled.")]
        [SerializeField] private Vector3 pulledLocalEulerOffset = new Vector3(-35f, 0f, 0f);
        [SerializeField] private float pullSpeed = 240f;
        [SerializeField] private float returnSpeed = 180f;
        [SerializeField] private float pulledHoldTime = 0.08f;

        [Header("Spawn Rotation")]
        [SerializeField] private SpawnRotationMode spawnRotationMode = SpawnRotationMode.UseSpawnPointYawOnly;

        [Header("Behavior")]
        [SerializeField] private bool requirePlayerTrigger = true;

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

            StartCoroutine(PullAndSpawnRoutine());
        }

        private IEnumerator PullAndSpawnRoutine()
        {
            if (leverHandle == null)
            {
                Debug.LogWarning("LeverBallSpawner: leverHandle is not assigned.", this);
                yield break;
            }

            if (spawnPoint == null)
            {
                Debug.LogWarning("LeverBallSpawner: spawnPoint is not assigned.", this);
                yield break;
            }

            if (spawnedTrapPrefab == null)
            {
                Debug.LogWarning("LeverBallSpawner: spawnedTrapPrefab is not assigned.", this);
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

            Quaternion spawnRotation = GetSpawnRotation();

            Record(TrapEventType.Triggered);

            GameObject spawnedTrap = Instantiate(
                spawnedTrapPrefab,
                spawnPoint.position,
                spawnRotation
            );

            if (spawnedTrapParent != null)
            {
                spawnedTrap.transform.SetParent(spawnedTrapParent);
            }

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

            yield return new WaitForSeconds(reuseCooldown);

            isOnCooldown = false;
        }

        private Quaternion GetSpawnRotation()
        {
            switch (spawnRotationMode)
            {
                case SpawnRotationMode.UseSpawnPointFullRotation:
                    return spawnPoint.rotation;

                case SpawnRotationMode.UseSpawnPointYawOnly:
                    return Quaternion.Euler(0f, spawnPoint.eulerAngles.y, 0f);

                case SpawnRotationMode.UsePrefabRotationOnly:
                    return spawnedTrapPrefab.transform.rotation;

                default:
                    return spawnPoint.rotation;
            }
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