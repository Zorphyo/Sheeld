using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScoreTrackUI : MonoBehaviour
{
    public int currentScore;
    public TextMeshProUGUI scoreCountText;
    public TextMeshProUGUI deathScoreText;

    public TextMeshProUGUI winScoreText;
    
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
        return;
    }

    public void winPopupScore(int score)
    {
        winScoreText.text = score.ToString();
        return;
    }
}
