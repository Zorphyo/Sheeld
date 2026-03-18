using UnityEngine;
using System.Collections.Generic;

// Singleton — lives in the scene for the lifetime of a match.
// Director AI will call RegisterMedic / UnregisterMedic when it spawns medics.
public class MedicManager : MonoBehaviour
{
    public static MedicManager Instance { get; private set; }

    private readonly List<MedicAI>      liveMedics   = new();
    private readonly List<EnemyHealth>  enemies      = new();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // ── Medic registration (called by MedicAI) ────────────────────────────────
    public void RegisterMedic(MedicAI medic)
    {
        if (!liveMedics.Contains(medic))
            liveMedics.Add(medic);

        BroadcastMedicPresence(true);
    }

    public void UnregisterMedic(MedicAI medic)
    {
        liveMedics.Remove(medic);

        if (liveMedics.Count == 0)
            BroadcastMedicPresence(false);
    }

    // ── Enemy registration (called by WaveManager) ────────────────────────────
    public void RegisterEnemy(EnemyHealth enemy)
    {
        if (!enemies.Contains(enemy))
            enemies.Add(enemy);

        // Tell this enemy immediately whether a medic is already alive
        enemy.medicPresent = liveMedics.Count > 0;
    }

    public void UnregisterEnemy(EnemyHealth enemy)
    {
        enemies.Remove(enemy);
    }

    // ── Queries used by MedicAI and EnemyHealth ───────────────────────────────
    public bool IsMedicAlive => liveMedics.Count > 0;

    // Returns the closest medic to a world position, null if none alive
    public MedicAI GetClosestMedic(Vector3 position)
    {
        MedicAI closest  = null;
        float   bestDist = float.MaxValue;

        foreach (MedicAI medic in liveMedics)
        {
            if (medic == null) continue;
            float dist = Vector3.Distance(position, medic.transform.position);
            if (dist < bestDist) { bestDist = dist; closest = medic; }
        }

        return closest;
    }

    public float GetClosestMedicDistance(Vector3 position)
    {
        MedicAI closest = GetClosestMedic(position);
        if (closest == null) return float.MaxValue;
        return Vector3.Distance(position, closest.transform.position);
    }

    // Returns all DBNO enemies — MedicAI uses this to find its target
    public EnemyHealth GetClosestDBNOEnemy(Vector3 fromPosition)
    {
        EnemyHealth closest  = null;
        float       bestDist = float.MaxValue;

        foreach (EnemyHealth enemy in enemies)
        {
            if (enemy == null || !enemy.isDBNO) continue;
            float dist = Vector3.Distance(fromPosition, enemy.transform.position);
            if (dist < bestDist) { bestDist = dist; closest = enemy; }
        }

        return closest;
    }

    // ── Internal ──────────────────────────────────────────────────────────────
    void BroadcastMedicPresence(bool present)
    {
        foreach (EnemyHealth enemy in enemies)
        {
            if (enemy != null)
                enemy.medicPresent = present;
        }
    }
}