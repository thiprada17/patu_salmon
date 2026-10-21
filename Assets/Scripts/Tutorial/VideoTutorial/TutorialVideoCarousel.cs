
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;

public class TutorialVideoCarousel : MonoBehaviour
{
    [System.Serializable]
    public class VideoPage
    {
        public VideoClip clip;
        public string title;
    }

    [System.Serializable]
    public class TutorialTopic
    {
        public string topicName;
        public Sprite headerSprite;

        [TextArea(2, 5)]
        public string description;

        public VideoPage[] pages;
    }

    [Header("Video Player")]
    [SerializeField] private VideoPlayer videoPlayer;

    [Header("UI References")]
    [SerializeField] private Image sectionTitle;
    [SerializeField] private TMP_Text descriptionText;

    [Header("Page Dots - UI Images")]
    [SerializeField] private Image[] dots;

    [SerializeField]
    private Color activeColor = new Color32(255, 165, 31, 255);

    [SerializeField]
    private Color inactiveColor = new Color32(210, 192, 160, 255);

    [Header("Tutorial Topics")]
    [SerializeField] private TutorialTopic[] topics = new TutorialTopic[3];

    [Header("Popup Closing")]
    [SerializeField] private GameObject tutorialPopup;
    [SerializeField] private GameObject dimBackground;

    [Header("Scene Settings")]
    [SerializeField] private string tutorialSceneName = "TutorialVDO";
    [SerializeField] private string mainSceneName = "main_game";

    private int currentTopicIndex = 0;
    private int currentVideoIndex = 0;

    private float previousTimeScale = 1f;
    private bool isClosing = false;

    private void Awake()
    {
        // Remember the previous time scale.
        previousTimeScale = Time.timeScale;

        // Pause gameplay and block gameplay input.
        Time.timeScale = 0f;
        GameplayInputLock.Lock();

        Debug.Log(
            "[Tutorial] TutorialVDO opened. Gameplay paused and input locked."
        );
    }

    private void Start()
    {
        if (topics == null || topics.Length == 0)
        {
            Debug.LogError(
                "[Tutorial] No tutorial topics assigned.", this
            );
            return;
        }

        ShowPage();
    }

    public void NextPage()
    {
        if (isClosing)
            return;

        TutorialTopic topic = GetCurrentTopic();

        if (!HasValidPages(topic))
            return;

        if (currentVideoIndex < topic.pages.Length - 1)
        {
            currentVideoIndex++;
            ShowPage();
            return;
        }

        if (currentTopicIndex < topics.Length - 1)
        {
            currentTopicIndex++;
            currentVideoIndex = 0;
            ShowPage();
            return;
        }

        CloseTutorialScene();
    }

    public void PreviousPage()
    {
        if (isClosing)
            return;

        if (currentVideoIndex > 0)
        {
            currentVideoIndex--;
        }
        else if (currentTopicIndex > 0)
        {
            currentTopicIndex--;

            TutorialTopic previousTopic = GetCurrentTopic();

            if (!HasValidPages(previousTopic))
                return;

            currentVideoIndex = previousTopic.pages.Length - 1;
        }
        else
        {
            return;
        }

        ShowPage();
    }

    public void SkipTopic()
    {
        if (isClosing || topics == null || topics.Length == 0)
            return;

        if (currentTopicIndex < topics.Length - 1)
        {
            currentTopicIndex++;
            currentVideoIndex = 0;
            ShowPage();
            return;
        }

        CloseTutorialScene();
    }

    private TutorialTopic GetCurrentTopic()
    {
        if (topics == null ||
            currentTopicIndex < 0 ||
            currentTopicIndex >= topics.Length)
        {
            return null;
        }

        return topics[currentTopicIndex];
    }

    private bool HasValidPages(TutorialTopic topic)
    {
        if (topic == null ||
            topic.pages == null ||
            topic.pages.Length == 0)
        {
            Debug.LogWarning(
                "[Tutorial] Current topic has no video pages.", this
            );

            return false;
        }

        return true;
    }

