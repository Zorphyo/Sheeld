using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class SnowballThrowable : Throwable
{
    public override void EnemyHit(EnemyLocomotion enemy)
    {
        base.EnemyHit(enemy);

        StartCoroutine(SlowEnemy(enemy.gameObject));
    }

    public IEnumerator SlowEnemy(GameObject enemy)
    {
        NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();

        agent.speed = agent.speed * 0.5f;

        yield return new WaitForSeconds(10);

        agent.speed = agent.speed * 2;
    }
}
