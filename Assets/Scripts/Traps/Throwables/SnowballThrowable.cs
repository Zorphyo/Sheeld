using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace Traps.Throwables
{
    public class SnowballThrowable : Throwable
    {
        public float SLOW_TIMER;
        public override void EnemyHit(EnemyLocomotion enemy)
        {
            base.EnemyHit(enemy);

            StartCoroutine(SlowEnemy(enemy.gameObject));
        }

        public IEnumerator SlowEnemy(GameObject enemy)
        {
            NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();

            agent.speed = agent.speed * 0.5f;

            yield return new WaitForSeconds(SLOW_TIMER);

            agent.speed = agent.speed * 2;
        }
    }
}
