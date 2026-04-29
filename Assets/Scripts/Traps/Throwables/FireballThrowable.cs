using System.Collections;
using UnityEngine;

namespace Traps.Throwables
{
    public class FireballThrowable : Throwable
    {
        public int BURN_INSTANCES;
        public int BURN_DAMAGE;

        public override void EnemyHit(EnemyLocomotion enemy)
        {
            base.EnemyHit(enemy);

            StartCoroutine(BurnDamage(enemy.gameObject));
        }

        public IEnumerator BurnDamage(GameObject enemy)
        {
            EnemyHealth health = enemy.GetComponent<EnemyHealth>();

            for (int i = 0; i < BURN_INSTANCES; i++)
            {
                yield return new WaitForSeconds(1);

                health.TakeDamage(BURN_DAMAGE);
            }
        }
    }
}
