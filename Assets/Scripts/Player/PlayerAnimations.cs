using NUnit.Framework.Constraints;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerAnimations : MonoBehaviour
{
    Animator animator;
    PlayerController pc;

    int runOkay;
    int sprintOkay;

    void Awake()
    {
        animator = GetComponent<Animator>();
        pc = GetComponent<PlayerController>();
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        HandleAnimation();
    }

    public void HandleAnimation()
    {
        if (pc.pia.Player.Movement.ReadValue<Vector2>() != Vector2.zero)
        {
            animator.SetBool("Run", true);
        }

        else
        {
            animator.SetBool("Run", false);
        }

        if (pc.isSprinting)
        {
            animator.SetBool("Sprint", true);
        }

        else
        {
            animator.SetBool("Sprint", false);
        }

        if (pc.isBlocking)
        {
            animator.SetBool("Block", true);
        }

        else
        {
            animator.SetBool("Block", false);
        }

        if (pc.isGrounded)
        {
            animator.SetBool("Falling", false);
        }

        else
        {
            animator.SetBool("Falling", true);
        }

        if (pc.isDodging)
        {
            animator.SetBool("Dodge", true);
        }

        else
        {
            animator.SetBool("Dodge", false);
        }
    }
}