using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TrapSpawnerNavMesh : MonoBehaviour
{
    [Header("Trap Prefabs")]
    [SerializeField] private GameObject[] trapPrefabs;

    [Header("Spawn Rules")]
    [SerializeField] private int trapsToSpawn = 5;
    [SerializeField] private float arenaRadius = 18f;               // radius of arena
    [SerializeField] private float minDistanceBetweenTraps = 3f;    // spacing so they don't cluster
    [SerializeField] private float safeRadiusFromPlayer = 6f;       // don't spawn near player start

    [Header("References")]
    [SerializeField] private Transform arenaCenter;                 // empty object at center of arena
    [SerializeField] private Transform player;                      // player transform (or spawn point)
    [SerializeField] private Transform spawnedTrapsParent;          // "SpawnedTraps" container

    [Header("NavMesh Sampling")]
    [SerializeField] private float navMeshSearchRadius = 2f;        // how far to search from random point
    [SerializeField] private int maxAttempts = 200;                 // tries to find valid positions

    private readonly List<Vector3> _chosenPositions = new();

    private void Start()
    {
        SpawnTraps();
    }

    public void SpawnTraps()
    {
        if (trapPrefabs == null || trapPrefabs.Length == 0)
        {
            Debug.LogWarning("TrapSpawnerNavMesh: No trap prefabs assigned.");
            return;
        }

        if (arenaCenter == null)
        {
            Debug.LogWarning("TrapSpawnerNavMesh: arenaCenter not assigned.");
            return;
        }

        if (spawnedTrapsParent == null)
        {
            Debug.LogWarning("TrapSpawnerNavMesh: spawnedTrapsParent not assigned.");
            return;
        }

        // Clear old traps if re-running 
        for (int i = spawnedTrapsParent.childCount - 1; i >= 0; i--)
            Destroy(spawnedTrapsParent.GetChild(i).gameObject);

        _chosenPositions.Clear();

        int spawned = 0;
        int attempts = 0;

        while (spawned < trapsToSpawn && attempts < maxAttempts)
        {
            attempts++;

            // 1) Random point inside circle (XZ plane)
            Vector2 rand2 = Random.insideUnitCircle * arenaRadius;
            Vector3 randomWorld = arenaCenter.position + new Vector3(rand2.x, 0f, rand2.y);

            // 2) Snap to nearest NavMesh point
            if (!NavMesh.SamplePosition(randomWorld, out NavMeshHit hit, navMeshSearchRadius, NavMesh.AllAreas))
                continue;

            Vector3 candidate = hit.position;

            // 3) Safe radius away from player
            if (player != null)
            {
                float dPlayer = Vector3.Distance(candidate, player.position);
                if (dPlayer < safeRadiusFromPlayer)
                    continue;
            }

            // 4) Spacing check from other traps
            bool tooClose = false;
            for (int i = 0; i < _chosenPositions.Count; i++)
            {
                if (Vector3.Distance(candidate, _chosenPositions[i]) < minDistanceBetweenTraps)
                {
                    tooClose = true;
                    break;
                }
            }
            if (tooClose) continue;

            // 5) Spawn a random trap prefab
            GameObject prefab = trapPrefabs[Random.Range(0, trapPrefabs.Length)];

            // face toward center (optional i might remove thjs)
            Quaternion rot = Quaternion.LookRotation((arenaCenter.position - candidate).normalized, Vector3.up);

            Instantiate(prefab, candidate, rot, spawnedTrapsParent);

            _chosenPositions.Add(candidate);
            spawned++;
        }

        Debug.Log($"TrapSpawnerNavMesh: Spawned {spawned}/{trapsToSpawn} traps in {attempts} attempts.");
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (arenaCenter == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(arenaCenter.position, arenaRadius);
    }
#endif
}