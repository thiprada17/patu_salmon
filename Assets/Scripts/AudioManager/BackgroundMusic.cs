using UnityEngine;
using UnityEngine.SceneManagement;

public class BackgroundMusic : MonoBehaviour
{
    public static BackgroundMusic Instance { get; private set; }

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;

    [SerializeField] private AudioClip bgm1; // Opening
    [SerializeField] private AudioClip bgm2; // Map / Settings
    [SerializeField] private AudioClip bgm3; // Main Game

    private const string BGM_VOLUME_KEY = "BGMVolume";

    private int currentTrack = -1;
    private float bgmVolume = 1f;

    public float Volume => bgmVolume;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            Debug.LogError(
                "BackgroundMusic: No AudioSource assigned or attached!"
            );
            return;
        }

        bgmVolume = PlayerPrefs.GetFloat(BGM_VOLUME_KEY, 1f);
        audioSource.volume = bgmVolume;
        audioSource.loop = true;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        PlayTrackForScene(SceneManager.GetActiveScene().name);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Additive scenes such as the recipe book are overlays. They should
        // not replace or stop the music from the active gameplay scene.
        if (mode == LoadSceneMode.Additive)
            return;

        PlayTrackForScene(scene.name);
    }

    private void PlayTrackForScene(string sceneName)
    {
        int track = GetTrackForScene(sceneName);

        // Keep the current song playing if the track hasn't changed.
        if (track == currentTrack)
            return;

        currentTrack = track;

        AudioClip clip = track switch
        {
            1 => bgm1,
            2 => bgm2,
            3 => bgm3,
            _ => null
        };

        if (audioSource == null)
            return;

        if (clip == null)
        {
            audioSource.Stop();
            audioSource.clip = null;

            Debug.LogWarning(
                "BackgroundMusic: No BGM assigned for scene " + sceneName
            );
            return;
        }

        audioSource.clip = clip;
        audioSource.loop = true;
        audioSource.volume = bgmVolume;
        audioSource.Play();

        Debug.Log("BackgroundMusic: Playing track " + track);
    }

    private int GetTrackForScene(string sceneName)
    {
        // BGM 1: Opening group
        if (sceneName == "opening" ||
            sceneName == "signup" ||
            sceneName == "SelectCharacter" ||
            sceneName == "idcard" ||
            sceneName == "loading")
        {
            return 1;
        }

        // BGM 2: Map and Settings
        if (sceneName == "map" ||
            sceneName == "setting")
        {
            return 2;
        }

        // BGM 3: Gameplay
        if (sceneName == "main_game")
        {
            return 3;
        }

        return 0;
    }

    // Called by the Settings BGM slider.

    public void SetVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);

        if (audioSource != null)
            audioSource.volume = volume;

        PlayerPrefs.SetFloat("BGMVolume", volume);
        PlayerPrefs.Save();

        Debug.Log("BackgroundMusic: Volume = " + volume);
    }

    public float GetVolume()
    {
        return audioSource != null
            ? audioSource.volume
            : PlayerPrefs.GetFloat("BGMVolume", 1f);
    }


    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Instance = null;
        }
    }
}
