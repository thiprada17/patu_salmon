
using UnityEngine;
using UnityEngine.SceneManagement;

public class BackgroundMusic : MonoBehaviour
{
    public static BackgroundMusic Instance { get; private set; }

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip bgm1;
    [SerializeField] private AudioClip bgm2;
    //[SerializeField] private AudioClip bgm3;

    private int currentTrack = -1;

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

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        PlayTrackForScene(SceneManager.GetActiveScene().name);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayTrackForScene(scene.name);
    }

    private void PlayTrackForScene(string sceneName)
    {
        int track = GetTrackForScene(sceneName);

        // Keep the current song playing if the track group hasn't changed.
        if (track == currentTrack)
            return;

        currentTrack = track;

        AudioClip clip = track switch
        {
            1 => bgm1,
            2 => bgm2,
            //3 => bgm3,
            _ => null
        };

        if (clip == null)
        {
            audioSource.Stop();
            audioSource.clip = null;
            return;
        }

        audioSource.clip = clip;
        audioSource.loop = true;
        audioSource.Play();
    }

    private int GetTrackForScene(string sceneName)
    {
        // BGM 1
        if (sceneName == "opening" ||
            sceneName == "signup" ||
            sceneName == "SelectCharacter"||
            sceneName == "idcard"||
            sceneName == "loading")
            return 1;

        // BGM 2
        if (sceneName == "map" ||
            sceneName == "setting")
            return 2;

        // BGM 3
        //if (sceneName == "Main_Game")
        //    return 3;

        return 0;
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
