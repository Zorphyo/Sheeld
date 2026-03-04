using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StaminaBarScript : MonoBehaviour
{
    public Slider staminaBarSlider;
    public TextMeshProUGUI staminaBarValueText;

    public int maxStamina;
    public int currStamina;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currStamina = maxStamina;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.DownArrow))
            currStamina--;

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

        staminaBarSlider.maxValue = maxStamina;
    }
}
