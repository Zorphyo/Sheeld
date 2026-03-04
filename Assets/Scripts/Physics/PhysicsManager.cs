using System.Collections.Generic;
using UnityEngine;

// Part of code from ChatGPT 5.2, used solely for referencing specific physics objects
// and math

public sealed class PhysicsManager : MonoBehaviour
{
    public static PhysicsManager Instance { get; private set; }

    [Header("Timestep")]
    [SerializeField] private float fixedDeltaTime = 1f / 60f;
    [SerializeField] private float maximumDeltaTime = 0.05f;

    [Header("Solver")]
    [SerializeField] private int solverIterations = 10;
    [SerializeField] private int solverVelocityIterations = 2;

    [Header("Debug")]
    [SerializeField] private bool showMetrics;

    private readonly List<IRagdollPhysics> ragdolls = new();
    private readonly List<ITrapPhysics> traps = new();

    // Queue physics commands so they happen in FixedUpdate consistently.
    private readonly List<QueuedImpulse> impulseQueue = new(256);

    private int currentWave;
    private float difficultyScalar = 1f;

    // simple metrics
    private int registeredRagdolls;
    private int registeredTraps;
    private int queuedImpulsesLastStep;

    private struct QueuedImpulse
    {
        public Rigidbody body;
        public Vector3 impulse;
        public Vector3 point;
        public ForceMode mode;
        public bool usePoint;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        ApplyGlobalSettings();
    }

    private void OnValidate()
    {
        if (Application.isPlaying) ApplyGlobalSettings();
    }

    private void ApplyGlobalSettings()
    {
        Time.fixedDeltaTime = fixedDeltaTime;
        Time.maximumDeltaTime = maximumDeltaTime;

        Physics.defaultSolverIterations = Mathf.Max(1, solverIterations);
        Physics.defaultSolverVelocityIterations = Mathf.Max(1, solverVelocityIterations);
    }

    // ----------------- Registration -----------------

    public void RegisterRagdoll(IRagdollPhysics ragdoll)
    {
        if (ragdoll == null) return;
        if (!ragdolls.Contains(ragdoll)) ragdolls.Add(ragdoll);
        registeredRagdolls = ragdolls.Count;
    }

    public void UnregisterRagdoll(IRagdollPhysics ragdoll)
    {
        if (ragdoll == null) return;
        ragdolls.Remove(ragdoll);
        registeredRagdolls = ragdolls.Count;
    }

    public void RegisterTrap(ITrapPhysics trap)
    {
        if (trap == null) return;
        if (!traps.Contains(trap)) traps.Add(trap);
        registeredTraps = traps.Count;
    }

    public void UnregisterTrap(ITrapPhysics trap)
    {
        if (trap == null) return;
        traps.Remove(trap);
        registeredTraps = traps.Count;
    }

    // ----------------- Wave state -----------------

    public void SetWaveState(int waveIndex, float difficulty)
    {
        currentWave = waveIndex;
        difficultyScalar = Mathf.Max(0.1f, difficulty);
    }

    // ----------------- Physics commands -----------------

    // Queue an impulse/force to be applied in FixedUpdate.
    public void QueueImpulse(Rigidbody rb, Vector3 impulse, ForceMode mode = ForceMode.Impulse)
    {
        if (rb == null) return;
        impulseQueue.Add(new QueuedImpulse
        {
            body = rb,
            impulse = impulse,
            mode = mode,
            usePoint = false
        });
    }

    // Queue an impulse/force at a point (useful for knockback / explosions / blades).
    public void QueueImpulseAtPoint(
        Rigidbody rb,
        Vector3 impulse,
        Vector3 worldPoint,
        ForceMode mode = ForceMode.Impulse)
    {
        if (rb == null) return;
        impulseQueue.Add(new QueuedImpulse
        {
            body = rb,
            impulse = impulse,
            point = worldPoint,
            mode = mode,
            usePoint = true
        });
    }

    // ----------------- FixedUpdate pipeline -----------------

    private void FixedUpdate()
    {
        PreStep();
        StepTraps();
        StepRagdolls();
    }

    private void PreStep()
    {
        // Apply queued impulses consistently in physics time.
        for (int i = 0; i < impulseQueue.Count; i++)
        {
            var q = impulseQueue[i];
            if (q.body == null) continue;

            if (q.usePoint) q.body.AddForceAtPosition(q.impulse, q.point, q.mode);
            else q.body.AddForce(q.impulse, q.mode);
        }

        queuedImpulsesLastStep = impulseQueue.Count;
        impulseQueue.Clear();
    }

    // TODO: Check with Ruba on naming
    private void StepTraps()
    {
        // Remove dead references while iterating (iterate backwards).
        for (int i = traps.Count - 1; i >= 0; i--)
        {
            var t = traps[i];
            if (t == null || !t.IsValid) { traps.RemoveAt(i); continue; }

            t.PhysicsTick(Time.fixedDeltaTime, currentWave, difficultyScalar);
        }

        registeredTraps = traps.Count;
    }

    private void StepRagdolls()
    {
        for (int i = ragdolls.Count - 1; i >= 0; i--)
        {
            var r = ragdolls[i];
            if (r == null || !r.IsValid) { ragdolls.RemoveAt(i); continue; }

            r.PhysicsTick(Time.fixedDeltaTime);
        }

        registeredRagdolls = ragdolls.Count;
    }
}

// -------------------- Interfaces --------------------

public interface IRagdollPhysics
{
    bool IsValid { get; }
    void PhysicsTick(float dt);
}

public interface ITrapPhysics
{
    bool IsValid { get; }
    void PhysicsTick(float dt, int waveIndex, float difficultyScalar);
}