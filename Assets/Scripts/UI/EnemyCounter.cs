using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyCounter : MonoBehaviour
{
    GameObject[] enemies;
    public TextMeshProUGUI enemyCountText;
    SceneManagerScript sceneManager;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        enemies = GameObject.FindGameObjectsWithTag("Enemy");
        enemyCountText.text = enemies.Length.ToString();

        if(enemies.Length == 0)
        {
            if(sceneManager != null)
            {
                sceneManager.OnPlayerWin();
            }
        }
    }
}