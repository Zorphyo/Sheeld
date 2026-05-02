using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuScript : MonoBehaviour
{
    void Start()
    {
        SceneManager.LoadScene("PhotoScene", LoadSceneMode.Additive);
    }

    public void PlayStandard()
    {
        if (DirectorAI.Instance != null)
        {
            GameSession.CurrentMode = GameMode.Standard;
            DirectorAI.Instance.endlessMode = false;
            DirectorAI.Instance.StartGame();
        }
    }

    public void PlayEndlessForest()
    {
        if (DirectorAI.Instance != null)
        {
            GameSession.CurrentMode = GameMode.EndlessForest;
            DirectorAI.Instance.endlessMode = true;
            DirectorAI.Instance.StartGame(0);
        }
    }

    //Change to actually go to icy
    public void PlayEndlessIcy()
    {
        if (DirectorAI.Instance != null)
        {
            GameSession.CurrentMode = GameMode.EndlessIcy;
            DirectorAI.Instance.endlessMode = true;
            DirectorAI.Instance.StartGame(1);
        }
    }

    //Change to actually go to lava
    public void PlayEndlessLava()
    {
        if (DirectorAI.Instance != null)
        {
            GameSession.CurrentMode = GameMode.EndlessLava;
            DirectorAI.Instance.endlessMode = true;
            DirectorAI.Instance.StartGame(2);
        }
    }

    public void Exit()
    {
        Application.Quit();
    }
}