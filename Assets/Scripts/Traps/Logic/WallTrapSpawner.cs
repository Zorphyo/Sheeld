using System.Collections.Generic;
using UnityEngine;

namespace Traps.Logic
{
    public class ArenaWallTrapSpawner : MonoBehaviour
    {
        public enum ArenaType
        {
            Forest,
            Ice,
            Fire
        }

        [System.Serializable]
        public class WallTrapDefinition
        {
            [Header("Basic Info")]
            public string trapName;
            public GameObject prefab;

            [Header("Arena Restrictions")]
            public List<ArenaType> allowedArenas = new List<ArenaType>();

            [Header("Spawn Count")]
            public int minSpawnCount = 1;
            public int maxSpawnCount = 3;

            [Header("Wall Placement")]
            [Tooltip("How far above the arena center/floor this trap should spawn.")]
            public float heightAboveFloor = 5f;

            [Tooltip("Pushes trap slightly inward toward the arena center.")]
            public float inwardOffset = 0.2f;

            [Tooltip("Extra rotation offset for prefab alignment.")]
            public Vector3 baseRotationEuler = Vector3.zero;

            [Header("Spacing")]
            public float minDistanceFromOtherWallTraps = 8f;
        }
        
        [System.Serializable]
        public class BlockedWallArc
        {
            [Tooltip("Start angle in degrees around the arena center.")]
            public float startAngle;

            [Tooltip("End angle in degrees around the arena center.")]
            public float endAngle;
        }
        
        [Header("Blocked Wall Arcs / Doorways")]
        public List<BlockedWallArc> blockedWallArcs = new List<BlockedWallArc>();

        [Header("Arena Settings")]
        public ArenaType currentArena = ArenaType.Forest;

        [Tooltip("Center of the arena. Usually an empty object at the middle of the arena.")]
        public Transform arenaCenter;

        [Tooltip("Radius from the arena center to the inside wall where traps should spawn.")]
        public float wallRadius = 95f;

        [Header("Wall Trap Definitions")]
        public List<WallTrapDefinition> wallTrapDefinitions = new List<WallTrapDefinition>();

        [Header("Scene References")]
        public Transform wallTrapParent;

        [Header("Generation Settings")]
        public bool generateOnStart = true;
        public bool clearOldWallTrapsBeforeGenerating = true;
        public bool useRandomSeed = true;
        public int fixedSeed = 54321;
        public int maxPlacementAttemptsPerTrap = 50;

        private readonly List<Vector3> placedPositions = new List<Vector3>();

        private void Start()
        {
            if (generateOnStart)
            {
                GenerateWallTraps();
            }
        }

        [ContextMenu("Generate Wall Traps")]
        public void GenerateWallTraps()
        {
            if (arenaCenter == null)
            {
                Debug.LogWarning("ArenaWallTrapSpawner: arenaCenter is not assigned.");
                return;
            }

            if (clearOldWallTrapsBeforeGenerating)
            {
                ClearSpawnedWallTraps();
            }

            placedPositions.Clear();

            if (useRandomSeed)
                Random.InitState(System.Environment.TickCount);
            else
                Random.InitState(fixedSeed);

            List<WallTrapDefinition> validTraps = GetValidWallTrapsForArena();

            foreach (WallTrapDefinition trap in validTraps)
            {
                if (trap.prefab == null)
                {
                    Debug.LogWarning("ArenaWallTrapSpawner: Missing prefab for " + trap.trapName);
                    continue;
                }

                int spawnCount = Random.Range(trap.minSpawnCount, trap.maxSpawnCount + 1);

                for (int i = 0; i < spawnCount; i++)
                {
                    bool spawned = TrySpawnWallTrap(trap);

                    if (!spawned)
                    {
                        Debug.LogWarning("ArenaWallTrapSpawner: Failed to place wall trap " + trap.trapName);
                    }
                }
            }
        }

