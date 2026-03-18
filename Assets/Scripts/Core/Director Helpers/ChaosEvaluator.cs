using UnityEngine;

// Owned by DirectorAI. Reads game state every frame and returns
// a chaos delta to apply. Keeping this separate means the Director
// stays readable and chaos math can be tuned without touching spawn logic.
public class ChaosEvaluator
{
    // ── Tuning ────────────────────────────────────────────────────────────────
    // Increase rates (units per second)
    public float closeEnemyRate      = 3f;   // per enemy within close radius
    public float surroundedRate      = 5f;   // when 3+ enemies within surround radius
    public float speedsterAliveRate  = 4f;   // per live speedster
    public float archerHitRate       = 8f;   // per archer hit event (pulsed, not per frame)
    public float trapCooldownRate    = 2f;   // per trap currently on cooldown
    public float playerHitRate       = 10f;  // per hit event (pulsed)

    // Decrease rates (units per second)
    public float openSpaceRate       = 2f;   // no enemies within close radius
    public float microBreathRate     = 6f;   // while micro-breath state is active
    public float noEnemiesCloseRate  = 3f;   // no enemies in close range at all

    // Thresholds
    public float closeEnemyRadius    = 8f;
    public float surroundRadius      = 12f;
    public int   surroundCount       = 3;

    // ── Pulsed event accumulators (set externally, consumed each tick) ────────
    public float pendingArcherHits   = 0f;
    public float pendingPlayerHits   = 0f;
    public float pendingTrapKills    = 0f;   // decreases chaos
    public float pendingKnockbacks   = 0f;   // decreases chaos

    public float Evaluate(
        Transform         player,
        EnemyRoster       roster,
        TrapRegistry      traps,
        bool              microBreathActive,
        float             deltaTime)
    {
        float delta = 0f;

        // ── Increases ─────────────────────────────────────────────────────────
        int closeCount = 0;
        foreach (var enemy in roster.LiveEnemies)
        {
            if (enemy == null) continue;
            float dist = Vector3.Distance(player.position, enemy.transform.position);
            if (dist <= closeEnemyRadius) closeCount++;
        }

        if (closeCount > 0)
            delta += closeEnemyRate * closeCount * deltaTime;

        if (closeCount >= surroundCount)
            delta += surroundedRate * deltaTime;

        delta += speedsterAliveRate * roster.LiveSpeedsterCount * deltaTime;

        // Consume pulsed events
        delta += archerHitRate  * pendingArcherHits;
        delta += playerHitRate  * pendingPlayerHits;
        pendingArcherHits = 0f;
        pendingPlayerHits = 0f;

        delta += trapCooldownRate * traps.CooldownCount * deltaTime;

        // ── Decreases ─────────────────────────────────────────────────────────
        if (closeCount == 0)
            delta -= noEnemiesCloseRate * deltaTime;

        if (microBreathActive)
            delta -= microBreathRate * deltaTime;

        // Consume pulsed decreases
        delta -= 5f * pendingTrapKills;
        delta -= 3f * pendingKnockbacks;
        pendingTrapKills  = 0f;
        pendingKnockbacks = 0f;

        return delta;
    }
}