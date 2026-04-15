using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Audio;
using UnityEngine.UI; // Added for Sliders
using TMPro;
using System.Collections.Generic;

public class SettingsManagerScript : MonoBehaviour
{
    public RenderPipelineAsset lowQualityAsset;
    public RenderPipelineAsset mediumQualityAsset;
    public RenderPipelineAsset highQualityAsset;

    public AudioMixer mainAudioMixer;

    // References to UI elements so we can sync them on Load
    public TMP_Dropdown ResDropDown;
    public TMP_Dropdown QualityDropDown; 
    public Slider MusicSlider;
    public Slider SFXSlider;
    public Toggle FullScreenToggle;

    bool isFullScreen;
    Resolution[] AllResolutions;
    List<Resolution> SelectedResolutionList = new List<Resolution>();

    public GameObject settingsMenuUI;

    void Start()
    {
        SetupResolutionDropdown();
        LoadSettings();
    }

    void SetupResolutionDropdown()
    {
        ResDropDown.ClearOptions();
        AllResolutions = Screen.resolutions;
        List<string> options = new List<string>();

        for (int i = 0; i < AllResolutions.Length; i++)
        {
            string option = AllResolutions[i].width + " x " + AllResolutions[i].height;
            if (!options.Contains(option))
            {
                options.Add(option);
                SelectedResolutionList.Add(AllResolutions[i]);
            }
        }
        ResDropDown.AddOptions(options);
    }

    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
        PlayerPrefs.SetInt("QualityLevel", qualityIndex); // SAVE

        switch (qualityIndex)
        {
            case 0: GraphicsSettings.defaultRenderPipeline = lowQualityAsset; break;
            case 1: GraphicsSettings.defaultRenderPipeline = mediumQualityAsset; break;
            case 2: GraphicsSettings.defaultRenderPipeline = highQualityAsset; break;
        }
    }

    public void ChangeMusicVolume(float musicVol)
    {
        mainAudioMixer.SetFloat("MusicVolume", musicVol);
        PlayerPrefs.SetFloat("MusicVolume", musicVol); // SAVE
    }

    public void ChangeSFXVolume(float sfxVol)
    {
        mainAudioMixer.SetFloat("SFXVolume", sfxVol);
        PlayerPrefs.SetFloat("SFXVolume", sfxVol); // SAVE
    }

    public void SetFullScreen(bool IsFullScreen)
    {
        Screen.fullScreen = IsFullScreen;
        isFullScreen = IsFullScreen;
        PlayerPrefs.SetInt("FullScreen", IsFullScreen ? 1 : 0); // SAVE
    }

    public void ChangeResolution(int index)
    {
        Resolution res = SelectedResolutionList[index];
        Screen.SetResolution(res.width, res.height, isFullScreen);
        PlayerPrefs.SetInt("ResIndex", index); // SAVE
    }

    void LoadSettings()
    {
        if (SelectedResolutionList.Count == 0) return;
        
        // Load Quality
        int savedQuality = PlayerPrefs.GetInt("QualityLevel", 2); // Default to High (2)
        SetQuality(savedQuality);
        if(QualityDropDown != null) QualityDropDown.value = savedQuality;

        // Load Volume
        float savedMusic = PlayerPrefs.GetFloat("MusicVolume", 0f);
        mainAudioMixer.SetFloat("MusicVolume", savedMusic);
        if(MusicSlider != null) MusicSlider.value = savedMusic;

        float savedSFX = PlayerPrefs.GetFloat("SFXVolume", 0f);
        mainAudioMixer.SetFloat("SFXVolume", savedSFX);
        if(SFXSlider != null) SFXSlider.value = savedSFX;

        // Load Fullscreen
        isFullScreen = PlayerPrefs.GetInt("FullScreen", 1) == 1;
        Screen.fullScreen = isFullScreen;
        if(FullScreenToggle != null) FullScreenToggle.isOn = isFullScreen;

        // Load Resolution
        int savedRes = PlayerPrefs.GetInt("ResIndex", SelectedResolutionList.Count - 1);
        if (savedRes < SelectedResolutionList.Count)
        {
            ChangeResolution(savedRes);
            ResDropDown.value = savedRes;
            ResDropDown.RefreshShownValue();
        }
    }

    public void PauseGame() 
    { 
        settingsMenuUI.SetActive(true);
        if(ResDropDown != null) SetupResolutionDropdown();
        LoadSettings();
        Invoke("StopWorld", 0.01f); 
    }

    void StopWorld()
    {
        Time.timeScale = 0f; 
    }
    public void ResumeGame() { Time.timeScale = 1f; }
}
