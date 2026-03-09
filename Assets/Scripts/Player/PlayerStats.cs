using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public class PlayerStats : MonoBehaviour
{
    PlayerController pc;
    SceneManagerScript sceneManager;

    public int MAX_HEALTH;
    public int MAX_STAMINA;

    public int DODGE_STAMINA_COST;
    public int SHIELD_BASH_STAMINA_COST;

    [HideInInspector] public int currentHealth;
    [HideInInspector] public int currentStamina;
    [HideInInspector] public bool staminaRegen = true;
    [HideInInspector] public bool staminaLockout = false;

    UnityEvent Death;

    void Awake()
    {
        pc = GetComponent<PlayerController>();
        sceneManager = FindFirstObjectByType<SceneManagerScript>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = MAX_HEALTH;
        currentStamina = MAX_STAMINA;

        if (Death == null)
        {
            Death = new UnityEvent();
        }

        Death.AddListener(Die);

        StartCoroutine(StaminaRegen(1));
        StartCoroutine(Sprinting(-1));
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ChangeHealth(int amount)
    {
        currentHealth += amount;

        if (currentHealth > MAX_HEALTH)
        {
            currentHealth = MAX_HEALTH;
        }

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Death.Invoke();
        }
    }

    public void ChangeStamina(int amount)
    {
        currentStamina += amount;

        if (currentStamina > MAX_STAMINA)
        {
            currentStamina = MAX_STAMINA;
        }

        if (currentStamina <= 0)
        {
            currentStamina = 0;
            staminaRegen = false;
            staminaLockout = true;

            pc.NoMoreStamina();

            InvokeRepeating("StaminaRecharge", 0.1f, 0.1f);
        }
    }

    IEnumerator StaminaRegen(int amount)
    {
        while (true)
        {
            yield return new WaitForSeconds(0.75f);

            if (staminaRegen && !staminaLockout && !pc.isSprinting && !pc.isBlocking && currentStamina < MAX_STAMINA)
            {
                ChangeStamina(amount);
            }
        }
    }

    void StaminaRecharge()
    {
        currentStamina += 1;

        if (currentStamina == MAX_STAMINA)
        {
            CancelInvoke("StaminaRecharge");
            staminaRegen = true;
            staminaLockout = false;

            pc.RegainedStamina();
        }
    }

    IEnumerator Sprinting(int amount)
    {
        while (true)
        {
            yield return new WaitForSeconds(0.25f);

            if (!staminaRegen && !staminaLockout && !pc.isBlocking && !pc.isDodging && currentStamina > 0)
            {
                ChangeStamina(amount);
            }
        }
    }

    public void Die()
    {
        staminaRegen = false;

        if(sceneManager != null)
        {
            sceneManager.OnPlayerDeath();
        }

        Destroy(this);
    }
}