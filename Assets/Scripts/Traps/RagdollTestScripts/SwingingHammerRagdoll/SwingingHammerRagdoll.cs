using System.Collections;
using UnityEngine;

namespace Traps.SwingingHammerRagdoll
{
    public class SwingingHammerRagdoll : MonoBehaviour
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

        [Header("Rotation")]
        [SerializeField] private RotationAxis rotationAxis = RotationAxis.Z;

        [Tooltip("The hidden/resting angle. Example: 0")]
        [SerializeField] private float hiddenAngle = 0f;

        [Tooltip("The fast downward strike angle. Example: -90")]
        [SerializeField] private float strikeAngle = -90f;

        [Tooltip("The final angle after completing the full loop. Example: -360")]
        [SerializeField] private float loopReturnAngle = -360f;

        [Header("Speed")]
        [SerializeField] private float strikeSpeed = 360f;
        [SerializeField] private float loopSpeed = 220f;

        [Header("Damage Timing")]
        [Tooltip("Damage becomes active once the hammer rotates past this angle.")]
        [SerializeField] private float damageStartAngle = -20f;

        [Tooltip("Damage stops after the hammer rotates past this angle.")]
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

            float currentAngle = hiddenAngle;

            if (damageZone != null)
            {
                damageZone.enabled = false;
            }

            // Phase 1: fast drop from hidden to strike
            while (currentAngle > strikeAngle)
            {
                currentAngle -= strikeSpeed * Time.deltaTime;
                if (currentAngle < strikeAngle)
                    currentAngle = strikeAngle;

                SetPivotAngle(currentAngle);
                UpdateDamageZone(currentAngle);
                yield return null;
            }

            // Phase 2: continue full loop back into hiding
            while (currentAngle > loopReturnAngle)
            {
                currentAngle -= loopSpeed * Time.deltaTime;
                if (currentAngle < loopReturnAngle)
                    currentAngle = loopReturnAngle;

                SetPivotAngle(currentAngle);
                UpdateDamageZone(currentAngle);
                yield return null;
            }

            // Snap cleanly back to hidden pose
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

            damageZone.enabled = true;
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
    }
}