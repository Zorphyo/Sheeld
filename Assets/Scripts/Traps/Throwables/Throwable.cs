using System.Collections;
using Core.Interfaces;
using Traps.TrapUsageData;
using UnityEngine;

namespace Traps.Throwables
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(SphereCollider))]
    public abstract class Throwable : MonoBehaviour, IInteractable
    {
        SphereCollider collider;
        Rigidbody rb;
        BoxCollider bcollider;
        PlayerController player;
        bool held = false;
        bool thrown = false;

        [Header("Trap Data")]
        [SerializeField] private TrapType trapType = TrapType.Throwable;

        public float THROW_FORCE;
        public float HIT_FORCE;
        public int DAMAGE;

        void Awake()
        {
            collider = GetComponent<SphereCollider>();
            bcollider = GetComponent<BoxCollider>();
            collider.isTrigger = true;

            rb = GetComponent<Rigidbody>();
        }

        void Start()
        {
            player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        }

        void FixedUpdate()
        {
            if (player.isHolding && held)
            {
                rb.MovePosition(player.holdPosition.position);
                rb.MoveRotation(player.holdPosition.rotation);
            }
        }

        public void OnTriggerEnter(Collider collider)
        {
            if (collider.gameObject.TryGetComponent<EnemyLocomotion>(out EnemyLocomotion enemy) && thrown)
            {
                EnemyHit(enemy);
            }
        }

        public void Interact()
        {
            if (!player.isHolding)
            {
                Record(TrapEventType.Interacted);

                player.isHolding = true;
                held = true;
                rb.useGravity = false;
                rb.detectCollisions = false;
            }
        }

        public void Throw()
        {
            StartCoroutine(ApplyThrow());
        }

        public IEnumerator ApplyThrow()
        {
            if (player.isHolding && !player.isBlocking)
            {
                Record(TrapEventType.Triggered);

                player.isHolding = false;
                held = false;
                rb.useGravity = true;
                rb.detectCollisions = true;
                thrown = true;

                rb.WakeUp();
                rb.AddForce((player.transform.forward + new Vector3(0, 0.3f, 0)) * THROW_FORCE, ForceMode.Impulse);

                yield return new WaitForFixedUpdate();
                yield return new WaitUntil(() => rb.linearVelocity.magnitude < 0.5f);

                thrown = false;
            }
        }

        public virtual void EnemyHit(EnemyLocomotion enemy)
        {
            Record(TrapEventType.HitEnemy);

            EnemyHealth health = enemy.gameObject.GetComponent<EnemyHealth>();

            if (health != null)
            {
                health.TakeDamage(DAMAGE);
                Record(TrapEventType.DamagedEnemy);
            }

            Vector3 direction = rb.linearVelocity.normalized;
            enemy.Knockback(direction, HIT_FORCE);

            thrown = false;
        }

        private void Record(TrapEventType eventType)
        {
            if (TrapStatsManager.Instance != null)
            {
                TrapStatsManager.Instance.RecordTrapEvent(trapType, eventType);
            }
        }
    }
}