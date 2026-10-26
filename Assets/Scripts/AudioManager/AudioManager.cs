using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Background Music")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioClip openingBGM;
    [SerializeField] private AudioClip mapBGM;
    [SerializeField] private AudioClip mainGameBGM;

    [Header("Shared SFX")]
    [SerializeField] private AudioSource sfxSource;

    [Header("Station SFX Sources")]
    [SerializeField]
    private List<AudioSource> stationSFXSources =
        new List<AudioSource>();

    [Header("Volume Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float bgmVolume = 1f;

    [Range(0f, 1f)]
    [SerializeField] private float sfxVolume = 1f;

    private const string BGM_VOLUME_KEY = "BGMVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";

    private readonly Dictionary<AudioSource, float> originalVolumes =
        new Dictionary<AudioSource, float>();

    private AudioClip currentClip;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        AudioSource[] sources = GetComponents<AudioSource>();

        if (bgmSource == null && sources.Length > 0)
            bgmSource = sources[0];

        if (sfxSource == null && sources.Length > 1)
            sfxSource = sources[1];

        bgmVolume = PlayerPrefs.GetFloat(BGM_VOLUME_KEY, 1f);
        sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f);

        if (bgmSource != null)
        {
            bgmSource.loop = true;
            bgmSource.volume = bgmVolume;
        }
        else
        {
            Debug.LogError("AudioManager: Assign the BGM AudioSource!");
        }

        if (sfxSource != null)
            sfxSource.volume = sfxVolume;

        CacheOriginalVolumes();

        SceneManager.sceneLoaded += OnSceneLoaded;

        string currentScene = SceneManager.GetActiveScene().name;
        PlayMusicForScene(currentScene);

        if (currentScene == "main_game")
            RegisterSceneAudioSources();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Instance = null;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayMusicForScene(scene.name);

        if (scene.name == "main_game")
            RegisterSceneAudioSources();
    }

    // ---------------- BGM ----------------

    private AudioClip GetBGMForScene(string sceneName)
    {
        switch (sceneName)
        {
            case "opening":
                return openingBGM;

            case "map":
            case "setting":
                return mapBGM;

            case "main_game":
                return mainGameBGM;

            default:
                return currentClip;
        }
    }

    private void PlayMusicForScene(string sceneName)
    {
        AudioClip clip = GetBGMForScene(sceneName);

        if (clip == null || clip == currentClip)
            return;

        if (bgmSource == null)
            return;

        currentClip = clip;
        bgmSource.clip = clip;
        bgmSource.volume = bgmVolume;
        bgmSource.loop = true;
        bgmSource.Play();

        Debug.Log("AudioManager: Playing BGM for " + sceneName);
    }

    public void PlayBGM(AudioClip clip)
    {
        if (clip == null || bgmSource == null)
            return;

        if (clip == currentClip && bgmSource.isPlaying)
            return;

        currentClip = clip;
        bgmSource.clip = clip;
        bgmSource.volume = bgmVolume;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        if (bgmSource != null)
            bgmSource.Stop();

        currentClip = null;
    }

    // ---------------- SFX REGISTRATION ----------------

    private void RegisterSceneAudioSources()
    {
        AudioSource[] sceneSources =
            FindObjectsOfType<AudioSource>(true);

        foreach (AudioSource source in sceneSources)
        {
            if (source == null ||
                source == bgmSource ||
                source == sfxSource ||
                source.transform == transform ||
                source.transform.IsChildOf(transform))
            {
                continue;
            }

            RegisterStationSFX(source);
        }

        ApplySFXVolume();

        Debug.Log(
            "AudioManager: Registered " +
            stationSFXSources.Count + " SFX AudioSources."
        );
    }

    public void RegisterStationSFX(AudioSource source)
    {
        if (source == null ||
            source == bgmSource ||
            source == sfxSource ||
            source.transform == transform ||
            source.transform.IsChildOf(transform))
        {
            return;
        }

        if (!stationSFXSources.Contains(source))
            stationSFXSources.Add(source);

        if (!originalVolumes.ContainsKey(source))
            originalVolumes.Add(source, source.volume);

        ApplyVolumeToSource(source);
    }

    public void UnregisterStationSFX(AudioSource source)
    {
        if (source == null)
            return;

        stationSFXSources.Remove(source);
        originalVolumes.Remove(source);
    }

    private void CacheOriginalVolumes()
    {
        foreach (AudioSource source in stationSFXSources)
        {
            if (source == null)
                continue;

            if (!originalVolumes.ContainsKey(source))
                originalVolumes.Add(source, source.volume);
        }
    }

    // ---------------- VOLUME CONTROLS ----------------

    public void SetBGMVolume(float volume)
    {
        volume = Mathf.Clamp01(volume);

        // Control the AudioSource that actually plays the music.
        if (BackgroundMusic.Instance != null)
        {
            BackgroundMusic.Instance.SetVolume(volume);
        }

        bgmVolume = volume;

        PlayerPrefs.SetFloat("BGMVolume", volume);
        PlayerPrefs.Save();

        Debug.Log("AudioManager: BGM volume = " + volume);
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);

        ApplySFXVolume();

        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, sfxVolume);
        PlayerPrefs.Save();

        Debug.Log("AudioManager: SFX volume = " + sfxVolume);
    }

    private void ApplySFXVolume()
    {
        if (sfxSource != null)
            sfxSource.volume = sfxVolume;

        for (int i = stationSFXSources.Count - 1; i >= 0; i--)
        {
            AudioSource source = stationSFXSources[i];

            if (source == null)
            {
                stationSFXSources.RemoveAt(i);
                continue;
            }

            ApplyVolumeToSource(source);
        }
    }

    private void ApplyVolumeToSource(AudioSource source)
    {
        if (source == null)
            return;

        if (originalVolumes.TryGetValue(source, out float baseVolume))
            source.volume = baseVolume * sfxVolume;
    }

    public float GetBGMVolume()
    {
        if (BackgroundMusic.Instance != null)
        {
            return BackgroundMusic.Instance.GetVolume();
        }

        return PlayerPrefs.GetFloat("BGMVolume", 1f);
    }

    public float GetSFXVolume()
    {
        return sfxVolume;
    }

    // ---------------- SHARED SFX PLAYBACK ----------------

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null || sfxSource == null)
            return;

        sfxSource.PlayOneShot(clip, 1f);
    }
}