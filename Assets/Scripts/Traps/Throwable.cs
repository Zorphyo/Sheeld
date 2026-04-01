using UnityEngine;
using Core.Interfaces;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SphereCollider))]

public class Throwable : MonoBehaviour, IInteractable, IThrowable
{
    SphereCollider collider;
    Rigidbody rb;
    BoxCollider bcollider;
    PlayerController player;
    bool held = false;

    public float THROW_FORCE;

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
        if (player.isHolding && !player.isBlocking)
        {
            player.isHolding = false;
            held = false;
            rb.useGravity = true;
            rb.detectCollisions = true;
            rb.WakeUp();

            rb.AddForce((player.transform.forward + new Vector3(0, 0.4f, 0)) * THROW_FORCE, ForceMode.Impulse);
        }
    }
}