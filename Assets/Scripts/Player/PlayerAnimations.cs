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

        //runOkay = Animator.StringToHash("Run");
        //sprintOkay = Animator.StringToHash("Sprint");
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
    }
}