using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StaminaBar : MonoBehaviour
{
    public Slider staminaBarSlider;
    public TextMeshProUGUI staminaBarValueText;
    public Image staminaBarFill;

    public int maxStamina;
    public int currStamina;

    PlayerStats playerStats;

    public float pulseSpeed = 10f;
    public float shakeAmount = 2f;
    private Vector3 originalPos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerStats = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStats>();
        maxStamina = playerStats.MAX_STAMINA;

        staminaBarSlider.maxValue = maxStamina;

        originalPos = staminaBarSlider.transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        currStamina = playerStats.currentStamina;

        staminaBarSlider.value = Mathf.Clamp(currStamina, 0, maxStamina);

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

        if (playerStats.staminaLockout)
        {
            staminaBarFill.color = Color.gray;
            staminaBarSlider.transform.localPosition = originalPos;
        }
        else if(currStamina < maxStamina * 0.3f)
        {
            float lerp = Mathf.PingPong(Time.time * pulseSpeed, 1);
            staminaBarFill.color = Color.Lerp(Color.green, Color.gray, lerp);
            staminaBarSlider.transform.localPosition = originalPos + (Vector3)Random.insideUnitCircle * shakeAmount;
        }
        else
        {
            staminaBarFill.color = Color.green;
            staminaBarSlider.transform.localPosition = originalPos;
        }
    }
}