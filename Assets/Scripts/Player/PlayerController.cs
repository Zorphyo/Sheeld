using System.Collections;
using Core.Interfaces;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour, IKnockbackable
{
    Rigidbody rb;
    public PlayerInputActions pia;
    GameObject camera;
    Animator animator;
    PlayerAnimations pa;
    PlayerStats ps;

    [SerializeField] float RUN_SPEED;
    [SerializeField] float SPRINT_SPEED;
    [SerializeField] float BLOCKING_SPEED;
    [SerializeField] float ROTATION_SPEED;
    [SerializeField] float JUMP_FORCE;
    [SerializeField] float DODGE_FORCE;
    [SerializeField] float SHIELD_BASH_FORCE;

    float currentSpeed;

    Vector3 moveDirection;
    Vector3 rotateDirection;
    Quaternion targetRotation;

    [HideInInspector] public bool isSprinting = false;
    [HideInInspector] public bool isBlocking = false;
    [HideInInspector] public bool isGrounded;
    [HideInInspector] public bool isDodging = false;
    [HideInInspector] public bool isShieldBashing = false;

    public LayerMask groundLayer;
    public float rayCastHeightOffset;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        pia = new PlayerInputActions();
        animator = GetComponent<Animator>();
        pa = GetComponent<PlayerAnimations>();
        ps = GetComponent<PlayerStats>();

        pia.Enable();
        pia.Player.Movement.started += SprintOkay;
        pia.Player.Movement.canceled += SprintNotOkay;
        pia.Player.Jump.performed += JumpPerformed;
        pia.Player.Sprint.performed += SprintPerformed;
        pia.Player.Sprint.canceled += SprintCanceled;
        pia.Player.Block.performed += BlockPerformed;
        pia.Player.Block.canceled += BlockCanceled;
        pia.Player.Dodge.performed += DodgePerformed;
        pia.Player.ShieldBash.performed += ShieldBashPerformed;
        pia.Player.ShieldBash.Disable();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        camera = GameObject.FindWithTag("MainCamera");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        HandleFalling();

        Vector2 inputVector = pia.Player.Movement.ReadValue<Vector2>();

        HandleMovement(inputVector);
        HandleRotation(inputVector);
    }

    void HandleMovement(Vector2 inputVector)
    {
        moveDirection = camera.transform.forward * inputVector.y;
        moveDirection += camera.transform.right * inputVector.x;
        moveDirection.Normalize();
        moveDirection.y = 0;

        if (isBlocking)
        {
            currentSpeed = BLOCKING_SPEED;
        }

        else if (isSprinting)
        {
            currentSpeed = SPRINT_SPEED;
        }

        else
        {
            currentSpeed = RUN_SPEED;
        }

        rb.MovePosition(rb.position + (moveDirection * currentSpeed * Time.fixedDeltaTime));
    }

    void HandleRotation(Vector2 inputVector)
    {
        rotateDirection = camera.transform.forward * inputVector.y;
        rotateDirection += camera.transform.right * inputVector.x;
        rotateDirection.Normalize();
        rotateDirection.y = 0;

        if (inputVector == Vector2.zero)
        {
            rotateDirection = transform.forward;
        }

        targetRotation = Quaternion.LookRotation(rotateDirection);

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, ROTATION_SPEED * Time.fixedDeltaTime);
    }

    void HandleFalling()
    {
        RaycastHit hit;
        Vector3 rayCastOrigin;
        rayCastOrigin = transform.position;
        rayCastOrigin.y += rayCastHeightOffset;

        if (Physics.SphereCast(rayCastOrigin, 0.2f, -Vector3.up, out hit, 0.4f, groundLayer))
        {
            isGrounded = true;
        }

        else
        {
            isGrounded = false;
        }
    }

    public void JumpPerformed(InputAction.CallbackContext context)
    {
        if (isGrounded)
        {
            rb.AddForce(Vector3.up * JUMP_FORCE, ForceMode.Impulse);
        }
    }

    public void SprintPerformed(InputAction.CallbackContext context)
    {
        if (!ps.staminaLockout)
        {
            isSprinting = true;
            ps.staminaRegen = false;   
        }
    }

    public void SprintCanceled(InputAction.CallbackContext context)
    {
        isSprinting = false;
        ps.staminaRegen = true;
    }

    public void BlockPerformed(InputAction.CallbackContext context)
    {
        isBlocking = true;
        pia.Player.ShieldBash.Enable();
    }

    public void BlockCanceled(InputAction.CallbackContext context)
    {
        isBlocking = false;
        pia.Player.ShieldBash.Disable();
    }

    public void DodgePerformed(InputAction.CallbackContext context)
    {
        if (isGrounded)
        {
            isDodging = true;
            rb.AddForce(transform.forward * DODGE_FORCE, ForceMode.Impulse);
            ps.ChangeStamina(-ps.DODGE_STAMINA_COST);
            ps.staminaRegen = false;
            pia.Player.Dodge.Disable();
        }

        StartCoroutine(DodgeTimer());
    }

    IEnumerator DodgeTimer()
    {           
        yield return new WaitForSeconds((1.433f - 0.35825f) + 0.1f);

        isDodging = false;

        if (!ps.staminaLockout)
        {
            pia.Player.Dodge.Enable();

            if (!isSprinting)
            {
                ps.staminaRegen = true;
            }
        }
    }

    public void ShieldBashPerformed(InputAction.CallbackContext context)
    {
        if (isBlocking)
        {
            isShieldBashing = true;
            rb.AddForce(transform.forward * SHIELD_BASH_FORCE, ForceMode.Impulse);
            ps.ChangeStamina(-ps.SHIELD_BASH_STAMINA_COST);
            ps.staminaRegen = false;
            pia.Player.ShieldBash.Disable();
        }

        StartCoroutine(ShieldBashTimer());
    }

    IEnumerator ShieldBashTimer()
    {           
        yield return new WaitForSeconds((1.433f - 0.35825f) + 0.1f);

        isShieldBashing = false;

        if (!ps.staminaLockout)
        {
            pia.Player.ShieldBash.Enable();

            if (!isSprinting)
            {
                ps.staminaRegen = true;
            }
        }
    }

    public void SprintOkay(InputAction.CallbackContext context)
    {
        pia.Player.Sprint.Enable();
    }

    public void SprintNotOkay(InputAction.CallbackContext context)
    {
        pia.Player.Sprint.Disable();
        isSprinting = false;
    }

    public void NoMoreStamina()
    {
        pia.Player.Sprint.Disable();
        pia.Player.Block.Disable();
        pia.Player.Dodge.Disable();
    }

    public void RegainedStamina()
    {
        pia.Player.Sprint.Enable();
        pia.Player.Block.Enable();
        pia.Player.Dodge.Enable();
    }

    public void ApplyKnockback(Vector3 direction, float force)
    {
        direction.y = 0f;
        direction.Normalize();
        rb.AddForce(direction * force, ForceMode.Impulse);
    }
}