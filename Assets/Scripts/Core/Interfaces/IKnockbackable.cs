using UnityEngine;

namespace Core.Interfaces
{
    /*
        IKnockbackable
        --------------
        Any object that can receive knockback should implement this interface.

        Parameters:
        - direction: direction the object should be pushed
        - force: how strong the knockback is
    */
    public interface IKnockbackable
    {
        void ApplyKnockback(Vector3 direction, float force);
    }
}