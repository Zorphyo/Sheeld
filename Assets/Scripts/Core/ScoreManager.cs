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

        score = Mathf.CeilToInt(damageDealt * (1.3f * (1 + multiHit)) + (20 * wavesDone));

        Debug.Log($"Score changed: {score} | DamageDealt: {damageDealt} | MultiHit: {multiHit} | Waves: {wavesDone}");
    }

    public void WaveCompleted()
    {
        wavesDone++;

        score = Mathf.CeilToInt(damageDealt * (1.3f * (1 + multiHit)) + (20 * wavesDone));

        Debug.Log($"Score changed: {score} | DamageDealt: {damageDealt} | MultiHit: {multiHit} | Waves: {wavesDone}");
    }

    public void TrapUsed()
    {
        trapsUsed++;

        score = Mathf.CeilToInt(damageDealt * (1.3f * (1 + multiHit)) + (20 * wavesDone));

        Debug.Log($"Score changed: {score} | DamageDealt: {damageDealt} | MultiHit: {multiHit} | Waves: {wavesDone} | TrapsUsed: {trapsUsed}");
    }
}