using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class LeaderboardUI : MonoBehaviour
{
    public GameMode mode;

    [System.Serializable]
    public class Row
    {
        public TMP_Text dateText;
        public TMP_Text timeText;
        public TMP_Text scoreText;
        public TMP_Text wavesText;
    }

    public Row[] rows; // size 5

    void OnEnable()
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        List<ScoreEntry> scores = ScoreSystem.Instance.GetScores(mode);

        for (int i = 0; i < rows.Length; i++)
        {
            if (i < scores.Count)
            {
                rows[i].dateText.text = scores[i].date;
                rows[i].timeText.text = scores[i].timeTaken.ToString();
                rows[i].scoreText.text = scores[i].score.ToString();

                if(rows[i].wavesText != null)
                {
                    rows[i].wavesText.text = scores[i].waves.ToString();
                }
            }
            else
            {
                rows[i].dateText.text = "---";
                rows[i].timeText.text = "---";
                rows[i].scoreText.text = "---";

                if(rows[i].wavesText != null)
                {
                    rows[i].wavesText.text = "---";
                }
            }
        }
    }
}