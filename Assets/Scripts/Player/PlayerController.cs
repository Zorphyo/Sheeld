using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    Rigidbody rb;
    public PlayerInputActions pia;

    [SerializeField] float MOVEMENT_SPEED;
    [SerializeField] float ROTATION_SPEED;
    [SerializeField] float JUMP_FORCE;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        pia = new PlayerInputActions();

        pia.Enable();
        pia.Player.Jump.performed += JumpPerformed;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector2 inputVector = pia.Player.Movement.ReadValue<Vector2>();

        HandleMovement(inputVector);
        HandleRotation(inputVector);
    }

    void HandleMovement(Vector2 inputVector)
    {
        rb.MovePosition(rb.position + (new Vector3(inputVector.x, 0, inputVector.y) * MOVEMENT_SPEED * Time.fixedDeltaTime));
    }

    void HandleRotation(Vector2 inputVector)
    {
        Vector3 targetDirection = new Vector3(inputVector.x, 0, inputVector.y);

        if (targetDirection == Vector3.zero)
        {
            targetDirection = transform.forward;
        }

        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        Quaternion playerRotation = Quaternion.Slerp(transform.rotation, targetRotation, ROTATION_SPEED * Time.fixedDeltaTime);

        transform.rotation = playerRotation;
    }

    public void JumpPerformed(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            rb.AddForce(Vector3.up * JUMP_FORCE, ForceMode.Impulse);
        }
    }
}
