using UnityEngine;
using System.Collections;
using Core.Interfaces;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]

public class Throwable : MonoBehaviour, IInteractable, IThrowable
{
    SphereCollider collider;
    Rigidbody rb;
    BoxCollider bcollider;
    PlayerController player;
    bool held = false;
    bool thrown = false;

    public float THROW_FORCE;
    public float HIT_FORCE;
    public int DAMAGE;

    void Awake()
    {
        collider = GetComponent<SphereCollider>();
        bcollider = GetComponent<BoxCollider>();
        collider.isTrigger = true;

        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
    }

    void FixedUpdate()
    {
        if (player.isHolding && held)
        {
            rb.MovePosition(player.holdPosition.position);
            rb.MoveRotation(player.holdPosition.rotation);
        }
    }

    public void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.TryGetComponent<EnemyLocomotion>(out EnemyLocomotion enemy) && thrown)
        {
            EnemyHit(enemy);
        }
    }

    public void Interact()
    {
        if (!player.isHolding)
        {
            player.isHolding = true;
            held = true;
            rb.useGravity = false;
            rb.detectCollisions = false;
        }
    }

    public void Throw()
    {
        StartCoroutine(ApplyThrow());
    }

    public IEnumerator ApplyThrow()
    {
        if (player.isHolding && !player.isBlocking)
        {
            player.isHolding = false;
            held = false;
            rb.useGravity = true;
            rb.detectCollisions = true;
            thrown = true;

            rb.WakeUp();
            rb.AddForce((player.transform.forward + new Vector3(0, 0.3f, 0)) * THROW_FORCE, ForceMode.Impulse);

            yield return new WaitForFixedUpdate();
            yield return new WaitUntil(() => rb.linearVelocity.magnitude < 0.1f);

            thrown = false;
        }
    }

    public void EnemyHit(EnemyLocomotion enemy)
    {
        EnemyHealth health = enemy.gameObject.GetComponent<EnemyHealth>();
        health.TakeDamage(DAMAGE);

        Vector3 direction = rb.linearVelocity.normalized;
        enemy.Knockback(direction, HIT_FORCE);

        thrown = false;
    }
}