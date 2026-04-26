using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AI;

public class DirectorAI : MonoBehaviour
{
    public static DirectorAI Instance { get; private set; }

    // ── Round settings ────────────────────────────────────────────────────────
    [Header("Round Settings")]
    public int chaosSpikesRequiredBase = 3;
    public int chaosSpikesScaling = 1;
    public float timeRequiredBase = 60f;
    public float timeRequiredScaling = 15f;
    public string mainMenuScene = "MainMenu";
    private Coroutine spawnRoutine;

    [Header("Arena Scenes")]
    public string[] arenaScenes;             // assign 3 scenes in the inspector

    [Header("Arena Rounds")]
    public int[] roundsPerArena = { 3, 5, 9 }; // arena 1 = 3 rounds, arena 2 = 5, arena 3 = 9

    // ── Endless mode ──────────────────────────────────────────────────────────
    [Header("Endless Mode")]
    public bool endlessMode = false;
    public int endlessArenaIndex = 0;        // which arena to use for endless (set in inspector)

    // ── Chaos thresholds ──────────────────────────────────────────────────────
    [Header("Chaos Thresholds")]
    public float overwhelmThreshold = 40f;
    public float chaosSpikThreshold = 65f;
    public float microBreathThreshold = 85f;

    // ── Spawn settings ────────────────────────────────────────────────────────
    [Header("Spawn Cap")]
    public int maxAliveEnemies = 25;

    [Header("Spawn Intervals (seconds)")]
    public float pressureSpawnInterval = 4f;
    public float overwhelmSpawnInterval = 2.5f;
    public float chaosSpikeSpawnInterval = 1.5f;
    public float microBreathSpawnInterval = 8f;

    [Header("Micro-Breath Duration")]
    public float microBreathDuration = 6f;
    public float betweenRoundBreathDuration = 10f;
    private float lastSpikeTime = -999f;
    public float spikeCooldown = 5f;

    // ── Medic trigger ─────────────────────────────────────────────────────────
    [Header("Medic Trigger — Player Doing Well")]
    public int killStreakThreshold = 5;
    public float noDamageTimeThreshold = 20f;
    public float noAttackTimeThreshold = 10f;
    public float lowChaosTimeThreshold = 15f;

    [Header("Spawn Settings")]
    public float spawnMinDistanceFromPlayer = 15f;
    public float spawnMaxDistanceFromPlayer = 40f;

    // ── Runtime state ─────────────────────────────────────────────────────────
    [HideInInspector] public float chaosLevel = 0f;

    private DirectorState currentState = DirectorState.Pressure;
    private bool microBreathActive;
    private float microBreathTimer;
    private bool betweenRounds;

    // Round tracking
    private int currentRound = 0;
    private int chaosSpikesThisRound = 0;
    private float timeInRound = 0f;
    private bool roundEndPending = false;

    // Endless tracking
    private int endlessWave = 0;

    // Performance tracking
    private int killStreak = 0;
    private float timeSinceLastDamage = 0f;
    private float timeSinceLastAttack = 0f;
    private float timeInLowChaos = 0f;
    private bool medicSpawned = false;

    // ── Prefab pools ──────────────────────────────────────────────────────────
    private List<GameObject> basicPrefabs = new();
    private List<GameObject> heavyPrefabs = new();
    private List<GameObject> speedsterPrefabs = new();
    private List<GameObject> archerPrefabs = new();
    private List<GameObject> medicPrefabs = new();

    // ── Systems ───────────────────────────────────────────────────────────────
    private ChaosEvaluator evaluator;
    public EnemyRoster Roster { get; private set; }
    public TrapRegistry Traps { get; private set; }

    private Transform player;
    private PlayerStats playerStats;

    public enum DirectorState { Pressure, Overwhelm, ChaosSpike, MicroBreath }

    // ── Round requirements ────────────────────────────────────────────────────
    int RequiredSpikes => chaosSpikesRequiredBase + (currentRound - 1) * chaosSpikesScaling;
    float RequiredTime => timeRequiredBase + (currentRound - 1) * timeRequiredScaling;

    public int CurrentRound => currentRound;
    public int EndlessWave => endlessWave;

    // ── Unity lifecycle ───────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        evaluator = new ChaosEvaluator();
        Roster = new EnemyRoster();
        Traps = new TrapRegistry();

