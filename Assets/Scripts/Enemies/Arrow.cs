using UnityEngine;

public class Arrow : MonoBehaviour
{
    public float speed = 25f;       // initial forward speed
    public float lifetime = 5f;     // auto-destroy
    public int damage = 10;

    private Rigidbody rb;
    private bool hasHit = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
    }

    // Call this after instantiating
    public void Launch(Vector3 direction)
    {
        // Ensure direction is normalized
        direction.Normalize();

        // Set initial velocity
        rb.linearVelocity = direction * speed;

        // Align arrow to direction immediately
        transform.rotation = Quaternion.LookRotation(direction);

        // Destroy after lifetime
        Destroy(gameObject, lifetime);
    }

    void FixedUpdate()
    {
        if (hasHit) return;

        // Only rotate along velocity if moving
        if (rb.linearVelocity.sqrMagnitude > 0.1f)
        {
            transform.rotation = Quaternion.LookRotation(rb.linearVelocity);
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

        rb.isKinematic = true;
        transform.parent = other.transform;
        hasHit = true;

        Destroy(gameObject, 5f);
    }
}