        private bool TrySpawnWallTrap(WallTrapDefinition trap)
        {
            for (int attempt = 0; attempt < maxPlacementAttemptsPerTrap; attempt++)
            {
                float randomAngle = Random.Range(0f, 360f);
                
                if (IsAngleBlocked(randomAngle))
                {
                    continue;
                }

                Vector3 outwardDirection = Quaternion.Euler(0f, randomAngle, 0f) * Vector3.forward;
                Vector3 inwardDirection = -outwardDirection;

                Vector3 spawnPosition = arenaCenter.position + outwardDirection * wallRadius;
                spawnPosition += inwardDirection * trap.inwardOffset;
                spawnPosition.y = arenaCenter.position.y + trap.heightAboveFloor;

                if (!IsFarEnoughFromOtherWallTraps(spawnPosition, trap.minDistanceFromOtherWallTraps))
                {
                    continue;
                }

                Quaternion faceCenterRotation = Quaternion.LookRotation(inwardDirection, Vector3.up) * Quaternion.Euler(0f, -90f, 0f);
                Quaternion offsetRotation = Quaternion.Euler(trap.baseRotationEuler);
                Quaternion finalRotation = faceCenterRotation * offsetRotation;

                GameObject spawnedTrap = Instantiate(trap.prefab, spawnPosition, finalRotation);

                if (wallTrapParent != null)
                {
                    spawnedTrap.transform.SetParent(wallTrapParent);
                }

                placedPositions.Add(spawnPosition);
                return true;
            }

            return false;
        }

        private bool IsFarEnoughFromOtherWallTraps(Vector3 position, float minDistance)
        {
            foreach (Vector3 placed in placedPositions)
            {
                if (Vector3.Distance(position, placed) < minDistance)
                    return false;
            }

            return true;
        }
        
        private bool IsAngleBlocked(float angle)
        {
            angle = NormalizeAngle(angle);

            foreach (BlockedWallArc arc in blockedWallArcs)
            {
                float start = NormalizeAngle(arc.startAngle);
                float end = NormalizeAngle(arc.endAngle);

                if (start <= end)
                {
                    if (angle >= start && angle <= end)
                        return true;
                }
                else
                {
                    if (angle >= start || angle <= end)
                        return true;
                }
            }

            return false;
        }

        private float NormalizeAngle(float angle)
        {
            angle %= 360f;

            if (angle < 0f)
                angle += 360f;

            return angle;
        }

        private List<WallTrapDefinition> GetValidWallTrapsForArena()
        {
            List<WallTrapDefinition> valid = new List<WallTrapDefinition>();

            foreach (WallTrapDefinition trap in wallTrapDefinitions)
            {
                if (trap == null || trap.prefab == null)
                    continue;

                if (trap.allowedArenas.Contains(currentArena))
                    valid.Add(trap);
            }

            return valid;
        }

        [ContextMenu("Clear Spawned Wall Traps")]
        public void ClearSpawnedWallTraps()
        {
            if (wallTrapParent == null)
                return;

            List<GameObject> children = new List<GameObject>();

            foreach (Transform child in wallTrapParent)
            {
                children.Add(child.gameObject);
            }

            foreach (GameObject child in children)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    DestroyImmediate(child);
                else
                    Destroy(child);
#else
                Destroy(child);
#endif
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            if (arenaCenter == null)
                return;

            float radius = wallRadius;

            // Draw full circle (reference)
            Gizmos.color = Color.white;
            DrawCircle(arenaCenter.position, radius);

            // Draw blocked arcs
            Gizmos.color = Color.red;

            foreach (var arc in blockedWallArcs)
            {
                DrawArc(arenaCenter.position, radius, arc.startAngle, arc.endAngle);
            }
        }
        private void DrawCircle(Vector3 center, float radius)
        {
            int segments = 100;
            Vector3 prevPoint = center + Quaternion.Euler(0, 0, 0) * Vector3.forward * radius;

            for (int i = 1; i <= segments; i++)
            {
                float angle = i * (360f / segments);
                Vector3 nextPoint = center + Quaternion.Euler(0, angle, 0) * Vector3.forward * radius;

                Gizmos.DrawLine(prevPoint, nextPoint);
                prevPoint = nextPoint;
            }
        }

        private void DrawArc(Vector3 center, float radius, float startAngle, float endAngle)
        {
            int segments = 50;

            float normalizedStart = NormalizeAngle(startAngle);
            float normalizedEnd = NormalizeAngle(endAngle);

            float angleRange = normalizedEnd - normalizedStart;
            if (angleRange < 0) angleRange += 360f;

            Vector3 prevPoint = center + Quaternion.Euler(0, normalizedStart, 0) * Vector3.forward * radius;

            for (int i = 1; i <= segments; i++)
            {
                float angle = normalizedStart + (angleRange * i / segments);
                Vector3 nextPoint = center + Quaternion.Euler(0, angle, 0) * Vector3.forward * radius;

                // draw triangle from center → prev → next
                Gizmos.DrawLine(center, prevPoint);
                Gizmos.DrawLine(prevPoint, nextPoint);
                Gizmos.DrawLine(nextPoint, center);

                prevPoint = nextPoint;
            }
        }
    }
    
}