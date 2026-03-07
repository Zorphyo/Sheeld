using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    Rigidbody rb;
    public PlayerInputActions pia;
    GameObject camera;

    [SerializeField] float MOVEMENT_SPEED;
    [SerializeField] float ROTATION_SPEED;
    [SerializeField] float JUMP_FORCE;

    Vector3 moveDirection;
    Vector3 rotateDirection;
    Quaternion targetRotation;

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
        camera = GameObject.FindWithTag("MainCamera");
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
        moveDirection = camera.transform.forward * inputVector.y;
        moveDirection += camera.transform.right * inputVector.x;
        moveDirection.Normalize();
        moveDirection.y = 0;

        rb.MovePosition(rb.position + (moveDirection * MOVEMENT_SPEED * Time.fixedDeltaTime));
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

    public void JumpPerformed(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            rb.AddForce(Vector3.up * JUMP_FORCE, ForceMode.Impulse);
        }
    }
}
