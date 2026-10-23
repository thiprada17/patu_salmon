using UnityEngine;
using UnityEngine.UI;
using System.Collections;
//using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LanguagePopup : MonoBehaviour
{
    [SerializeField] private GameObject popup;
    //[SerializeField] private Button thaiButton;
    [SerializeField] private Button englishButton;

    [Header("Animation")]
    [SerializeField] private Transform popupBox;
    [SerializeField] private CanvasGroup popupGroup;
    [SerializeField] private CanvasGroup dimGroup;

    [SerializeField] private float openTime = 0.22f;
    [SerializeField] private float closeTime = 0.14f;
    [SerializeField] private float startScale = 0.8f;
    [SerializeField] private float overshoot = 1.04f;
    [SerializeField] private float dimAlpha = 0.55f;

    [Header("Button Punch")]
    [SerializeField] private float punchScale = 1.15f;
    [SerializeField] private float punchTime = 0.18f;

    [Header("Selected Button Color")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = Color.yellow;
    private void Awake()
    {
        popup.SetActive(false);

        //thaiButton.onClick.AddListener(() =>
        //{
        //    StartCoroutine(Punch(thaiButton.transform));
        //    SelectLanguage("thai");
        //});

        englishButton.onClick.AddListener(() =>
        {
            StartCoroutine(Punch(englishButton.transform));
            SelectLanguage("english");
        });
    }

    public void Open()
    {
        Debug.Log("OPEN POPUP");

        if (popup == null)
        {
            Debug.LogError("popup เป็น NULL");
            return;
        }

        popup.SetActive(true);

        StopAllCoroutines();
        StartCoroutine(OpenAnimation());

    }

    public void Close()
    {
        if (popup == null) return;

        StopAllCoroutines();
        StartCoroutine(CloseAnimation());
    }

    private IEnumerator OpenAnimation()
    {
        float time = 0f;

        if (popupBox != null)
            popupBox.localScale = Vector3.one * startScale;

        if (popupGroup != null)
            popupGroup.alpha = 0f;

        if (dimGroup != null)
            dimGroup.alpha = 0f;

        while (time < openTime)
        {
            time += Time.unscaledDeltaTime;

            float p = Mathf.Clamp01(time / openTime);

            // Ease Out
            float ease = 1f - Mathf.Pow(1f - p, 3f);

            if (popupBox != null)
            {
                float scale = Mathf.Lerp(
                    startScale,
                    overshoot,
                    ease
                );

                popupBox.localScale = Vector3.one * scale;
            }

            if (popupGroup != null)
                popupGroup.alpha = ease;

            if (dimGroup != null)
                dimGroup.alpha = ease * dimAlpha;

            yield return null;
        }

        // เด้งกลับมาที่ 1
        time = 0f;

        while (time < 0.08f)
        {
            time += Time.unscaledDeltaTime;

            float p = Mathf.Clamp01(time / 0.08f);

            if (popupBox != null)
            {
                float scale = Mathf.Lerp(
                    overshoot,
                    1f,
                    p
                );

                popupBox.localScale = Vector3.one * scale;
            }

            yield return null;
        }

        if (popupBox != null)
            popupBox.localScale = Vector3.one;

        if (popupGroup != null)
            popupGroup.alpha = 1f;

        if (dimGroup != null)
            dimGroup.alpha = dimAlpha;
    }

    private IEnumerator CloseAnimation()
    {
        float time = 0f;

        Vector3 start =
            popupBox != null
            ? popupBox.localScale
            : Vector3.one;

        while (time < closeTime)
        {
            time += Time.unscaledDeltaTime;

            float p = Mathf.Clamp01(time / closeTime);

            if (popupBox != null)
            {
                popupBox.localScale =
                    Vector3.Lerp(
                        start,
                        Vector3.one * startScale,
                        p
                    );
            }

            if (popupGroup != null)
                popupGroup.alpha = 1f - p;

            if (dimGroup != null)
                dimGroup.alpha = dimAlpha * (1f - p);

            yield return null;
        }

        popup.SetActive(false);
    }

    private string selectedLanguage;

    private void SelectLanguage(string language)
    {
        selectedLanguage = language;

        PlayerPrefs.SetString("Language", language);
        PlayerPrefs.Save();

        //SetButtonColor(thaiButton, language == "thai");
        SetButtonColor(englishButton, language == "english");

        Debug.Log("Selected language: " + language);
    }

    private void SetButtonColor(Button button, bool selected)
    {
        if (button == null) return;

        Image image = button.targetGraphic as Image;

        if (image != null)
        {
            image.color = selected ? selectedColor : normalColor;
        }
    }

    private void InitializeNewPlayerTutorial()
    {
        TutorialSession.Initialize(0);

        Debug.Log(
            "[LanguagePopup] New player tutorial initialized. " +
            "FirstTimePlayer=" + TutorialSession.IsFirstTimePlayer
        );
    }


    public void OnEnterClick()
    {
        if (string.IsNullOrEmpty(selectedLanguage))
        {
            Debug.Log("Please select language first");
            return;
        }

        // Save the new player's tutorial state before changing scenes.
        InitializeNewPlayerTutorial();

        Close();
        SceneManager.LoadScene("SelectCharacter");
    }


    private IEnumerator Punch(Transform target)
    {
        Vector3 normal = target.localScale;
        Vector3 big = normal * punchScale;

        yield return Scale(
            target,
            normal,
            big,
            punchTime * 0.4f
        );

        yield return Scale(
            target,
            big,
            normal,
            punchTime * 0.6f
        );

        target.localScale = normal;
    }

    private IEnumerator Scale(
        Transform target,
        Vector3 from,
        Vector3 to,
        float duration)
    {
        float time = 0f;

        while (time < duration)
        {
            time += Time.unscaledDeltaTime;

            float p = Mathf.Clamp01(time / duration);

            p = 1f - Mathf.Pow(1f - p, 3f);

            target.localScale =
                Vector3.Lerp(from, to, p);

            yield return null;
        }

        target.localScale = to;
    }
}