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

    private static readonly Quaternion meshOffset = Quaternion.Euler(90f, 0f, 0f);

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
    }

    // Call this after instantiating
    public void Launch(Vector3 direction)
    {
        Debug.Log("Launch direction: " + direction);
        direction.Normalize();

        rb.linearVelocity = direction * speed;
        transform.rotation = Quaternion.LookRotation(direction) * meshOffset;

        Destroy(gameObject, lifetime);
    }

    void FixedUpdate()
    {
        if (hasHit) return;

        if (rb.linearVelocity.sqrMagnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(rb.linearVelocity) * meshOffset;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 10f);
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

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;

        if (other.CompareTag("Player"))
        {
            PlayerStats ph = other.GetComponent<PlayerStats>();
            if (ph != null) ph.TakeDamage(damage);
        }

        // Stop physics completely
        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;
        rb.detectCollisions = false;

        // Store target + offset
        stuckTarget = other.transform;
        offset = transform.position - stuckTarget.position;

        hasHit = true;

        Destroy(gameObject, 5f);
    }
}