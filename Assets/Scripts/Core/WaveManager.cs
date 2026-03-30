using UnityEngine;
using UnityEngine.AI;
using System.Linq;
using System.Collections.Generic;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    [Header("Wave Settings")]
    public Transform player;
    public GameObject[] enemyPrefabs;
    public int   enemiesPerWave    = 5;
    public float spawnRadius       = 10f;
    public float timeBetweenWaves  = 5f;

    private int                  waveNumber   = 0;
    private List<GameObject>     aliveEnemies = new();

    public int AliveEnemyCount => aliveEnemies.Count(e => e != null);

    // ── Unity lifecycle ───────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        enemyPrefabs = Resources.LoadAll<GameObject>("Enemies");

        if (enemyPrefabs.Length > 0)
            Debug.Log("Loaded " + enemyPrefabs.Length + " enemy prefabs.");
        else
            Debug.LogError("No enemy prefabs found in Resources/Enemies!");
    }

    void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").transform;

        StartCoroutine(SpawnWaves());
    }

    // ── Wave loop ─────────────────────────────────────────────────────────────
    System.Collections.IEnumerator SpawnWaves()
    {
        while (true)
        {
            waveNumber++;
            Debug.Log($"Starting wave {waveNumber}");

            for (int i = 0; i < enemiesPerWave; i++)
            {
                Vector3 spawnPos   = player.position + Random.insideUnitSphere * spawnRadius;
                spawnPos.y         = player.position.y;

                int        randomIndex = Random.Range(0, enemyPrefabs.Length);
                GameObject enemy       = Instantiate(enemyPrefabs[randomIndex], spawnPos, Quaternion.identity);

                EnemyCombat ec = enemy.GetComponent<EnemyCombat>();
                if (ec != null) ec.player = player;

                EnemyBrain eb = enemy.GetComponent<EnemyBrain>();
                if (eb != null) eb.player = player;

                EnemyHealth eh = enemy.GetComponent<EnemyHealth>();
                if (eh != null && MedicManager.Instance != null)
                    MedicManager.Instance.RegisterEnemy(eh);

                aliveEnemies.Add(enemy);

                // Tell all existing enemies a new ally spawned — roles may rebalance
                NotifyEnemiesOfDeath();

                yield return new WaitForSeconds(0.5f);
            }

            // Wait for wave to clear, rebalancing roles whenever an enemy dies
            int previousCount = AliveEnemyCount;
            while (aliveEnemies.Exists(e => e != null))
            {
                int currentCount = AliveEnemyCount;
                if (currentCount < previousCount)
                {
                    NotifyEnemiesOfDeath();
                    previousCount = currentCount;
                }
                yield return null;
            }

            Debug.Log($"Wave {waveNumber} cleared! Next wave in {timeBetweenWaves} seconds.");
            yield return new WaitForSeconds(timeBetweenWaves);
        }
    }

    // ── Role rebalancing ──────────────────────────────────────────────────────
    void NotifyEnemiesOfDeath()
    {
        foreach (GameObject e in aliveEnemies)
        {
            if (e == null) continue;
            e.GetComponent<EnemyBrain>()?.ReevaluateRole();
        }
    }
}