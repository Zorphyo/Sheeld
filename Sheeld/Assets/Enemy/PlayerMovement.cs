using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    private Vector2 moveInput;
    private CharacterController controller;

    [Header("Attack")]
    public float attackRange = 2f;
    public int attackDamage = 25;
    public float attackCooldown = 0.5f;
    private float lastAttackTime;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // Convert 2D input into 3D movement on XZ plane
        Vector3 movement = new Vector3(moveInput.x, 0f, moveInput.y);
        controller.Move(movement * moveSpeed * Time.deltaTime);
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnAttack()
    {
        if (Time.time < lastAttackTime + attackCooldown)
            return;

        lastAttackTime = Time.time;

        // Debug log
        Debug.Log("Player attacked at time: " + Time.time);
        PerformAttack();
    }

    void PerformAttack()
    {
        Vector3 inputDirection = new Vector3(moveInput.x, 0f, moveInput.y);

        Vector3 attackPoint;

        // If the player is pressing a movement key, attack in that direction
        if (inputDirection.sqrMagnitude > 0.01f)
        {
            inputDirection.Normalize();
            attackPoint = transform.position + inputDirection * attackRange;
        }
        else
        {
            // fallback: forward attack
            attackPoint = transform.position + transform.forward * attackRange;
        }

        Collider[] hits = Physics.OverlapSphere(attackPoint, attackRange);

        foreach (Collider hit in hits)
        {
            EnemyHealth enemyHealth = hit.GetComponentInParent<EnemyHealth>();
            if (enemyHealth != null)
            {
                Debug.Log("Hitting enemy: " + enemyHealth.gameObject.name);
                enemyHealth.TakeDamage(attackDamage);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Vector3 inputDirection = Application.isPlaying ? new Vector3(moveInput.x, 0f, moveInput.y) : Vector3.forward;
        Vector3 attackPoint;

        if (inputDirection.sqrMagnitude > 0.01f)
        {
            inputDirection.Normalize();
            attackPoint = transform.position + inputDirection * attackRange;
        }
        else
        {
            attackPoint = transform.position + transform.forward * attackRange;
        }

        Gizmos.DrawWireSphere(attackPoint, attackRange);
    }
}