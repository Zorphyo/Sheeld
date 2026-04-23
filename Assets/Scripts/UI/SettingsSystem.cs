using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using System.Collections.Generic;
using TigerForge;

public class SettingsSystem : MonoBehaviour
{
    public static SettingsSystem Instance;

    EasyFileSave file;

    // ===== ACTUAL SETTINGS DATA =====
    public int quality = 2;
    public float musicVolume = 0f;
    public float sfxVolume = 0f;
    public bool fullscreen = true;
    public int resolutionIndex = 0;

    void Awake()
    {
        // Singleton (global instance)
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        file = new EasyFileSave("SettingsFile");

        Load();
    }

    // ===== LOAD / SAVE =====

    public void Load()
    {
        if (!file.Load()) return;

        quality = file.GetInt("QualityLevel", 2);
        musicVolume = file.GetFloat("MusicVolume", 0f);
        sfxVolume = file.GetFloat("SFXVolume", 0f);
        fullscreen = file.GetBool("FullScreen", true);
        resolutionIndex = file.GetInt("ResIndex", 0);
    }

    public void Save()
    {
        file.Add("QualityLevel", quality);
        file.Add("MusicVolume", musicVolume);
        file.Add("SFXVolume", sfxVolume);
        file.Add("FullScreen", fullscreen);
        file.Add("ResIndex", resolutionIndex);

        file.Save();
    }
}