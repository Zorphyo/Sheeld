using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyHealthBar : MonoBehaviour
{
    public Slider enemyHealthBarSlider;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        enemyHealthBarSlider.maxValue = 100;
    }

    public void SetHealthBar(int current, int max)
    {
        if(current <= 0)
        {
            enemyHealthBarSlider.value = 0;
        }
        else if(current >= max)
        {
            enemyHealthBarSlider.value = max;
        }
        else
        {
            enemyHealthBarSlider.value = current;
        }
    }
}
