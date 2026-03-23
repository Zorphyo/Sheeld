using UnityEngine;
using UnityEngine.UI;

public class HealthEffectScript : MonoBehaviour
{
    public Image LowHealthEffect;

    PlayerStats playerStats;
    public int maxHealth;
    public int currHealth;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerStats = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStats>();
        maxHealth = playerStats.MAX_HEALTH;
    }

    // Update is called once per frame
    void Update()
    {
        currHealth = playerStats.currentHealth;

        float healthPercent = (float)currHealth / maxHealth;

        float targetAlpha = 0f;

        if(healthPercent < 0.5f)
        {
            targetAlpha = 1f - (healthPercent * 2f);
        }

        Color c = LowHealthEffect.color;
        c.a = Mathf.Lerp(c.a, targetAlpha, Time.deltaTime * 5f);
        LowHealthEffect.color = c;
    }
}
