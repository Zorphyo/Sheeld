using UnityEngine;

namespace Traps
{
    public class RotatingTrapPhysics : MonoBehaviour
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
            // Apply torque
            rb.AddTorque(Vector3.up * torqueForce);

            // Clamp max spin speed
            if (rb.angularVelocity.magnitude > maxAngularSpeed)
            {
                rb.angularVelocity = rb.angularVelocity.normalized * maxAngularSpeed;
            }
        }
    }
}
