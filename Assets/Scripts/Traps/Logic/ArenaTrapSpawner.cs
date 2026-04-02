using System.Collections.Generic;
using UnityEngine;

namespace Traps.Logic
{
    public class ArenaTrapSpawner : MonoBehaviour
    {
        public enum ArenaType
        {
            Forest,
            Ice,
            Fire
        }

        public enum PlacementMode
        {
            FlatOnFloor,
            EmbeddedInFloor
        }
        
        public enum TrapOverlapGroup
        {
            None,
            StationarySpikePit,
            HiddenMovingSpikePit,
            LogTrap,
            Throwable,
            Other
        }

        [System.Serializable]
        public class TrapDefinition
        {
            [Header("Basic Info")]
            public string trapName;
            public GameObject prefab;

            [Header("Arena Restrictions")]
            public List<ArenaType> allowedArenas = new List<ArenaType>();

            [Header("Spawn Count")]
            public int minSpawnCount = 1;
            public int maxSpawnCount = 3;

            [Header("Placement")]
            public PlacementMode placementMode = PlacementMode.FlatOnFloor;

            [Tooltip("Raise the trap after raycast hit. Use this if the pivot is in the middle instead of the bottom.")]
            public float bottomOffset = 0f;

            [Tooltip("Minimum depth for embedded traps.")]
            public float minEmbedDepth = 0.2f;

            [Tooltip("Maximum depth for embedded traps.")]
            public float maxEmbedDepth = 0.2f;

            [Tooltip("Extra Y adjustment after everything else.")]
            public float yOffset = 0f;

            [Header("Rotation")]
            public bool randomYaw = true;
            
            [Tooltip("Extra rotation offset if needed.")]
            public Vector3 baseRotationEuler = Vector3.zero;

            [Header("Spacing")]
            public float minDistanceFromPlayerSpawn = 6f;
            public float minDistanceFromEnemySpawn = 5f;

            [Header("Surface Rules")]
            [Tooltip("Maximum allowed slope angle in degrees. Keeps traps off steep surfaces.")]
            public float maxAllowedSlope = 15f;

            [Header("Overlap Rules")]
            public TrapOverlapGroup overlapGroup = TrapOverlapGroup.Other;

            [Tooltip("Groups this trap is allowed to overlap with.")]
            public List<TrapOverlapGroup> allowedOverlapGroups = new List<TrapOverlapGroup>();

            [Tooltip("Minimum spacing used when overlap is NOT allowed.")]
            public float overlapCheckRadius = 3f;
        }

        [Header("Arena Settings")]
        public ArenaType currentArena = ArenaType.Forest;

        [Tooltip("This should be your Main Floor object.")]
        public Collider arenaFloorCollider;

        [Tooltip("Set this to WalkableFloor in the Inspector.")]
        public LayerMask floorLayer;

        [Header("Trap Definitions")]
        public List<TrapDefinition> trapDefinitions = new List<TrapDefinition>();

        [Header("Scene References")]
        public Transform playerSpawnPoint;
        public List<Transform> enemySpawnPoints = new List<Transform>();
        public Transform trapParent;

        [Header("Raycast Settings")]
        public float raycastStartHeightAboveFloor = 50f;
        public float raycastDistance = 200f;

        [Header("Generation Settings")]
        public bool generateOnStart = true;
        public bool clearOldTrapsBeforeGenerating = true;
        public bool useRandomSeed = true;
        public int fixedSeed = 12345;
        public int maxPlacementAttemptsPerTrap = 50;

        private readonly List<SpawnedTrapInfo> placedTraps = new List<SpawnedTrapInfo>();

        private void Start()
        {
            if (generateOnStart)
            {
                GenerateTraps();
            }
        }

        [ContextMenu("Generate Traps")]
        public void GenerateTraps()
        {
            if (arenaFloorCollider == null)
            {
                Debug.LogWarning("ArenaTrapSpawner: arenaFloorCollider is not assigned.");
                return;
            }

            if (clearOldTrapsBeforeGenerating)
            {
                ClearSpawnedTraps();
            }

            placedTraps.Clear();

            if (useRandomSeed)
            {
                Random.InitState(System.Environment.TickCount);
            }
            else
            {
                Random.InitState(fixedSeed);
            }

            List<TrapDefinition> validTraps = GetValidTrapsForArena();

            if (validTraps.Count == 0)
            {
                Debug.LogWarning("ArenaTrapSpawner: No traps are allowed for arena " + currentArena);
                return;
            }

            foreach (TrapDefinition trap in validTraps)
            {
                if (trap.prefab == null)
                {
                    Debug.LogWarning("ArenaTrapSpawner: Trap prefab missing for " + trap.trapName);
                    continue;
                }

                int spawnCount = Random.Range(trap.minSpawnCount, trap.maxSpawnCount + 1);

                for (int i = 0; i < spawnCount; i++)
                {
                    bool spawned = TrySpawnTrap(trap);

                    if (!spawned)
                    {
                        Debug.LogWarning("ArenaTrapSpawner: Failed to place trap " + trap.trapName);
                    }
                }
            }
        }
        
