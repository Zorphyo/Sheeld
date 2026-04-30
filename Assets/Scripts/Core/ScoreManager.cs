using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Score Values")]
    public int score = 0;
    public int damageDealt = 0;
    public int multiHit = 0;
    public int wavesDone = 0;
    public int trapsUsed = 0;

    public float timeSpent = 0; //Need to implement

    [Header("Enemy Death Counts")]
    public int basicDeaths = 0;
    public int heavyDeaths = 0;
    public int archerDeaths = 0;
    public int speedsterDeaths = 0;
    public int medicDeaths = 0;

    [Header("Multi-Hit Settings")]
    public float multiHitTimeframe = 2f;

    private float lastDamageTime = -999f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        score = 0;
        damageDealt = 0;
        multiHit = 0;
        wavesDone = 0;
        trapsUsed = 0;
        timeSpent = 0;
        basicDeaths = 0;
        heavyDeaths = 0;
        archerDeaths = 0;
        speedsterDeaths = 0;
        medicDeaths = 0;
    }

    void Update()
    {
        timeSpent += Time.deltaTime;
    }

    public void ReportDamage(int damageAmount)
    {
        if (damageAmount <= 0)
            return;

        if (Time.time <= lastDamageTime + multiHitTimeframe)
        {
            multiHit++;
        }

        damageDealt += damageAmount;
        lastDamageTime = Time.time;

        ChangeScore();
    }

    public void WaveCompleted()
    {
        wavesDone++;
        ChangeScore();
    }

    public void TrapUsed()
    {
        trapsUsed++;
        ChangeScore();
    }

    public void ReportEnemyDeath(string enemyName)
    {
        if (string.IsNullOrEmpty(enemyName))
            return;

        if (enemyName.StartsWith("rdEnemy_Basic"))
        {
            basicDeaths++;
        }
        else if (enemyName.StartsWith("rdEnemy_Heavy"))
        {
            heavyDeaths++;
        }
        else if (enemyName.StartsWith("rdEnemy_Archer"))
        {
            archerDeaths++;
        }
        else if (enemyName.StartsWith("rdEnemy_Speedster"))
        {
            speedsterDeaths++;
        }
        else if (enemyName.StartsWith("rdEnemy_Medic"))
        {
            medicDeaths++;
        }
        else
        {
            Debug.LogWarning("Unknown enemy type for score tracking: " + enemyName);
        }

        ChangeScore();

        Debug.Log(
            $"Enemy Deaths | Basic: {basicDeaths} | Heavy: {heavyDeaths} | Archer: {archerDeaths} | Speedster: {speedsterDeaths} | Medic: {medicDeaths}"
        );
    }

    public void ChangeScore()
    {
        score = Mathf.CeilToInt((float)((damageDealt * (1 + 0.05 * trapsUsed)) + (1.3f * (1 + multiHit)) + (20 * wavesDone)) + (10 * basicDeaths + 20 * (archerDeaths + speedsterDeaths) + 30 * heavyDeaths + 27 * medicDeaths));
        Debug.Log($"Score changed: {score} | DamageDealt: {damageDealt} | MultiHit: {multiHit} | Waves: {wavesDone} | TrapsUsed: {trapsUsed}");
    }

    public void EndRun()
    {
        ScoreSystem.Instance.AddScore(
            GameSession.CurrentMode, 
            score,
            timeSpent,
            wavesDone
        );

        return;
    }
}