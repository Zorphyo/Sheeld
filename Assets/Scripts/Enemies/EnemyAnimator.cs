using UnityEngine;

public class EnemyAnimator : MonoBehaviour
{
    // ── Components ────────────────────────────────────────────────────────────
    private Animator        animator;
    private EnemyMovement   movement;

    // ── State instances (pre-allocated, no runtime allocs) ───────────────────
    private readonly IdleState      stateIdle      = new();
    private readonly MovingState    stateMoving    = new();
    private readonly AttackingState stateAttacking = new();
    private readonly HitState       stateHit       = new();
    private readonly DBNOState      stateDBNO      = new();
    private readonly DeadState      stateDead      = new();

    private IEnemyAnimState currentState;

    // ── Animator param IDs ────────────────────────────────────────────────────
    private static readonly int ParamSpeed     = Animator.StringToHash("Speed");
    private static readonly int ParamVelocityX = Animator.StringToHash("VelocityX");
    private static readonly int ParamVelocityZ = Animator.StringToHash("VelocityZ");
    private static readonly int ParamAttack    = Animator.StringToHash("Attack");
    private static readonly int ParamHit       = Animator.StringToHash("Hit");
    private static readonly int ParamIsDBNO    = Animator.StringToHash("isDBNO");
    private static readonly int ParamIsDead    = Animator.StringToHash("isDead");

    // ── Unity lifecycle ───────────────────────────────────────────────────────
    void Awake()
    {
        animator = GetComponent<Animator>();
        movement = GetComponent<EnemyMovement>();
    }

    void Start() => TransitionTo(stateIdle);

    void Update() => currentState?.Update(this);

    // ── Transition helpers ────────────────────────────────────────────────────
    public void TransitionTo(IEnemyAnimState next)
    {
        currentState?.Exit(this);
        currentState = next;
        currentState.Enter(this);
    }

    public void TransitionToHit(IEnemyAnimState origin)
    {
        currentState?.Exit(this);
        currentState = stateHit;
        stateHit.EnterFrom(this, origin);
    }

    // ── External event API ────────────────────────────────────────────────────
    public void Attack()     => currentState?.OnAttack(this);
    public void TakeDamage() => currentState?.OnDamage(this);
    public void EnterDBNO()  => currentState?.OnDBNO(this);
    public void Revive()     => currentState?.OnRevive(this);
    public void Die()        => currentState?.OnDie(this);

    // ── Accessors for states ──────────────────────────────────────────────────
    public Animator      Animator => animator;
    public EnemyMovement Movement => movement;

    // ── Animator helpers (keep all SetX calls off magic strings) ─────────────
    public void SetSpeed    (float v)  => animator.SetFloat(ParamSpeed,     v);
    public void SetVelocityX(float v)  => animator.SetFloat(ParamVelocityX, v);
    public void SetVelocityZ(float v)  => animator.SetFloat(ParamVelocityZ, v);
    public void TriggerAttack()        => animator.SetTrigger(ParamAttack);
    public void TriggerHit()           => animator.SetTrigger(ParamHit);
    public void SetDBNO    (bool v)    => animator.SetBool(ParamIsDBNO,     v);
    public void SetDead    (bool v)    => animator.SetBool(ParamIsDead,     v);

    // =========================================================================
    // State interface
    // =========================================================================
    public interface IEnemyAnimState
    {
        void Enter (EnemyAnimator ctx);
        void Update(EnemyAnimator ctx);
        void Exit  (EnemyAnimator ctx);

        void OnAttack(EnemyAnimator ctx) {}
        void OnDamage(EnemyAnimator ctx) {}
        void OnDBNO  (EnemyAnimator ctx) {}
        void OnRevive(EnemyAnimator ctx) {}
        void OnDie   (EnemyAnimator ctx) {}
    }

    // =========================================================================
    // Idle
    // =========================================================================
    private class IdleState : IEnemyAnimState
    {
        public void Enter(EnemyAnimator ctx)
        {
            ctx.SetSpeed(0f);
            ctx.SetVelocityX(0f);
            ctx.SetVelocityZ(0f);
        }

        public void Exit(EnemyAnimator ctx) {}

        public void Update(EnemyAnimator ctx)
        {
            if (ctx.Movement.GetSpeed() > 0.05f)
                ctx.TransitionTo(ctx.stateMoving);
        }

