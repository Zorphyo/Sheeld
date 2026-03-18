using UnityEngine;

public abstract class TrapBase : MonoBehaviour
{
    public abstract bool IsOnCooldown { get; }

    public void NotifyKill()
    {
        if (DirectorAI.Instance != null)
            DirectorAI.Instance.OnTrapKill();
    }

    public void NotifyKnockback()
    {
        if (DirectorAI.Instance != null)
            DirectorAI.Instance.OnKnockback();
    }
}