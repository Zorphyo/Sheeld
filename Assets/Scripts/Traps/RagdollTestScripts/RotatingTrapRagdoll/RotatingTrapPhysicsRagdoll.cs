using UnityEngine;

namespace Traps.RotatingLogRagdoll
{
    public class RotatingTrapPhysicsRagdoll : MonoBehaviour
    {
        [SerializeField] private float torqueForce = 1000f;
        [SerializeField] private float maxAngularSpeed = 15f; // radians per second

        private Rigidbody rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            /*
            EnemyRagdollController enemy = rb.GetComponentInParent<EnemyRagdollController>();

            if (enemy == null)
                return;

            enemy.Knockback((-transform.forward) * 100f + Vector3.up * 150f, transform.position + Vector3.up, 15f);
            */

            /* Ruba's original code below
            // Apply torque
            rb.AddTorque(Vector3.up * torqueForce);

            // Clamp max spin speed
            if (rb.angularVelocity.magnitude > maxAngularSpeed)
            {
                rb.angularVelocity = rb.angularVelocity.normalized * maxAngularSpeed;
            }
            */
        }
    }
}