    private void ShowPage()
    {
        TutorialTopic topic = GetCurrentTopic();

        if (!HasValidPages(topic))
            return;

        if (currentVideoIndex < 0 ||
            currentVideoIndex >= topic.pages.Length)
        {
            Debug.LogWarning("[Tutorial] Invalid video page index.", this);
            return;
        }

        VideoPage page = topic.pages[currentVideoIndex];

        if (sectionTitle != null)
        {
            sectionTitle.sprite = topic.headerSprite;
            sectionTitle.enabled = topic.headerSprite != null;
        }

        if (descriptionText != null)
            descriptionText.text = topic.description;

        UpdateDots(topic.pages.Length);

        if (videoPlayer != null)
        {
            videoPlayer.Stop();

            if (page.clip != null)
            {
                videoPlayer.clip = page.clip;
                videoPlayer.Play();
            }
            else
            {
                Debug.LogWarning(
                    "[Tutorial] Missing video clip in topic: "
                    + topic.topicName
                    + ", page: " + (currentVideoIndex + 1),
                    this
                );
            }
        }
        else
        {
            Debug.LogWarning(
                "[Tutorial] Video Player is not assigned.", this
            );
        }
    }

    private void UpdateDots(int pageCount)
    {
        if (dots == null)
            return;

        for (int i = 0; i < dots.Length; i++)
        {
            if (dots[i] == null)
                continue;

            bool shouldShow = i < pageCount;
            dots[i].gameObject.SetActive(shouldShow);

            if (shouldShow)
            {
                dots[i].color = i == currentVideoIndex
                    ? activeColor
                    : inactiveColor;
            }
        }
    }

    private void CloseTutorialPopup()
    {
        if (tutorialPopup != null)
            tutorialPopup.SetActive(false);

        if (dimBackground != null)
            dimBackground.SetActive(false);

        Debug.Log("[Tutorial] Tutorial popup closed.");
    }

    private void CloseTutorialScene()
    {
        Debug.Log("[Tutorial] CloseTutorialScene called.");

        if (isClosing)
        {
            Debug.Log("[Tutorial] Already closing.");
            return;
        }

        isClosing = true;

        // Keep gameplay paused until the tutorial is unloaded.
        Time.timeScale = 0f;
        GameplayInputLock.Lock();

        StartCoroutine(UnloadTutorialScene());
    }

    private IEnumerator UnloadTutorialScene()
    {
        Debug.Log("[Tutorial] UnloadTutorialScene started.");

        Scene tutorialScene =
            SceneManager.GetSceneByName(tutorialSceneName);

        Scene mainScene =
            SceneManager.GetSceneByName(mainSceneName);

        if (!tutorialScene.IsValid() || !tutorialScene.isLoaded)
        {
            Debug.LogWarning("[Tutorial] Tutorial scene is not loaded.");
            ResumeGameplay();
            yield break;
        }

        if (!mainScene.IsValid() || !mainScene.isLoaded)
        {
            Debug.LogError("[Tutorial] Main scene is not loaded.");
            isClosing = false;
            yield break;
        }

        if (!SceneManager.SetActiveScene(mainScene))
        {
            Debug.LogError("[Tutorial] Failed to activate main scene.");
            isClosing = false;
            yield break;
        }

        Debug.Log("[Tutorial] Main scene activated.");

        AsyncOperation unloadOperation =
            SceneManager.UnloadSceneAsync(tutorialSceneName);

        if (unloadOperation == null)
        {
            Debug.LogError("[Tutorial] Unload operation is null.");
            isClosing = false;
            yield break;
        }

        unloadOperation.completed += operation =>
        {
            Debug.Log("[Tutorial] Unload completed. Resuming gameplay.");

            ResumeGameplay();
        };

        yield break;
    }

    private void ResumeGameplay()
    {
        Debug.Log("[Tutorial] ResumeGameplay called.");

        GameplayInputLock.Unlock();

        // Restore the time scale captured when this component awoke.
        Time.timeScale = previousTimeScale;

        isClosing = false;

        Debug.Log(
            "[Tutorial] TutorialVDO unloaded. Gameplay resumed. "
            + "Time scale: " + Time.timeScale
            + ", Input locked: " + GameplayInputLock.IsLocked
        );
    }
}
