using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using System.Collections.Generic;

public class SettingsUI : MonoBehaviour
{
    public TMP_Dropdown ResDropDown;
    public TMP_Dropdown QualityDropDown;
    public Slider MusicSlider;
    public Slider SFXSlider;
    public Toggle FullscreenToggle;
    public GameObject settingsMenuUI;

    public AudioMixer mixer;

    Resolution[] resolutions;
    List<Resolution> filtered = new List<Resolution>();

    void Start()
    {
        SetupResolutions();
        LoadFromGlobal();
    }

    void SetupResolutions()
    {
        ResDropDown.ClearOptions();
        resolutions = Screen.resolutions;

        List<string> options = new List<string>();

        foreach (var r in resolutions)
        {
            string option = r.width + "x" + r.height;
            if (!options.Contains(option))
            {
                options.Add(option);
                filtered.Add(r);
            }
        }

        ResDropDown.AddOptions(options);
    }

    void LoadFromGlobal()
    {
        var s = SettingsSystem.Instance;

        QualityDropDown.SetValueWithoutNotify(s.quality);

        MusicSlider.SetValueWithoutNotify(s.musicVolume);
        mixer.SetFloat("MusicVolume", s.musicVolume);

        SFXSlider.SetValueWithoutNotify(s.sfxVolume);
        mixer.SetFloat("SFXVolume", s.sfxVolume);

        FullscreenToggle.SetIsOnWithoutNotify(SettingsSystem.Instance.fullscreen);
        Screen.fullScreen = s.fullscreen;

        if (s.resolutionIndex < filtered.Count)
        {
            var r = filtered[s.resolutionIndex];
            Screen.SetResolution(r.width, r.height, s.fullscreen);
            ResDropDown.SetValueWithoutNotify(s.resolutionIndex);
        }
    }

    // ===== UI EVENTS =====

    public void SetQuality(int v)
    {
        SettingsSystem.Instance.quality = v;
        SettingsSystem.Instance.Save();
    }

    public void SetMusic(float v)
    {
        SettingsSystem.Instance.musicVolume = v;
        SettingsSystem.Instance.Save();
    }

    public void SetSFX(float v)
    {
        SettingsSystem.Instance.sfxVolume = v;
        SettingsSystem.Instance.Save();
    }

    public void SetFullscreen(bool v)
    {
        SettingsSystem.Instance.fullscreen = v;
        Screen.fullScreen = v;
        SettingsSystem.Instance.Save();
    }

    public void SetResolution(int i)
    {
        SettingsSystem.Instance.resolutionIndex = i;

        var r = filtered[i];
        Screen.SetResolution(r.width, r.height, SettingsSystem.Instance.fullscreen);

        SettingsSystem.Instance.Save();
    }

    public void PauseGame()
    {
        settingsMenuUI.SetActive(true);
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        settingsMenuUI.SetActive(false);
        Time.timeScale = 1f;
    }
}