        public void OnAttack(EnemyAnimator ctx) => ctx.TransitionTo(ctx.stateAttacking);
        public void OnDamage(EnemyAnimator ctx) => ctx.TransitionToHit(this);
        public void OnDBNO  (EnemyAnimator ctx) => ctx.TransitionTo(ctx.stateDBNO);
        public void OnDie   (EnemyAnimator ctx) => ctx.TransitionTo(ctx.stateDead);
    }

    // =========================================================================
    // Moving
    // =========================================================================
    private class MovingState : IEnemyAnimState
    {
        public void Enter(EnemyAnimator ctx) {}
        public void Exit (EnemyAnimator ctx) {}

        public void Update(EnemyAnimator ctx)
        {
            // VelocityX, VelocityZ and Speed are written every frame by
            // EnemyLocomotion — we only watch Speed here for state transitions.
            if (ctx.Movement.GetSpeed() <= 0.05f)
                ctx.TransitionTo(ctx.stateIdle);
        }

        public void OnAttack(EnemyAnimator ctx) => ctx.TransitionTo(ctx.stateAttacking);
        public void OnDamage(EnemyAnimator ctx) => ctx.TransitionToHit(this);
        public void OnDBNO  (EnemyAnimator ctx) => ctx.TransitionTo(ctx.stateDBNO);
        public void OnDie   (EnemyAnimator ctx) => ctx.TransitionTo(ctx.stateDead);
    }

    // =========================================================================
    // Attacking
    // =========================================================================
    private class AttackingState : IEnemyAnimState
    {
        public void Enter (EnemyAnimator ctx) => ctx.TriggerAttack();
        public void Exit  (EnemyAnimator ctx) {}

        public void Update(EnemyAnimator ctx)
        {
            AnimatorStateInfo info          = ctx.Animator.GetCurrentAnimatorStateInfo(0);
            bool              clipIsPlaying = info.IsTag("Attack") && info.normalizedTime < 1f;

            if (!clipIsPlaying)
            {
                ctx.TransitionTo(ctx.Movement.GetSpeed() > 0.05f
                    ? (IEnemyAnimState)ctx.stateMoving
                    : ctx.stateIdle);
            }
        }

        public void OnDamage(EnemyAnimator ctx) => ctx.TransitionToHit(this);
        public void OnDBNO  (EnemyAnimator ctx) => ctx.TransitionTo(ctx.stateDBNO);
        public void OnDie   (EnemyAnimator ctx) => ctx.TransitionTo(ctx.stateDead);
    }

    // =========================================================================
    // Hit reaction
    // =========================================================================
    private class HitState : IEnemyAnimState
    {
        private IEnemyAnimState _previousState;

        public void Enter(EnemyAnimator ctx) {}

        public void EnterFrom(EnemyAnimator ctx, IEnemyAnimState origin)
        {
            _previousState = origin;
            ctx.TriggerHit();
        }

        public void Update(EnemyAnimator ctx)
        {
            AnimatorStateInfo info          = ctx.Animator.GetCurrentAnimatorStateInfo(0);
            bool              clipIsPlaying = info.IsTag("Hit") && info.normalizedTime < 1f;

            if (!clipIsPlaying)
                ctx.TransitionTo(_previousState);
        }

        public void Exit(EnemyAnimator ctx) {}

        // Re-trigger — restarts clip, previous state unchanged
        public void OnDamage(EnemyAnimator ctx) => ctx.TriggerHit();

        public void OnDBNO(EnemyAnimator ctx) => ctx.TransitionTo(ctx.stateDBNO);
        public void OnDie (EnemyAnimator ctx) => ctx.TransitionTo(ctx.stateDead);
    }

    // =========================================================================
    // DBNO
    // =========================================================================
    private class DBNOState : IEnemyAnimState
    {
        public void Enter (EnemyAnimator ctx) => ctx.SetDBNO(true);
        public void Exit  (EnemyAnimator ctx) => ctx.SetDBNO(false);
        public void Update(EnemyAnimator ctx) {}

        public void OnRevive(EnemyAnimator ctx) => ctx.TransitionTo(ctx.stateIdle);
        public void OnDie   (EnemyAnimator ctx) => ctx.TransitionTo(ctx.stateDead);
        // OnDamage not overridden — hits blocked in DBNO
    }

    // =========================================================================
    // Dead (terminal)
    // =========================================================================
    private class DeadState : IEnemyAnimState
    {
        public void Enter (EnemyAnimator ctx) => ctx.SetDead(true);
        public void Update(EnemyAnimator ctx) {}
        public void Exit  (EnemyAnimator ctx) {}
        // Terminal — all events swallowed via interface defaults
    }
}