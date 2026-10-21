
using UnityEngine;
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
    //[SerializeField] private TMP_Text stepTitle;
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

    private int currentTopicIndex = 0;
    private int currentVideoIndex = 0;

    private void Start()
    {
        if (topics == null || topics.Length == 0)
        {
            Debug.LogError("TutorialVideoCarousel: No topics assigned.", this);
            return;
        }

        ShowPage();
    }

    public void NextPage()
    {
        TutorialTopic topic = GetCurrentTopic();

        if (!HasValidPages(topic))
            return;

        if (currentVideoIndex < topic.pages.Length - 1)
        {
            currentVideoIndex++;
            ShowPage();
        }
        else if (currentTopicIndex < topics.Length - 1)
        {
            currentTopicIndex++;
            currentVideoIndex = 0;
            ShowPage();
        }
        else
        {
            // Final video of the final topic.
            CloseTutorialPopup();
        }
    }

    public void PreviousPage()
    {
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
        if (topics == null || topics.Length == 0)
            return;

        if (currentTopicIndex < topics.Length - 1)
        {
            // Jump to the next topic.
            currentTopicIndex++;
            currentVideoIndex = 0;
            ShowPage();
        }
        else
        {
            // No more topics: close the tutorial popup.
            CloseTutorialPopup();
        }
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
                "TutorialVideoCarousel: Current topic has no video pages.",
                this);

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
            return;

        VideoPage page = topic.pages[currentVideoIndex];

        // Change the topic header image.
        if (sectionTitle != null)
        {
            sectionTitle.sprite = topic.headerSprite;
            sectionTitle.enabled = topic.headerSprite != null;
        }

        // Change the title for each video.
        //if (stepTitle != null)
        //    stepTitle.text = page.title;

        // Change the description only when the topic changes.
        if (descriptionText != null)
            descriptionText.text = topic.description;

        // Update the page dots.
        UpdateDots(topic.pages.Length);

        // Play the selected video.
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
                    $"Missing video clip: {topic.topicName}, " +
                    $"page {currentVideoIndex + 1}.",
                    this);
            }
        }
        else
        {
            Debug.LogWarning("Video Player is not assigned.", this);
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

        Debug.Log("Tutorial carousel completed.");
    }
}
