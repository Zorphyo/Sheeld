using UnityEngine;
using System.Collections.Generic;
using System.Linq;

// Lightweight registry — DirectorAI and ChaosEvaluator query this
// instead of calling FindObjectsOfType every frame.
public class EnemyRoster
{
    private readonly List<GameObject> live = new();

    public IReadOnlyList<GameObject> LiveEnemies     => live;
    public int                       TotalCount       => live.Count(e => e != null);
    public int                       LiveSpeedsterCount =>
        live.Count(e => e != null && e.CompareTag("Speedster"));

    public void Register  (GameObject e) { if (!live.Contains(e)) live.Add(e); }
    public void Unregister(GameObject e) => live.Remove(e);
    public void Clear() => live.Clear();
    public void Purge     ()             => live.RemoveAll(e => e == null);
}
