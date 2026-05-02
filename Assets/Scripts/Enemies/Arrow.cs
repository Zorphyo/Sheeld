using UnityEngine;

public class Arrow : MonoBehaviour
{
    public float speed = 25f;
    public float lifetime = 5f;
    public int damage = 10;

    private Rigidbody rb;
    private bool hasHit = false;

    // Improved Sticking logic
    private Transform stuckTarget;
    private Vector3 relativePosition;
    private Quaternion relativeRotation;

    private Vector3 lastPosition;
    private static readonly Quaternion meshOffset = Quaternion.Euler(90f, 0f, 0f);

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        // ContinuousDynamic is key for high-speed projectiles
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    public void Launch(Vector3 direction)
    {
        direction.Normalize();
        rb.linearVelocity = direction * speed;
        transform.rotation = Quaternion.LookRotation(direction) * meshOffset;
        lastPosition = transform.position;

        Destroy(gameObject, lifetime);
    }

    void FixedUpdate()
    {
        if (hasHit) return;

        // 1. Orient arrow to travel direction
        if (rb.linearVelocity.sqrMagnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(rb.linearVelocity) * meshOffset;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 10f);
        }

        // 2. Manual Raycast Ray-bundle (Prevents tunneling)
        Vector3 currentPosition = transform.position;
        Vector3 travelVec = currentPosition - lastPosition;
        float distance = travelVec.magnitude;

        if (distance > 0f)
        {
            // We use travelVec.normalized instead of 'direction'
            if (Physics.Raycast(lastPosition, travelVec.normalized, out RaycastHit hit, distance))
            {
                // Ensure we don't hit our own shooter or triggers
                if (!hit.collider.isTrigger)
                {
                    HandleHit(hit.collider, hit.point, travelVec.normalized);
                    return;
                }
            }
        }
        lastPosition = currentPosition;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasHit) return;
        
        ContactPoint contact = collision.contacts[0];
        HandleHit(collision.collider, contact.point, rb.linearVelocity.normalized);
    }

    private void HandleHit(Collider other, Vector3 hitPoint, Vector3 direction)
    {
        if (hasHit) return;
        hasHit = true;

        // Stop all physics immediately
        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.detectCollisions = false;

        // Damage logic
        if (other.CompareTag("Player"))
        {
            if (other.TryGetComponent(out PlayerStats stats))
                stats.TakeDamage(damage);
        }

        // Snap to hit location
        transform.position = hitPoint;
        transform.rotation = Quaternion.LookRotation(direction) * meshOffset;

        // Rotation-aware sticking
        stuckTarget = other.transform;
        relativePosition = stuckTarget.InverseTransformPoint(transform.position);
        relativeRotation = Quaternion.Inverse(stuckTarget.rotation) * transform.rotation;

        Destroy(gameObject, 10f); 
    }

    void LateUpdate()
    {
        if (hasHit && stuckTarget != null)
        {
            // Keeps arrow stuck to moving/rotating targets
            transform.position = stuckTarget.TransformPoint(relativePosition);
            transform.rotation = stuckTarget.rotation * relativeRotation;
        }
    }
}