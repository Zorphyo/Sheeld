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
        // stop the director
        DirectorAI.Instance?.StopAllCoroutines();
    }

    public void OnPlayerWin()
    {
        winPopup.SetActive(true);
        Time.timeScale = 0f;
    }


    public void Restart()
    {
        Time.timeScale = 1f;
        // reload current scene and let director resume
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void EndGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void Exit()
    {
        Application.Quit();
    }
}
