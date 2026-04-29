using UnityEngine;
using System;
using System.Collections.Generic;
using TigerForge;

public class ScoreSystem : MonoBehaviour
{
    public static ScoreSystem Instance;

    private Dictionary<GameMode, List<ScoreEntry>> scores = new Dictionary<GameMode, List<ScoreEntry>>();

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        foreach (GameMode mode in System.Enum.GetValues(typeof(GameMode)))
        {
            LoadScores(mode);
        }
    }

    string GetFileName(GameMode mode)
    {
        return "Scores_" + mode.ToString();
    }

    //Load all scores
    void LoadAllScores()
    {
        foreach (GameMode mode in System.Enum.GetValues(typeof(GameMode)))
        {
            LoadScores(mode);
        }
    }

    //Load only one mode's scores
    void LoadScores(GameMode mode)
    {
        EasyFileSave file = new EasyFileSave(GetFileName(mode));
        if (file.Load())
        {
            scores[mode] = (List<ScoreEntry>)file.GetDeserialized(
                "scores",
                typeof(List<ScoreEntry>)
            );
            file.Dispose();
        }
        else
        {
            scores[mode] = new List<ScoreEntry>();
        }
    }

    //Save one mode's scores
    void SaveScores(GameMode mode)
    {
        EasyFileSave file = new EasyFileSave(GetFileName(mode));

        file.AddSerialized("scores", scores[mode]);
        file.Save();
    }

    //Add a new score (if in top 5)
    public void AddScore(GameMode mode, int score, float timeTaken, int waves)
    {
        if (!scores.ContainsKey(mode))
            scores[mode] = new List<ScoreEntry>();

        ScoreEntry entry = new ScoreEntry
        {
            score = score,
            date = DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
            timeTaken = timeTaken,
            waves = waves
        };

        scores[mode].Add(entry);
        scores[mode].Sort((a, b) => b.score.CompareTo(a.score));

        if (scores[mode].Count > 5)
            scores[mode].RemoveRange(5, scores[mode].Count - 5);

        SaveScores(mode);
    }

    //Get scores for leaderboard
    public List<ScoreEntry> GetScores(GameMode mode)
    {
        if (!scores.ContainsKey(mode))
            return new List<ScoreEntry>();

        return scores[mode];
    }
}