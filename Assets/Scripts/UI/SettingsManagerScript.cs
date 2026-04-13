using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Audio;
using TMPro;
using System.Collections.Generic;

public class SettingsManagerScript : MonoBehaviour
{
    public RenderPipelineAsset lowQualityAsset;
    public RenderPipelineAsset mediumQualityAsset;
    public RenderPipelineAsset highQualityAsset;

    public AudioMixer mainAudioMixer;

    bool isFullScreen;
    public TMP_Dropdown ResDropDown;
    Resolution[] AllResolutions;
    int SelectedResolution;
    List<Resolution> SelectedResolutionList = new List<Resolution>();

    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);

        switch (qualityIndex)
        {
            case 0: // Low
                GraphicsSettings.defaultRenderPipeline = lowQualityAsset;
                break;
            case 1: // Medium
                GraphicsSettings.defaultRenderPipeline = mediumQualityAsset;
                break;
            case 2: // High
                GraphicsSettings.defaultRenderPipeline = highQualityAsset;
                break;
        }
    }

    public void ChangeMusicVolume(float musicVol)
    {
        mainAudioMixer.SetFloat("MusicVolume", musicVol);
    }
    public void ChangeSFXVolume(float sfxVol)
    {
        mainAudioMixer.SetFloat("SFXVolume", sfxVol);
    }

    public void SetFullScreen(bool IsFullScreen)
    {
        Screen.fullScreen = IsFullScreen;
        isFullScreen = IsFullScreen;
    }

    public void ChangeResolution()
    {
        SelectedResolution = ResDropDown.value;
        Screen.SetResolution(SelectedResolutionList[SelectedResolution].width, SelectedResolutionList[SelectedResolution].height, isFullScreen);
    }
    void Start()
    {
        Screen.fullScreen = true;
        isFullScreen = true;
        
        ResDropDown.ClearOptions();
        AllResolutions = Screen.resolutions;

        List<string> options = new List<string>();
        int currentResIndex = 0;

        for (int i = 0; i < AllResolutions.Length; i++)
        {
            string option = AllResolutions[i].width + " x " + AllResolutions[i].height;

            if (!options.Contains(option))
            {
                options.Add(option);
                SelectedResolutionList.Add(AllResolutions[i]);
            }

            // Automatically find which one matches your monitor right now
            if (AllResolutions[i].width == Screen.currentResolution.width &&
                AllResolutions[i].height == Screen.currentResolution.height)
            {
                currentResIndex = i;
            }
        }

        ResDropDown.AddOptions(options);
        ResDropDown.value = currentResIndex;
        ResDropDown.RefreshShownValue();
    }
}



