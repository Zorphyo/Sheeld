using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBar : MonoBehaviour
{
    public Slider healthBarSlider;
    public TextMeshProUGUI healthBarValueText;

    public int maxHealth;
    public int currHealth;

    PlayerStats playerStats;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerStats = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStats>();
        maxHealth = playerStats.MAX_HEALTH;

        healthBarSlider.maxValue = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        currHealth = playerStats.currentHealth;

        if(currHealth <= 0)
        {
            healthBarSlider.value = 0;
            healthBarValueText.text = "0/" + maxHealth.ToString();
        }
        else if(currHealth >= maxHealth)
        {
            healthBarSlider.value = maxHealth;
            healthBarValueText.text = maxHealth.ToString() + "/" + maxHealth.ToString();
        }
        else
        {
            healthBarSlider.value = currHealth;
            healthBarValueText.text = currHealth.ToString() + "/" + maxHealth.ToString();
        }
    }
}