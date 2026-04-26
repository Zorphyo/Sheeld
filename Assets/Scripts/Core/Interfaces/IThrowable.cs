using UnityEngine;

namespace Core.Interfaces
{
    public interface IThrowable
    {
        void Throw();

        void EnemyHit(EnemyLocomotion enemy);
    }
}
