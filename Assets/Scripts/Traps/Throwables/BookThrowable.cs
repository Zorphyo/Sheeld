using NUnit.Framework.Constraints;
using UnityEngine;

public class BookThrowable : Throwable
{
    public override void EnemyHit(EnemyLocomotion enemy)
    {
        base.EnemyHit(enemy);
    }
}
