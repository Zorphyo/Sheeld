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

    public void Play()
    {
        if (DirectorAI.Instance != null)
        {
            DirectorAI.Instance.endlessMode = false;
            DirectorAI.Instance.StartGame();
        }
    }

    public void PlayEndless()
    {
        if (DirectorAI.Instance != null)
        {
            DirectorAI.Instance.endlessMode = true;
            DirectorAI.Instance.StartGame();
        }
    }

    public void Exit()
    {
        Application.Quit();
    }
}