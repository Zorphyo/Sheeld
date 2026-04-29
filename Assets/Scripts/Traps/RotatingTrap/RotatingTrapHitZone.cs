using System.Collections;
using Traps.TrapUsageData;
using UnityEngine;

namespace Traps.RotatingTrap
{
    public class SwingingTrap : MonoBehaviour
    {
        public enum RotationAxis
        {
            X,
            Y,
            Z
        }

        [Header("Trap Data")]
        [SerializeField] private TrapType trapType = TrapType.SwingingTrap;

        [Header("References")]
        [SerializeField] private Transform pivot;
        [SerializeField] private Collider damageZone;

        [Header("Rotation")]
        [SerializeField] private RotationAxis rotationAxis = RotationAxis.Z;

        [SerializeField] private float hiddenAngle = 0f;
        [SerializeField] private float strikeAngle = -90f;
        [SerializeField] private float loopReturnAngle = -360f;

        [Header("Speed")]
        [SerializeField] private float strikeSpeed = 360f;
        [SerializeField] private float loopSpeed = 220f;

        [Header("Damage Timing")]
        [SerializeField] private float damageStartAngle = -20f;
        [SerializeField] private float damageEndAngle = -140f;

        [Header("Behavior")]
        [SerializeField] private float cooldownTime = 2f;
        [SerializeField] private bool canRetrigger = true;

        private bool isActive = false;
        private bool isOnCooldown = false;

        private void Start()
        {
            if (pivot == null)
            {
                Debug.LogWarning("SwingingHammer: Pivot is not assigned.", this);
                return;
            }

            SetPivotAngle(hiddenAngle);

            if (damageZone != null)
            {
                damageZone.enabled = false;
            }
        }

        public void TryActivate()
        {
            if (pivot == null)
                return;

            if (isActive || isOnCooldown)
                return;

            StartCoroutine(ActivateTrap());
        }

        private IEnumerator ActivateTrap()
        {
            isActive = true;

            Record(TrapEventType.Triggered);

            float currentAngle = hiddenAngle;

            if (damageZone != null)
            {
                damageZone.enabled = false;
            }

            while (currentAngle > strikeAngle)
            {
                currentAngle -= strikeSpeed * Time.deltaTime;

                if (currentAngle < strikeAngle)
                {
                    currentAngle = strikeAngle;
                }

                SetPivotAngle(currentAngle);
                UpdateDamageZone(currentAngle);

                yield return null;
            }

            while (currentAngle > loopReturnAngle)
            {
                currentAngle -= loopSpeed * Time.deltaTime;

                if (currentAngle < loopReturnAngle)
                {
                    currentAngle = loopReturnAngle;
                }

                SetPivotAngle(currentAngle);
                UpdateDamageZone(currentAngle);

                yield return null;
            }

            SetPivotAngle(hiddenAngle);

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

        private void UpdateDamageZone(float currentAngle)
        {
            if (damageZone == null)
                return;

            bool active = currentAngle <= damageStartAngle && currentAngle >= damageEndAngle;
            damageZone.enabled = active;
        }

        private void SetPivotAngle(float angle)
        {
            Vector3 euler = pivot.localEulerAngles;

            switch (rotationAxis)
            {
                case RotationAxis.X:
                    euler.x = angle;
                    break;

                case RotationAxis.Y:
                    euler.y = angle;
                    break;

                case RotationAxis.Z:
                    euler.z = angle;
                    break;
            }

            pivot.localEulerAngles = euler;
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