        private class SpawnedTrapInfo
        {
            public Vector3 position;
            public TrapDefinition trapDefinition;

            public SpawnedTrapInfo(Vector3 position, TrapDefinition trapDefinition)
            {
                this.position = position;
                this.trapDefinition = trapDefinition;
            }
        }

        private bool TrySpawnTrap(TrapDefinition trap)
        {
            Bounds bounds = arenaFloorCollider.bounds;

            for (int attempt = 0; attempt < maxPlacementAttemptsPerTrap; attempt++)
            {
                float randomX = Random.Range(bounds.min.x, bounds.max.x);
                float randomZ = Random.Range(bounds.min.z, bounds.max.z);

                Vector3 rayOrigin = new Vector3(
                    randomX,
                    bounds.max.y + raycastStartHeightAboveFloor,
                    randomZ
                );

                if (!Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, raycastDistance, floorLayer))
                {
                    continue;
                }

                // Make sure the hit is actually the main floor object we assigned
                if (hit.collider != arenaFloorCollider)
                {
                    continue;
                }

                float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
                if (slopeAngle > trap.maxAllowedSlope)
                {
                    continue;
                }

                Vector3 spawnPosition = hit.point;

                // Raise for pivot correction
                spawnPosition.y += trap.bottomOffset;

                // Sink into floor if needed, with random depth range
                if (trap.placementMode == PlacementMode.EmbeddedInFloor)
                {
                    float randomEmbedDepth = Random.Range(trap.minEmbedDepth, trap.maxEmbedDepth);
                    spawnPosition.y -= randomEmbedDepth;
                }

                // Final adjustment
                spawnPosition.y += trap.yOffset;

                if (!IsPositionValid(spawnPosition, trap))
                {
                    continue;
                }

                //Quaternion prefabRotation = trap.prefab.transform.rotation;
                Quaternion baseRotation = Quaternion.Euler(trap.baseRotationEuler);

                Quaternion randomYawRotation = trap.randomYaw
                    ? Quaternion.Euler(0f, Random.Range(0f, 360f), 0f)
                    : Quaternion.identity;

                Quaternion finalRotation = randomYawRotation * baseRotation;

                GameObject spawnedTrap = Instantiate(trap.prefab, spawnPosition, finalRotation);

                if (trapParent != null)
                {
                    spawnedTrap.transform.SetParent(trapParent);
                }

                placedTraps.Add(new SpawnedTrapInfo(spawnPosition, trap));
                return true;
            }

            return false;
        }

        private bool IsPositionValid(Vector3 position, TrapDefinition trap)
        {
            if (playerSpawnPoint != null)
            {
                if (Vector3.Distance(position, playerSpawnPoint.position) < trap.minDistanceFromPlayerSpawn)
                {
                    return false;
                }
            }

            foreach (Transform enemySpawn in enemySpawnPoints)
            {
                if (enemySpawn == null)
                    continue;

                if (Vector3.Distance(position, enemySpawn.position) < trap.minDistanceFromEnemySpawn)
                {
                    return false;
                }
            }

            foreach (SpawnedTrapInfo placedTrap in placedTraps)
            {
                float distance = Vector3.Distance(position, placedTrap.position);

                TrapOverlapGroup newTrapGroup = trap.overlapGroup;
                TrapOverlapGroup placedTrapGroup = placedTrap.trapDefinition.overlapGroup;

                bool newTrapAllowsPlaced = trap.allowedOverlapGroups.Contains(placedTrapGroup);
                bool placedAllowsNew = placedTrap.trapDefinition.allowedOverlapGroups.Contains(newTrapGroup);

                bool overlapAllowed = newTrapAllowsPlaced && placedAllowsNew;

                if (!overlapAllowed && distance < trap.overlapCheckRadius)
                {
                    return false;
                }
            }

            return true;
        }

        private List<TrapDefinition> GetValidTrapsForArena()
        {
            List<TrapDefinition> valid = new List<TrapDefinition>();

            foreach (TrapDefinition trap in trapDefinitions)
            {
                if (trap == null || trap.prefab == null)
                    continue;

                if (trap.allowedArenas.Contains(currentArena))
                {
                    valid.Add(trap);
                }
            }

            return valid;
        }

        [ContextMenu("Clear Spawned Traps")]
        public void ClearSpawnedTraps()
        {
            if (trapParent == null)
                return;

            List<GameObject> children = new List<GameObject>();

            foreach (Transform child in trapParent)
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
    }
}