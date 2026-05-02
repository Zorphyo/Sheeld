using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using System.Collections;

public class MusicHandler : MonoBehaviour
{
    [Header("Mixer")]
    public AudioMixerGroup mixerGroup;

    [Header("Scene Music")]
    public AudioClip mainMenuMusic;
    public AudioClip level1Music;
    public AudioClip level2Music;
    public AudioClip level3Music;

    [Header("Scene Names")]
    public string mainMenuSceneName = "MainMenu";
    public string level1SceneName = "Arena1";
    public string level2SceneName = "Arena2";
    public string level3SceneName = "Arena3";

    [Header("Settings")]
    public float volume = 1f;
    public float loopDelay = 5f;
    public bool dontDestroyOnLoad = true;

    private AudioSource audioSource;
    private static MusicHandler instance;
    private Coroutine loopCoroutine;

    void Awake()
    {
        if (dontDestroyOnLoad)
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.outputAudioMixerGroup = mixerGroup;
        audioSource.volume = volume;

        // Important: false, because we are manually looping with a delay.
        audioSource.loop = false;
        audioSource.playOnAwake = false;
    }

    void Start()
    {
        PlayMusicForCurrentScene();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayMusicForScene(scene.name);
    }

    private void PlayMusicForCurrentScene()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        PlayMusicForScene(currentSceneName);
    }

    private void PlayMusicForScene(string sceneName)
    {
        AudioClip selectedClip = null;

        if (sceneName == mainMenuSceneName)
        {
            selectedClip = mainMenuMusic;
        }
        else if (sceneName == level1SceneName)
        {
            selectedClip = level1Music;
        }
        else if (sceneName == level2SceneName)
        {
            selectedClip = level2Music;
        }
        else if (sceneName == level3SceneName)
        {
            selectedClip = level3Music;
        }

        if (selectedClip == null)
        {
            Debug.LogWarning("No music assigned for scene: " + sceneName);
            return;
        }

        if (audioSource.clip == selectedClip && audioSource.isPlaying)
            return;

        if (loopCoroutine != null)
        {
            StopCoroutine(loopCoroutine);
        }

        audioSource.clip = selectedClip;
        loopCoroutine = StartCoroutine(PlayWithLoopDelay());
    }

    private IEnumerator PlayWithLoopDelay()
    {
        while (true)
        {
            audioSource.Play();

            yield return new WaitForSeconds(audioSource.clip.length);

            audioSource.Stop();

            yield return new WaitForSeconds(loopDelay);
        }
    }

    public void StopMusic()
    {
        if (loopCoroutine != null)
        {
            StopCoroutine(loopCoroutine);
            loopCoroutine = null;
        }

        audioSource.Stop();
    }

    public void PauseMusic()
    {
        audioSource.Pause();
    }

    public void ResumeMusic()
    {
        audioSource.UnPause();
    }

    public void SetVolume(float newVolume)
    {
        volume = newVolume;

        if (audioSource != null)
            audioSource.volume = volume;
    }
}