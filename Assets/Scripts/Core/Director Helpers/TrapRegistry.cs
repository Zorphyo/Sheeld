using System.Collections.Generic;
using System.Linq;

public class TrapRegistry
{
    private readonly List<TrapBase> traps = new();

    public int CooldownCount => traps.Count(t => t != null && t.IsOnCooldown);

    public void Register  (TrapBase t) { if (!traps.Contains(t)) traps.Add(t); }
    public void Unregister(TrapBase t) => traps.Remove(t);
    public void Clear() => traps.Clear();
}