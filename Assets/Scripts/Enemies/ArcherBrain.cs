using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class ArcherBrain : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public NavMeshAgent agent;
    public Animator anim;
    public GameObject arrowPrefab;
    public Transform firePoint;

    [Header("Combat Settings")]
    public float attackRange = 10f;
    public float retreatRange = 5f;
    public float strafeDistance = 3f;
    public float shootCooldown = 2f;
    public float drawSpeed = 1f; // how fast the AI draws bow
    public float postFireDelay = 0.5f; // delay after firing

    private float shootTimer;
    private float drawAmount = 0f;
    private float moveDelayTimer = 0f;
    private bool isFiring = false;


    void Start()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }
    }
    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        // Face the player
        Vector3 lookDir = (player.position - transform.position).normalized;
        lookDir.y = 0f;
        if (lookDir.sqrMagnitude > 0f)
            transform.forward = lookDir;

        // Update movement delay
        if (moveDelayTimer > 0f)
        {
            moveDelayTimer -= Time.deltaTime;
            agent.isStopped = true;
        }
        else if (!isFiring)
        {
            HandleMovement(distance);
        }

        // Handle cooldown
        shootTimer -= Time.deltaTime;

        // Draw / aim
        bool inRange = distance <= attackRange;

        if (inRange && !isFiring)
        {
            drawAmount += Time.deltaTime * drawSpeed;
            drawAmount = Mathf.Clamp01(drawAmount);
            anim.SetFloat("DrawAmount", drawAmount);

            if (drawAmount >= 1f && shootTimer <= 0f)
            {
                StartCoroutine(FireArrowRoutine());
            }
        }
        else if (!isFiring)
        {
            // Relax bow
            drawAmount -= Time.deltaTime * drawSpeed;
            drawAmount = Mathf.Clamp01(drawAmount);
            anim.SetFloat("DrawAmount", drawAmount);
        }
    }

    void HandleMovement(float distance)
    {
        agent.isStopped = false;

        if (distance > attackRange)
        {
            // Move toward player
            agent.SetDestination(player.position);
        }
        else if (distance < retreatRange)
        {
            // Back up
            Vector3 dirAway = (transform.position - player.position).normalized;
            agent.SetDestination(transform.position + dirAway * 5f);
        }
        else
        {
            // Strafe
            Vector3 strafeDir = Vector3.Cross(Vector3.up, player.position - transform.position).normalized;
            Vector3 targetPos = transform.position + strafeDir * strafeDistance;
            agent.SetDestination(targetPos);
        }
    }

    void Shoot()
    {
        if (arrowPrefab != null && firePoint != null)
        {
            GameObject arrowObj = Instantiate(arrowPrefab, firePoint.position, firePoint.rotation);
            Arrow arrow = arrowObj.GetComponent<Arrow>();
            if (arrow != null)
            {
                Vector3 targetPos = player.position + Vector3.up * 1f;
                Vector3 aimDir = (targetPos - firePoint.position).normalized;

                // Add slight upward loft scaled by distance
                float distance = Vector3.Distance(firePoint.position, targetPos);
                float loft = Mathf.Clamp(distance / 30f, 0f, 0.3f); // max 0.3 loft
                aimDir = (aimDir + Vector3.up * loft).normalized;

                arrow.Launch(aimDir);
            }
        }
    }

    private IEnumerator FireArrowRoutine()
    {
        isFiring = true;
        agent.isStopped = true;
        anim.SetTrigger("Fire");

        // Small delay to start animation
        yield return new WaitForSeconds(0.1f);

        // Spawn arrow
        Shoot();
        shootTimer = shootCooldown;

        // Pause movement after firing
        moveDelayTimer = postFireDelay;

        // Wait for animation to finish
        yield return new WaitForSeconds(0.2f);

        // Reset draw
        drawAmount = 0f;
        anim.SetFloat("DrawAmount", drawAmount);

        isFiring = false;
    }
}