        LoadEnemyPrefabs();
    }

    public void StartGame()
    {
        StartCoroutine(RunGame());
    }

    void Update()
    {
        if (player == null || betweenRounds) return;

        Roster.Purge();
        TickChaos();
        TickState();
        TickRoundProgress();
        TrackPlayerPerformance();
    }

    // ── Prefab loading ────────────────────────────────────────────────────────
    void LoadEnemyPrefabs()
    {
        GameObject[] all = Resources.LoadAll<GameObject>("Enemies");

        if (all.Length == 0)
        {
            Debug.LogError("[Director] No prefabs found in Resources/Enemies!");
            return;
        }

        foreach (GameObject prefab in all)
        {
            string n = prefab.name;

            if (n.StartsWith("Enemy_Basic")) basicPrefabs.Add(prefab);
            else if (n.StartsWith("Enemy_Heavy")) heavyPrefabs.Add(prefab);
            else if (n.StartsWith("Enemy_Speedster")) speedsterPrefabs.Add(prefab);
            else if (n.StartsWith("Enemy_Archer")) archerPrefabs.Add(prefab);
            else if (n.StartsWith("Enemy_Medic")) medicPrefabs.Add(prefab);
            else Debug.LogWarning($"[Director] Unrecognised prefab: {prefab.name}");
        }

        Debug.Log($"[Director] Loaded — " +
                  $"Basic:{basicPrefabs.Count} " +
                  $"Heavy:{heavyPrefabs.Count} " +
                  $"Speedster:{speedsterPrefabs.Count} " +
                  $"Archer:{archerPrefabs.Count} " +
                  $"Medic:{medicPrefabs.Count}");
    }

    // ── Game loop ─────────────────────────────────────────────────────────────
    IEnumerator FindPlayer()
    {
        while (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
            {
                player = p.transform;
                playerStats = p.GetComponent<PlayerStats>();
            }
            yield return null;
        }
    }

    IEnumerator RunGame()
    {
        if (endlessMode)
            yield return StartCoroutine(RunEndless());
        else
            yield return StartCoroutine(RunCampaign());
    }

    IEnumerator RunCampaign()
    {
        int globalRound = 1;

        for (int arenaIndex = 0; arenaIndex < arenaScenes.Length; arenaIndex++)
        {
            yield return StartCoroutine(LoadArena(arenaIndex));

            int roundsInThisArena = roundsPerArena[arenaIndex];

            for (int arenaRound = 1; arenaRound <= roundsInThisArena; arenaRound++)
            {
                yield return StartCoroutine(RunRound(globalRound));

                bool isLastRoundOfArena = arenaRound == roundsInThisArena;
                bool isLastArena = arenaIndex == arenaScenes.Length - 1;

                if (!isLastRoundOfArena || !isLastArena)
                    yield return StartCoroutine(RunBetweenRound());

                globalRound++;
            }
        }

        // All arenas complete
        yield return StartCoroutine(RunBetweenRound());
        SceneManager.LoadScene(mainMenuScene);
    }

    IEnumerator RunEndless()
    {
        endlessWave = 0;

        int clampedArena = Mathf.Clamp(endlessArenaIndex, 0, arenaScenes.Length - 1);
        yield return StartCoroutine(LoadArena(clampedArena));

        while (true) // runs until player dies
        {
            endlessWave++;
            Debug.Log($"[Director] Endless Wave {endlessWave} started.");

            yield return StartCoroutine(RunRound(endlessWave));
            yield return StartCoroutine(RunBetweenRound());
        }
    }

    IEnumerator LoadArena(int index)
    {
        yield return SceneManager.LoadSceneAsync(arenaScenes[index]);

        MedicManager.Instance?.ClearForNewArena();

        player = null;
        yield return StartCoroutine(FindPlayer());

        // play the flyover before the round starts
        if (ArenaIntroCamera.Instance != null)
            yield return StartCoroutine(ArenaIntroCamera.Instance.PlayIntro());

        Roster.Clear();
        Traps.Clear();
        medicSpawned = false;
    }

    IEnumerator RunRound(int round)
    {
        currentRound = round;
        chaosSpikesThisRound = 0;
        timeInRound = 0f;
        roundEndPending = false;
        betweenRounds = false;

        Debug.Log($"[Director] Round {round} started — " +
                  $"need {RequiredSpikes} spikes + {RequiredTime}s");

        if (spawnRoutine != null)
            StopCoroutine(spawnRoutine);

        spawnRoutine = StartCoroutine(SpawnLoop());

        yield return new WaitUntil(() => roundEndPending);

        if (spawnRoutine != null)
            StopCoroutine(spawnRoutine);
        Debug.Log($"[Director] Round {round} complete.");
    }

    IEnumerator RunBetweenRound()
    {
        betweenRounds = true;
        microBreathActive = true;
        microBreathTimer = betweenRoundBreathDuration;

        Debug.Log("[Director] Between rounds — forced Micro-Breath.");

        yield return new WaitForSeconds(betweenRoundBreathDuration);

        microBreathActive = false;
        chaosLevel = Mathf.Lerp(chaosLevel, 0f, 0.8f);
        betweenRounds = false;
    }

    // ── Round progress ────────────────────────────────────────────────────────
    void TickRoundProgress()
    {
        if (roundEndPending) return;

        timeInRound += Time.deltaTime;

        if (timeInRound >= RequiredTime && chaosSpikesThisRound >= RequiredSpikes)
            roundEndPending = true;
    }

    // ── Chaos ─────────────────────────────────────────────────────────────────
    void TickChaos()
    {
        float delta = evaluator.Evaluate(
            player, Roster, Traps, microBreathActive, Time.deltaTime);

        chaosLevel = Mathf.Clamp(chaosLevel + delta, 0f, 100f);
    }

    // ── State machine ─────────────────────────────────────────────────────────
    void TickState()
    {
        if (microBreathActive)
        {
            microBreathTimer -= Time.deltaTime;
            if (microBreathTimer <= 0f)
            {
                microBreathActive = false;
                chaosLevel = 30f;
            }
            return;
        }

        DirectorState next;

        if (chaosLevel >= microBreathThreshold) next = DirectorState.MicroBreath;
        else if (chaosLevel >= chaosSpikThreshold) next = DirectorState.ChaosSpike;
        else if (chaosLevel >= overwhelmThreshold) next = DirectorState.Overwhelm;
        else next = DirectorState.Pressure;

        if (next != currentState)
        {
            currentState = next;
            OnStateEnter(currentState);
        }
    }

    void OnStateEnter(DirectorState state)
    {
        Debug.Log($"[Director] → {state} | Chaos: {chaosLevel:F1} | " +
                  $"Round: {currentRound} | Spikes: {chaosSpikesThisRound}/{RequiredSpikes}");

        if (state == DirectorState.ChaosSpike &&
            Time.time - lastSpikeTime > spikeCooldown)
        {
            chaosSpikesThisRound++;
            lastSpikeTime = Time.time;
        }

        if (state == DirectorState.MicroBreath)
        {
            microBreathActive = true;
            microBreathTimer = microBreathDuration;
        }
    }

    // ── Spawn loop ────────────────────────────────────────────────────────────
    IEnumerator SpawnLoop()
    {
        while (!roundEndPending)
        {
            if (!betweenRounds && Roster.TotalCount < maxAliveEnemies)
                SpawnForCurrentState();

            float interval = currentState switch
            {
                DirectorState.Overwhelm => overwhelmSpawnInterval,
                DirectorState.ChaosSpike => chaosSpikeSpawnInterval,
                DirectorState.MicroBreath => microBreathSpawnInterval,
                _ => pressureSpawnInterval
            };

            yield return new WaitForSeconds(interval);
        }
    }

    void SpawnForCurrentState()
    {
        switch (currentState)
        {
            case DirectorState.Pressure:
                Spawn(RandomFrom(basicPrefabs));
                break;

            case DirectorState.Overwhelm:
                Spawn(RandomFrom(basicPrefabs));
                if (Random.value < 0.4f) Spawn(RandomFrom(speedsterPrefabs));
                if (Random.value < 0.3f) Spawn(RandomFrom(archerPrefabs));
                if (!medicSpawned && ShouldSpawnMedic())
                {
                    Spawn(RandomFrom(medicPrefabs));
                    medicSpawned = true;
                }
                break;

            case DirectorState.ChaosSpike:
            {
                int spawnCount = 1;

                if (Random.value < 0.6f) spawnCount++;
                if (Random.value < 0.4f) spawnCount++;

                spawnCount = Mathf.Min(spawnCount, 2); // cap burst

                for (int i = 0; i < spawnCount; i++)
                {
                    float roll = Random.value;

                    if (roll < 0.5f) Spawn(RandomFrom(basicPrefabs));
                    else if (roll < 0.8f) Spawn(RandomFrom(speedsterPrefabs));
                    else Spawn(RandomFrom(heavyPrefabs));
                }

                if (!medicSpawned && ShouldSpawnMedic())
                {
                    Spawn(RandomFrom(medicPrefabs));
                    medicSpawned = true;
                }
                break;
            }

            case DirectorState.MicroBreath:
                break;
        }
    }

    GameObject RandomFrom(List<GameObject> pool)
    {
        if (pool == null || pool.Count == 0) return null;
        return pool[Random.Range(0, pool.Count)];
    }

    void Spawn(GameObject prefab)
    {
        if (prefab == null) return;

        Vector3 spawnPos;
        if (!TryGetSpawnPosition(out spawnPos)) return;

        GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.identity);

        EnemyBrain brain = enemy.GetComponent<EnemyBrain>();
        if (brain != null) brain.player = player;

        EnemyCombat combat = enemy.GetComponent<EnemyCombat>();
        if (combat != null) combat.player = player;

        EnemyHealth health = enemy.GetComponent<EnemyHealth>();
        if (health != null && MedicManager.Instance != null)
            MedicManager.Instance.RegisterEnemy(health);

        Roster.Register(enemy);
        NotifyEnemiesOfRosterChange();
    }

    bool TryGetSpawnPosition(out Vector3 result)
    {
        for (int i = 0; i < 10; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle.normalized;
            float distance = Random.Range(spawnMinDistanceFromPlayer, spawnMaxDistanceFromPlayer);

            Vector3 candidate = player.position + new Vector3(
                randomCircle.x, 0f, randomCircle.y) * distance;

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }

        Debug.LogWarning("[Director] Could not find valid spawn position after 10 attempts.");
        result = Vector3.zero;
        return false;
    }

    // ── Role rebalancing ──────────────────────────────────────────────────────
    void NotifyEnemiesOfRosterChange()
    {
        foreach (GameObject e in Roster.LiveEnemies)
        {
            if (e == null) continue;
            e.GetComponent<EnemyBrain>()?.ReevaluateRole();
        }
    }

    // ── Player performance tracking ───────────────────────────────────────────
    void TrackPlayerPerformance()
    {
        timeSinceLastDamage += Time.deltaTime;
        timeSinceLastAttack += Time.deltaTime;

        if (chaosLevel < overwhelmThreshold)
            timeInLowChaos += Time.deltaTime;
        else
            timeInLowChaos = 0f;

        if (!medicSpawned && ShouldSpawnMedic() &&
            currentState == DirectorState.ChaosSpike)
        {
            Spawn(RandomFrom(medicPrefabs));
            medicSpawned = true;
            ResetPerformanceTracking();
        }
    }

    bool ShouldSpawnMedic()
    {
        return killStreak >= killStreakThreshold ||
               timeSinceLastDamage >= noDamageTimeThreshold ||
               timeSinceLastAttack >= noAttackTimeThreshold ||
               timeInLowChaos >= lowChaosTimeThreshold;
    }

    void ResetPerformanceTracking()
    {
        killStreak = 0;
        timeSinceLastDamage = 0f;
        timeSinceLastAttack = 0f;
        timeInLowChaos = 0f;
        medicSpawned = false;
    }

    // ── External event hooks ──────────────────────────────────────────────────
    public void OnEnemyDied(GameObject enemy, bool killedByTrap)
    {
        Roster.Unregister(enemy);
        killStreak++;
        if (killedByTrap) evaluator.pendingTrapKills++;
        NotifyEnemiesOfRosterChange();
    }

    public void OnTrapKill() => evaluator.pendingTrapKills++;
    public void OnKnockback() => evaluator.pendingKnockbacks++;

    public void OnArcherHit()
    {
        evaluator.pendingArcherHits++;
        timeSinceLastAttack = 0f;
    }

    public void OnPlayerHit()
    {
        evaluator.pendingPlayerHits++;
        timeSinceLastDamage = 0f;
        killStreak = 0;
    }

    public void OnEnemyAttacked() => timeSinceLastAttack = 0f;

    // ── Restart ───────────────────────────────────────────────────────────────
public void RestartFromRound(int round = 1)
{
    StopAllCoroutines();
    currentRound = 0;
    endlessWave = 0;
    chaosLevel = 0f;
    chaosSpikesThisRound = 0;
    lastSpikeTime = -999f;
    Roster.Clear();
    ResetPerformanceTracking();
    StartGame(); 
}
}