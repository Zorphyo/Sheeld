using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class LeaderboardUI : MonoBehaviour
{
    public GameMode mode;
    public TMP_Text[] scoreTexts;

    void OnEnable()
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        var scores = ScoreSystem.Instance.GetScores(mode);

        for (int i = 0; i < scoreTexts.Length; i++)
        {
            if (i < scores.Count)
            {
                var s = scores[i];

                string waveText = (mode == GameMode.Standard)
                    ? ""
                    : " | W:" + s.waves;

                scoreTexts[i].text =
                    (i + 1) + ". " +
                    " | " + s.date +
                    " | " + s.timeTaken.ToString("F1") + "s" +
                    s.score +
                    waveText;
            }
            else
            {
                scoreTexts[i].text = (i + 1) + ". ---";
            }
        }
    }
}