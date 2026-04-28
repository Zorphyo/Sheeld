using UnityEngine;

public class Arrow : MonoBehaviour
{
    public float speed = 25f;
    public float lifetime = 5f;
    public int damage = 10;

    private Rigidbody rb;
    private bool hasHit = false;

    // For sticking without parenting
    private Transform stuckTarget;
    private Vector3 offset;
    private Vector3 lastPosition;
    private static readonly Quaternion meshOffset = Quaternion.Euler(90f, 0f, 0f);



    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;

        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    // Call this after instantiating
    public void Launch(Vector3 direction)
    {
        Debug.Log("Launch direction: " + direction);
        direction.Normalize();

        rb.linearVelocity = direction * speed;
        transform.rotation = Quaternion.LookRotation(direction) * meshOffset;
        lastPosition = transform.position;

        Destroy(gameObject, lifetime);
    }

    void FixedUpdate()
    {
        if (hasHit) return;

        if (rb.linearVelocity.sqrMagnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(rb.linearVelocity) * meshOffset;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 10f);

            Vector3 currentPosition = transform.position;
            Vector3 direction = currentPosition - lastPosition;
            float distance = direction.magnitude;

            if (distance > 0f)
            {
                RaycastHit hit;
                if (Physics.Raycast(lastPosition, direction.normalized, out hit, distance))
                {
                    if (!hit.collider.isTrigger && !hit.collider.CompareTag("Player"))
                    {
                        transform.position = hit.point;
                        transform.rotation = Quaternion.LookRotation(direction) * meshOffset;
                        StickTo(null);
                        return;
                    }
                }
            }

            lastPosition = currentPosition;


        }
    }

    void LateUpdate()
    {
        // Follow target position ONLY (ignore rotation)
        if (stuckTarget != null)
        {
            transform.position = stuckTarget.position + offset;
        }
    }

    private void StickTo(Transform target)
    {
        hasHit = true;

        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;
        rb.detectCollisions = false;

        if (target != null)
        {
            stuckTarget = target;
            offset = transform.position - stuckTarget.position;
        }

        Destroy(gameObject, 5f);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (hasHit) return;

        if (collision.collider.CompareTag("Player"))
        {
            PlayerStats ph = collision.collider.GetComponent<PlayerStats>();
            if (ph != null) ph.TakeDamage(damage);

            StickTo(collision.transform);
            return;
        }

        if (!collision.collider.isTrigger)
        {
            ContactPoint contact = collision.contacts[0];

            transform.position = contact.point;
            transform.rotation = Quaternion.LookRotation(rb.linearVelocity) * meshOffset;

            StickTo(null);
        }
    }
}