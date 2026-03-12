using UnityEngine;

// Drives the enemy's Animator based on movement speed and external events.
public class EnemyAnimator : MonoBehaviour
{
    private Animator animator;
    private EnemyMovement movement;

    void Awake()
    {
        animator = GetComponent<Animator>();
        movement = GetComponent<EnemyMovement>();
    }

    void Update()
    {
        // Sync movement speed with blend tree
        if (animator != null && movement != null)
        {
            animator.SetFloat("Speed", movement.GetSpeed());
        }
    }

    // Attack trigger
    public void Attack()
    {
        animator.SetTrigger("Attack");
    }

    // Hit reaction
    public void TakeDamage()
    {
       // animator.SetTrigger("Hit");
    }

    // Enter DBNO state
    public void EnterDBNO()
    {
        animator.SetBool("isDBNO", true);
    }

    // Revive from DBNO
    public void Revive()
    {
        animator.SetBool("isDBNO", false);
    }

    // Instant death (no medic present)
    public void Die()
    {
        animator.SetBool("isDead", true);
    }
}