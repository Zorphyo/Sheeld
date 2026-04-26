using System.Collections;
using Core.Interfaces;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(EnemyMovement))]
[RequireComponent(typeof(SphereCollider))]
public class EnemyLocomotion : MonoBehaviour, IKnockbackable
{
    [Header("Range Thresholds")]
    public float farRange   = 35f;
    public float closeRange = 10f;

    [Header("Rotation Speeds (deg/sec)")]
    public float sprintTurnSpeed = 180f;   // fast, facing movement dir
    public float midTurnSpeed    = 270f;   // lerping toward player
    public float closeTurnSpeed  = 360f;   // always snapped to player

    [Header("Strafe Settings")]
    public float strafeIntervalMin = 2f;
    public float strafeIntervalMax = 5f;
    public float strafeDuration    = 1.2f;
    public float strafeStrength    = 1f;   // how far left/right on blend tree

    [Header("Blend Smoothing")]
    public float velocitySmoothing = 0.12f;

    // ── Private state ─────────────────────────────────────────────────────────
    private NavMeshAgent agent;
    private Animator     animator;
    private Transform    player;
    private Rigidbody    rb;

    private Vector2 smoothVelocity;
    private Vector2 velocityRef;        // SmoothDamp ref

    private float strafeTimer;
    private float strafeDurationTimer;
    private float strafeDirection;      // −1 left, +1 right
    private bool  isStrafing;

    private enum LocomotionMode { Far, Mid, Close }
    private LocomotionMode currentMode;

    // Animator param IDs (faster than string lookup every frame)
    private static readonly int VelX    = Animator.StringToHash("VelocityX");
    private static readonly int VelZ    = Animator.StringToHash("VelocityZ");
    private static readonly int SpeedID = Animator.StringToHash("Speed");

    void Awake()
    {
        agent    = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        rb       = GetComponent<Rigidbody>();
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        ScheduleNextStrafe();
    }

    void Update()
    {
        if (player == null || !agent.isOnNavMesh) return;

        float distance = Vector3.Distance(transform.position, player.position);
        UpdateMode(distance);
        UpdateRotation(distance);
        UpdateStrafe(distance);
        UpdateAnimatorVelocity();
    }

    // ── Mode switching ────────────────────────────────────────────────────────
    void UpdateMode(float distance)
    {
        if      (distance > farRange)   currentMode = LocomotionMode.Far;
        else if (distance > closeRange) currentMode = LocomotionMode.Mid;
        else                            currentMode = LocomotionMode.Close;
    }

    // ── Rotation ──────────────────────────────────────────────────────────────
    void UpdateRotation(float distance)
    {
        Vector3 targetDir;
        float   turnSpeed;

        switch (currentMode)
        {
            case LocomotionMode.Far:
                // Face the direction we're actually moving
                if (agent.velocity.sqrMagnitude > 0.1f)
                    targetDir = agent.velocity.normalized;
                else
                    targetDir = (player.position - transform.position).normalized;
                turnSpeed = sprintTurnSpeed;
                break;

            case LocomotionMode.Mid:
                // Lerp from movement dir toward player dir as we close in
                float t = 1f - Mathf.InverseLerp(closeRange, farRange, distance);
                Vector3 moveDir   = agent.velocity.sqrMagnitude > 0.1f
                    ? agent.velocity.normalized
                    : (player.position - transform.position).normalized;
                Vector3 playerDir = (player.position - transform.position).normalized;
                targetDir = Vector3.Slerp(moveDir, playerDir, t);
                turnSpeed = midTurnSpeed;
                break;

            default: // Close
                targetDir = (player.position - transform.position).normalized;
                turnSpeed = closeTurnSpeed;
                break;
        }

        targetDir.y = 0f;
        if (targetDir.sqrMagnitude < 0.001f) return;

        Quaternion targetRot = Quaternion.LookRotation(targetDir);
        transform.rotation   = Quaternion.RotateTowards(
            transform.rotation, targetRot, turnSpeed * Time.deltaTime);
    }

    // ── Strafe logic ──────────────────────────────────────────────────────────
    void UpdateStrafe(float distance)
    {
        if (currentMode != LocomotionMode.Close)
        {
            isStrafing = false;
            return;
        }

        if (isStrafing)
        {
            strafeDurationTimer -= Time.deltaTime;
            if (strafeDurationTimer <= 0f)
            {
                isStrafing = false;
                ScheduleNextStrafe();
            }
        }
        else
        {
            strafeTimer -= Time.deltaTime;
            if (strafeTimer <= 0f)
            {
                isStrafing          = true;
                strafeDurationTimer = strafeDuration;
                strafeDirection     = Random.value > 0.5f ? 1f : -1f;
            }
        }
    }

    void ScheduleNextStrafe()
    {
        strafeTimer = Random.Range(strafeIntervalMin, strafeIntervalMax);
    }

    // ── Animator velocity ─────────────────────────────────────────────────────
    void UpdateAnimatorVelocity()
    {
        // Project world-space agent velocity onto our local axes
        Vector3 worldVel  = agent.velocity;
        Vector3 localVel  = transform.InverseTransformDirection(worldVel);

        // Normalize by agent's max speed so values stay in -1..1
        float maxSpeed    = Mathf.Max(agent.speed, 0.01f);
        Vector2 targetVel = new Vector2(localVel.x / maxSpeed, localVel.z / maxSpeed);

        // Inject strafe offset at close range
        if (isStrafing && currentMode == LocomotionMode.Close)
            targetVel.x = Mathf.Clamp(targetVel.x + strafeDirection * strafeStrength, -1f, 1f);

        // Smooth so blend tree doesn't snap
        smoothVelocity = Vector2.SmoothDamp(
            smoothVelocity, targetVel, ref velocityRef, velocitySmoothing);

        animator.SetFloat(VelX,    smoothVelocity.x);
        animator.SetFloat(VelZ,    smoothVelocity.y);
        animator.SetFloat(SpeedID, smoothVelocity.magnitude);
    }

    public void Knockback(Vector3 direction, float force)
    {
        StartCoroutine(ApplyKnockback(direction, force));
    }

    public IEnumerator ApplyKnockback(Vector3 direction, float force)
    {
        yield return null;

        animator.applyRootMotion = false;
        agent.enabled = false;
        rb.useGravity = true;
        rb.isKinematic = false;

        rb.AddForce(direction + new Vector3(0, 0.2f, 2) * force, ForceMode.Impulse);

        yield return new WaitForFixedUpdate();
        yield return new WaitUntil(() => rb.linearVelocity.magnitude < 2f);

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.useGravity = false;
        rb.isKinematic = true;

        agent.Warp(transform.position);
        agent.enabled = true;
        animator.applyRootMotion = true;

        yield return null;
    }
}