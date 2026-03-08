using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StaminaBar : MonoBehaviour
{
    public Slider staminaBarSlider;
    public TextMeshProUGUI staminaBarValueText;

    public int maxStamina;
    public int currStamina;

    PlayerStats playerStats;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerStats = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStats>();
        maxStamina = playerStats.MAX_STAMINA;

        staminaBarSlider.maxValue = maxStamina;
    }

    // Update is called once per frame
    void Update()
    {
        currStamina = playerStats.currentStamina;

        if(currStamina <= 0)
        {
            staminaBarSlider.value = 0;
            staminaBarValueText.text = "0/" + maxStamina.ToString();
        }
        else if(currStamina >= maxStamina)
        {
            staminaBarSlider.value = maxStamina;
            staminaBarValueText.text = maxStamina.ToString() + "/" + maxStamina.ToString();
        }
        else
        {
            staminaBarSlider.value = currStamina;
            staminaBarValueText.text = currStamina.ToString() + "/" + maxStamina.ToString();
        }
    }
}