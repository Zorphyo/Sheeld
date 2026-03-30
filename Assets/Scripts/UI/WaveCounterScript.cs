using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WaveCounterScript : MonoBehaviour
{
    public TextMeshProUGUI waveCountText;
    
    void OnEnable()
    {
        if (WaveManager.Instance != null)
            WaveManager.Instance.OnWaveStarted += UpdateWaveText;
    }

    void OnDisable()
    {
        if (WaveManager.Instance != null)
            WaveManager.Instance.OnWaveStarted -= UpdateWaveText;
    }

    void UpdateWaveText(int waveNumber)
    {
        waveCountText.text = waveNumber.ToString();
    }

}