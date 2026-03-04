using UnityEngine;
using System.Collections.Generic;

public class WaveManager : MonoBehaviour
{
    [Header("Wave Settings")]
    public Transform player;            // Player Transform
    public GameObject enemyPrefab = null;      // Enemy prefab with EnemyCombat + EnemyBrain
    public int enemiesPerWave = 5;      // How many enemies per wave
    public float spawnRadius = 10f;     // Radius around player to spawn enemies
    public float timeBetweenWaves = 5f; // Delay before next wave
    private int waveNumber = 0;
    private List<GameObject> aliveEnemies = new List<GameObject>();

    void Start()
    {
        // Start spawning waves
        StartCoroutine(SpawnWaves());
    }

    void Awake()
    {
        // Find the player automatically
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;

        // Force load the prefab from Resources, ignoring inspector
        GameObject loadedPrefab = Resources.Load<GameObject>("HumanCharacterDummy_M");
        if (loadedPrefab != null)
        {
            enemyPrefab = loadedPrefab;
            Debug.Log("Successfully loaded enemy prefab from Resources.");
        }
        else
        {
            Debug.LogError("Enemy prefab 'HumanCharacterDummy_M' not found in Resources!");
        }
    }

    System.Collections.IEnumerator SpawnWaves()
    {
        while (true)
        {
            waveNumber++;
            Debug.Log($"Starting wave {waveNumber}");

            for (int i = 0; i < enemiesPerWave; i++)
            {
                // Random position around player
                Vector3 spawnPos = player.position + Random.insideUnitSphere * spawnRadius;
                spawnPos.y = player.position.y; // Keep same ground level

                GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

                // Assign player to enemy scripts
                EnemyCombat ec = enemy.GetComponent<EnemyCombat>();
                if (ec != null) ec.player = player;

                EnemyBrain eb = enemy.GetComponent<EnemyBrain>();
                if (eb != null) eb.player = player;

                aliveEnemies.Add(enemy);

                // Small delay between spawns so enemies don’t overlap
                yield return new WaitForSeconds(0.5f);
            }

            // Wait for all enemies to be destroyed
            while (aliveEnemies.Exists(e => e != null))
            {
                yield return null;
            }

            Debug.Log($"Wave {waveNumber} cleared! Next wave in {timeBetweenWaves} seconds.");
            yield return new WaitForSeconds(timeBetweenWaves);
        }
    }
}
