using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScoreTrackUI : MonoBehaviour
{
    public int currentScore;
    public TextMeshProUGUI scoreCountText;
    public TextMeshProUGUI deathScoreText;
    public TextMeshProUGUI winScoreText;
    public TextMeshProUGUI newHighScoreText;
    public GameMode mode;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        currentScore = ScoreManager.Instance.score;
        scoreCountText.text = currentScore.ToString();
    }

    public void deathPopupScore(int score)
    {
        deathScoreText.text = score.ToString();
        mode = GameSession.CurrentMode;
        CheckHighScore(score);
        return;
    }

    public void winPopupScore(int score)
    {
        winScoreText.text = score.ToString();
        mode = GameSession.CurrentMode;
        CheckHighScore(score);
        return;
    }

    public void CheckHighScore(int currentScore)
    {
        List<ScoreEntry> scores = ScoreSystem.Instance.GetScores(mode);

        if (scores == null || scores.Count == 0)
        {
            Debug.Log("No scores found yet — treating as new high score");
            newHighScoreText.gameObject.SetActive(true);
            return;
        }

        int highScore = scores.Max(x => x.score);

        if (currentScore > highScore)
        {
            newHighScoreText.gameObject.SetActive(true);
            Debug.Log("NEW HIGH SCORE!");
        }
        return;
    }
}
