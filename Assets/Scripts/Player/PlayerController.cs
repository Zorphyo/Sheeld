using System.Collections;
using Core.Interfaces;
using Traps.Throwables;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour, IKnockbackable
{
    Rigidbody rb;
    public PlayerInputActions pia;
    GameObject camera;
    PlayerStats ps;
    PlayerSounds sounds;

    [SerializeField] float RUN_SPEED;
    [SerializeField] float SPRINT_SPEED;
    [SerializeField] float BLOCKING_SPEED;
    [SerializeField] float ROTATION_SPEED;
    [SerializeField] float JUMP_FORCE;
    [SerializeField] float DODGE_FORCE;
    [SerializeField] float SHIELD_BASH_FORCE;
    [SerializeField] float KNOCKBACK_FORCE;

    float currentSpeed;

    Vector3 moveDirection;
    Vector3 rotateDirection;
    Quaternion targetRotation;

    [HideInInspector] public bool isSprinting = false;
    [HideInInspector] public bool isBlocking = false;
    [HideInInspector] public bool isGrounded;
    [HideInInspector] public bool isDodging = false;
    [HideInInspector] public bool isShieldBashing = false;
    [HideInInspector] public bool isHolding = false;

    [HideInInspector] public bool isMountedWeapon = false;
    private MountedWeapon currentMountedWeapon;

    [HideInInspector] public bool interactOkay = false;
    IInteractable currentInteractable;
    Throwable currentThrowable;
    InteractPopup text;
    public Transform holdPosition;

    public LayerMask groundLayer;
    public float rayCastHeightOffset;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        pia = new PlayerInputActions();
        ps = GetComponent<PlayerStats>();
        sounds = GetComponent<PlayerSounds>();

        pia.Enable();
        pia.Player.Movement.started += SprintOkay;
        pia.Player.Movement.canceled += SprintNotOkay;
        pia.Player.Jump.performed += JumpPerformed;
        pia.Player.Sprint.performed += SprintPerformed;
        pia.Player.Sprint.canceled += SprintCanceled;
        pia.Player.Block.started += BlockStarted;
        pia.Player.Block.canceled += BlockCanceled;
        pia.Player.Dodge.performed += DodgePerformed;
        pia.Player.ShieldBash.performed += ShieldBashPerformed;
        pia.Player.Interact.started += InteractStarted;
        pia.Player.Throw.started += ThrowStarted;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        camera = GameObject.FindWithTag("MainCamera");
        text = FindFirstObjectByType<InteractPopup>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        HandleFalling();

        if (isMountedWeapon)
        {
            return;
        }

        Vector2 inputVector = pia.Player.Movement.ReadValue<Vector2>();

        HandleMovement(inputVector);
        HandleRotation(inputVector);
    }

    private void Update()
    {
        if (isMountedWeapon && currentMountedWeapon != null)
        {
            currentMountedWeapon.HandleMountedUpdate();
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        GameObject otherGameObject = other.gameObject;

        if (otherGameObject.TryGetComponent<IInteractable>(out IInteractable interactable))
        {
            interactOkay = true;
            currentInteractable = interactable;
            text.EnablePopUp();
        }

        if (otherGameObject.TryGetComponent<Throwable>(out Throwable throwable))
        {
            if (!isHolding)
            {
                currentThrowable = throwable;
            }
        }

        if (otherGameObject.TryGetComponent<EnemyRagdollController>(out EnemyRagdollController enemy) && isShieldBashing)
        {
            Vector3 launchDirection = rb.linearVelocity.normalized;
            enemy.Knockback(launchDirection, launchDirection, 1.8f);
        }
    }

    public void OnTriggerExit(Collider other)
    {
        GameObject otherGameObject = other.gameObject;

        if (otherGameObject.TryGetComponent<IInteractable>(out IInteractable interactable))
        {
            interactOkay = false;
            currentInteractable = null;
            text.DisablePopUp();
        }
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
        if (isMountedWeapon)
        {
            DismountWeapon();
            return;
        }

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
        }
    }

    public void SprintCanceled(InputAction.CallbackContext context)
    {
        isSprinting = false;
    }

    public void BlockStarted(InputAction.CallbackContext context)
    {
        AudioSource.PlayClipAtPoint(sounds.shieldUp, transform.position);
        isBlocking = true;
        pia.Player.Throw.Disable();
    }

    public void BlockCanceled(InputAction.CallbackContext context)
    {
        AudioSource.PlayClipAtPoint(sounds.shieldDown, transform.position);
        isBlocking = false;
        pia.Player.Throw.Enable();
    }

    public void DodgePerformed(InputAction.CallbackContext context)
    {
        if (isGrounded)
        {
            isDodging = true;
            rb.AddForce(transform.forward * DODGE_FORCE, ForceMode.Impulse);
            ps.ChangeStamina(-ps.DODGE_STAMINA_COST);
            AudioSource.PlayClipAtPoint(sounds.roll, transform.position);
            pia.Player.Disable();
        }

        StartCoroutine(DodgeTimer());
    }

    IEnumerator DodgeTimer()
    {  
        yield return new WaitForSeconds(1f);

        isDodging = false;

        pia.Player.Enable();

        if (ps.staminaLockout)
        {
            NoMoreStamina();
        }
    }

    public void ShieldBashPerformed(InputAction.CallbackContext context)
    {
        if (isBlocking)
        {
            isShieldBashing = true;
            rb.AddForce(transform.forward * SHIELD_BASH_FORCE, ForceMode.Impulse);
            ps.ChangeStamina(-ps.SHIELD_BASH_STAMINA_COST);
            AudioSource.PlayClipAtPoint(sounds.shieldBash, transform.position);
            pia.Player.Disable();
        }

        StartCoroutine(ShieldBashTimer());
    }

    IEnumerator ShieldBashTimer()
    {           
        yield return new WaitForSeconds(1.1f);

        isShieldBashing = false;

        pia.Player.Enable();

        if (ps.staminaLockout)
        {
            NoMoreStamina();
        }
    }

    public void InteractStarted(InputAction.CallbackContext context)
    {
        if (isMountedWeapon)
        {
            DismountWeapon();
            return;
        }

        if (interactOkay && currentInteractable != null)
        {
            currentInteractable.Interact();
        }
    }

    public void ThrowStarted(InputAction.CallbackContext context)
    {
        if (isMountedWeapon && currentMountedWeapon != null)
        {
            currentMountedWeapon.UseWeapon();
            return;
        }

        if (isHolding)
        {
            currentThrowable.Throw();
            AudioSource.PlayClipAtPoint(sounds.thrown, transform.position);
            currentThrowable = null;
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
    
    public IEnumerator ApplyKnockback(Vector3 direction, float force)
    {
        yield return null;

        rb.AddForce(direction + new Vector3(0, 0.2f, 3) * force, ForceMode.Impulse);

        yield return new WaitForFixedUpdate();
        yield return new WaitUntil(() => rb.linearVelocity.magnitude < 2.5f);

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        yield return null;
    }

    public void Knockback(Vector3 direction, float force)
    {
        StartCoroutine(ApplyKnockback(direction, force));
    }

    public void MountWeapon(MountedWeapon weapon, Transform mountPoint)
    {
        if (weapon == null || mountPoint == null)
            return;

        isMountedWeapon = true;
        currentMountedWeapon = weapon;

        isSprinting = false;
        isBlocking = false;
        isDodging = false;
        isShieldBashing = false;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;

        transform.rotation = Quaternion.Euler(0, mountPoint.eulerAngles.y, 0);
    }

    public void DismountWeapon()
    {
        isMountedWeapon = false;
        currentMountedWeapon = null;

        rb.isKinematic = false;
    }
}
