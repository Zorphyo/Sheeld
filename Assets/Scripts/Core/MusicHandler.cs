using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class MusicHandler : MonoBehaviour
{
    [System.Serializable]
    public class SceneMusic
    {
        public string sceneName;
        public AudioClip musicClip;
    }

    [Header("Mixer")]
    public AudioMixerGroup mixerGroup;

    [Header("Scene Music")]
    public AudioClip mainMenuMusic;
    public AudioClip arena1Music;
    public AudioClip arena2Music;
    public AudioClip arena3Music;

    [Header("Scene Names")]
    public string mainMenuSceneName = "MainMenu";
    public string level1SceneName = "Arena1";
    public string level2SceneName = "Arena2";
    public string level3SceneName = "Arena3";

    [Header("Settings")]
    public float volume = 1f;
    public bool loopMusic = true;
    public bool dontDestroyOnLoad = true;

    private AudioSource audioSource;
    private static MusicHandler instance;

    void Awake()
    {
        // Prevent duplicate music managers when changing scenes
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
        audioSource.loop = loopMusic;
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
            selectedClip = arena1Music;
        }
        else if (sceneName == level2SceneName)
        {
            selectedClip = arena2Music;
        }
        else if (sceneName == level3SceneName)
        {
            selectedClip = arena3Music;
        }

        if (selectedClip == null)
        {
            Debug.LogWarning("No music assigned for scene: " + sceneName);
            return;
        }

        // Do not restart the same song if it is already playing
        if (audioSource.clip == selectedClip && audioSource.isPlaying)
            return;

        audioSource.clip = selectedClip;
        audioSource.Play();
    }

    public void StopMusic()
    {
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