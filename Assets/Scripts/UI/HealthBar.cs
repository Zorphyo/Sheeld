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
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currHealth = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
            currHealth--;

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

        healthBarSlider.maxValue = maxHealth;
    }
}
