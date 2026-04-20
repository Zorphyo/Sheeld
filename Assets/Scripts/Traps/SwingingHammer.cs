using System.Collections;
using UnityEngine;

namespace Traps
{
    public class SwingingHammerTrap : MonoBehaviour
    {
        public enum RotationAxis
        {
            X,
            Y,
            Z
        }

        [Header("References")]
        [SerializeField] private Transform pivot;
        [SerializeField] private Collider damageZone;

        [Header("Swing Settings")]
        [SerializeField] private RotationAxis rotationAxis = RotationAxis.Z;

        [Tooltip("Starting angle of the hammer at rest.")]
        [SerializeField] private float restAngle = -60f;

        [Tooltip("Target strike angle.")]
        [SerializeField] private float strikeAngle = 60f;

        [Tooltip("How fast the hammer swings toward the strike angle.")]
        [SerializeField] private float swingSpeed = 220f;

        [Tooltip("How fast the hammer returns to its resting angle.")]
        [SerializeField] private float returnSpeed = 140f;

        [Tooltip("How long the hammer pauses very briefly at the end of the strike.")]
        [SerializeField] private float strikePauseTime = 0.05f;

        [Header("Behavior")]
        [SerializeField] private float cooldownTime = 1.5f;
        [SerializeField] private bool canRetrigger = true;

        private bool isActive = false;
        private bool isOnCooldown = false;

        private void Start()
        {
            if (pivot != null)
            {
                SetPivotAngle(restAngle);
            }

            if (damageZone != null)
            {
                damageZone.enabled = false;
            }
        }

        public void TryActivate()
        {
            if (pivot == null)
            {
                Debug.LogWarning("SwingingHammerTrap: Pivot is not assigned.", this);
                return;
            }

            if (!isActive && !isOnCooldown)
            {
                StartCoroutine(ActivateTrap());
            }
        }

        private IEnumerator ActivateTrap()
        {
            isActive = true;

            if (damageZone != null)
            {
                damageZone.enabled = true;
            }

            // Swing toward strike angle
            while (Mathf.Abs(Mathf.DeltaAngle(GetPivotAngle(), strikeAngle)) > 0.5f)
            {
                float nextAngle = Mathf.MoveTowardsAngle(
                    GetPivotAngle(),
                    strikeAngle,
                    swingSpeed * Time.deltaTime
                );

                SetPivotAngle(nextAngle);
                yield return null;
            }

            SetPivotAngle(strikeAngle);

            if (strikePauseTime > 0f)
            {
                yield return new WaitForSeconds(strikePauseTime);
            }

            if (damageZone != null)
            {
                damageZone.enabled = false;
            }

            // Return to resting position
            while (Mathf.Abs(Mathf.DeltaAngle(GetPivotAngle(), restAngle)) > 0.5f)
            {
                float nextAngle = Mathf.MoveTowardsAngle(
                    GetPivotAngle(),
                    restAngle,
                    returnSpeed * Time.deltaTime
                );

                SetPivotAngle(nextAngle);
                yield return null;
            }

            SetPivotAngle(restAngle);

            isActive = false;

            if (canRetrigger)
            {
                isOnCooldown = true;
                yield return new WaitForSeconds(cooldownTime);
                isOnCooldown = false;
            }
        }

        private float GetPivotAngle()
        {
            Vector3 angles = pivot.localEulerAngles;

            switch (rotationAxis)
            {
                case RotationAxis.X:
                    return angles.x;
                case RotationAxis.Y:
                    return angles.y;
                case RotationAxis.Z:
                    return angles.z;
                default:
                    return angles.z;
            }
        }

        private void SetPivotAngle(float angle)
        {
            Vector3 angles = pivot.localEulerAngles;

            switch (rotationAxis)
            {
                case RotationAxis.X:
                    angles.x = angle;
                    break;
                case RotationAxis.Y:
                    angles.y = angle;
                    break;
                case RotationAxis.Z:
                    angles.z = angle;
                    break;
            }

            pivot.localEulerAngles = angles;
        }
    }
}