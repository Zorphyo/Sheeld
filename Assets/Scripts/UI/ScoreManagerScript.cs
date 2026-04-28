using UnityEngine;

public class ScoreManagerScript : MonoBehaviour
{
    public int finalScore;
    public float timeSurvived;
    public int wavesCompleted;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Add score logic
        finalScore = 0;
        wavesCompleted = 0;
        timeSurvived = 0;
    }

    // Update is called once per frame
    void Update()
    {
        //Add score logic
        finalScore++;
        wavesCompleted = DirectorAI.Instance.CurrentRound;
    }

    public void EndRun()
    {
        ScoreSystem.Instance.AddScore(
            GameSession.CurrentMode, 
            finalScore,
            timeSurvived,
            wavesCompleted
        );
    }
}
