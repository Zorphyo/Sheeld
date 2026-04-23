using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using TigerForge;

public class SettingsManagerScript : MonoBehaviour
{
    EasyFileSave myFile;

    // Graphics
    public RenderPipelineAsset lowQualityAsset;
    public RenderPipelineAsset mediumQualityAsset;
    public RenderPipelineAsset highQualityAsset;

    public AudioMixer mainAudioMixer;

    // UI
    public TMP_Dropdown ResDropDown;
    public TMP_Dropdown QualityDropDown; 
    public Slider MusicSlider;
    public Slider SFXSlider;
    public Toggle FullScreenToggle;

    public GameObject settingsMenuUI;

    bool isFullScreen;
    bool isInitialized = false;

    Resolution[] AllResolutions;
    List<Resolution> SelectedResolutionList = new List<Resolution>();

    void Awake()
    {
        myFile = new EasyFileSave("SettingsFile");
    }

    void Start()
    {
        Debug.Log("=== SETTINGS MANAGER START ===");

        Debug.Log("ResDropDown: " + (ResDropDown == null ? "NULL ❌" : "OK ✅"));
        Debug.Log("QualityDropDown: " + (QualityDropDown == null ? "NULL ❌" : "OK ✅"));
        Debug.Log("MusicSlider: " + (MusicSlider == null ? "NULL ❌" : "OK ✅"));
        Debug.Log("SFXSlider: " + (SFXSlider == null ? "NULL ❌" : "OK ✅"));
        Debug.Log("FullScreenToggle: " + (FullScreenToggle == null ? "NULL ❌" : "OK ✅"));

        InitializeUI();
    }

    void InitializeUI()
    {
        SetupResolutionDropdown();
        LoadSettings();
    }

    void SetupResolutionDropdown()
    {
        if (ResDropDown == null) return;

        ResDropDown.ClearOptions();
        SelectedResolutionList.Clear();

        AllResolutions = Screen.resolutions;
        List<string> options = new List<string>();

        foreach (var res in AllResolutions)
        {
            string option = res.width + " x " + res.height;

            if (!options.Contains(option))
            {
                options.Add(option);
                SelectedResolutionList.Add(res);
            }
        }

        ResDropDown.AddOptions(options);
    }

    // ===== SETTINGS =====

    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);

        switch (qualityIndex)
        {
            case 0: GraphicsSettings.defaultRenderPipeline = lowQualityAsset; break;
            case 1: GraphicsSettings.defaultRenderPipeline = mediumQualityAsset; break;
            case 2: GraphicsSettings.defaultRenderPipeline = highQualityAsset; break;
        }

        myFile.Add("QualityLevel", qualityIndex);
        myFile.Save();
    }

    public void ChangeMusicVolume(float musicVol)
    {
        mainAudioMixer.SetFloat("MusicVolume", musicVol);
        myFile.Add("MusicVolume", musicVol);
        myFile.Save();
    }

    public void ChangeSFXVolume(float sfxVol)
    {
        mainAudioMixer.SetFloat("SFXVolume", sfxVol);
        myFile.Add("SFXVolume", sfxVol);
        myFile.Save();
    }

    public void SetFullScreen(bool value)
    {
        Screen.fullScreen = value;
        isFullScreen = value;

        myFile.Add("FullScreen", value);
        myFile.Save();
    }

    public void ChangeResolution(int index)
    {
        if (index < 0 || index >= SelectedResolutionList.Count) return;

        Resolution res = SelectedResolutionList[index];
        Screen.SetResolution(res.width, res.height, isFullScreen);

        myFile.Add("ResIndex", index);
        myFile.Save();
    }

    void LoadSettings()
    {
        if (!myFile.Load())
        {
            Debug.Log("No save file found, using defaults.");
            return;
        }
    
        // Quality
        int quality = myFile.GetInt("QualityLevel", 2);
        QualitySettings.SetQualityLevel(quality);
    
        if (QualityDropDown != null)
            QualityDropDown.SetValueWithoutNotify(quality);
    
        // Music
        float music = myFile.GetFloat("MusicVolume", 0f);
        mainAudioMixer.SetFloat("MusicVolume", music);
        if (MusicSlider != null)
            MusicSlider.SetValueWithoutNotify(music);
    
        // SFX
        float sfx = myFile.GetFloat("SFXVolume", 0f);
        mainAudioMixer.SetFloat("SFXVolume", sfx);
        if (SFXSlider != null)
            SFXSlider.SetValueWithoutNotify(sfx);
    
        // Fullscreen
        isFullScreen = myFile.GetBool("FullScreen", true);
        Screen.fullScreen = isFullScreen;
        if (FullScreenToggle != null)
            FullScreenToggle.SetIsOnWithoutNotify(isFullScreen);
    
        // Resolution
        int resIndex = myFile.GetInt("ResIndex", SelectedResolutionList.Count - 1);
    
        if (resIndex >= 0 && resIndex < SelectedResolutionList.Count)
        {
            Resolution res = SelectedResolutionList[resIndex];
            Screen.SetResolution(res.width, res.height, isFullScreen);
    
            if (ResDropDown != null)
                ResDropDown.SetValueWithoutNotify(resIndex);
        }
    }

    // ===== PAUSE SYSTEM =====

    public void PauseGame()
    {
        settingsMenuUI.SetActive(true);
       InitializeUI();
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        settingsMenuUI.SetActive(false);
        Time.timeScale = 1f;
    }

    void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        InitializeUI();
    }
}