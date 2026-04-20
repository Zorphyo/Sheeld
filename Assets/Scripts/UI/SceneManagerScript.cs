using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerScript : MonoBehaviour
{
    [SerializeField]
    private float _timeToWaitBeforeExit;
    public GameObject deathPopup;
    public GameObject winPopup;
    public void OnPlayerDeath()
    {
        deathPopup.SetActive(true);
        Time.timeScale = 0f;
    }

    public void OnPlayerWin()
    {
        winPopup.SetActive(true);
        Time.timeScale = 0f;
    }

    public void EndGame()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void Exit()
    {
        Application.Quit();
    }
}
