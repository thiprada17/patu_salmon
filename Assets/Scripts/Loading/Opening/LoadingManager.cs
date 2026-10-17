
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class LoadingManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] Slider loadingSlider;
    [SerializeField] TextMeshProUGUI loadingText;

    [Header("Session")]
    [SerializeField] ResourceLoading resourceLoading;
    [SerializeField] private string loginSceneName = "signup";
    [SerializeField] private string menuSceneName = "loading";

    [Header("Behavior")]
    [SerializeField] float loadSpeed = 0.6f;
    [SerializeField] float tipChangeInterval = 1.5f;

    [Header("Loading Messages")]
    [TextArea]
    [SerializeField]
    string[] tips =
    {
        "Preparing to land...",
        "Beware of what to come...",
        "Don't overthink..."
    };

    int lastTipIndex = -1;
    bool isLoadingDone;

    void Start()
    {
        Application.targetFrameRate = 60;

        if (loadingSlider != null)
            loadingSlider.value = 0f;

        if (resourceLoading == null)
            resourceLoading = FindFirstObjectByType<ResourceLoading>();

        StartCoroutine(FakeLoading());
        StartCoroutine(CycleTips());
    }

    IEnumerator FakeLoading()
    {
        while (loadingSlider != null && loadingSlider.value < 1f)
        {
            loadingSlider.value = Mathf.Clamp01(
                loadingSlider.value + loadSpeed * Time.deltaTime);

            yield return null;
        }

        // รอให้ตรวจ Session เสร็จด้วย
        while (resourceLoading != null &&
               !resourceLoading.IsCheckDone)
        {
            yield return null;
        }

        isLoadingDone = true;

        if (resourceLoading.IsSessionValid)
        {
            SceneManager.LoadScene("loading");
        }
        else
        {
            SceneManager.LoadScene("signup");
        }
    }

    IEnumerator CycleTips()
    {
        SetRandomTip();

        while (!isLoadingDone)
        {
            yield return new WaitForSeconds(tipChangeInterval);

            if (!isLoadingDone)
                SetRandomTip();
        }
    }

    void SetRandomTip()
    {
        if (tips == null || tips.Length == 0)
            return;

        int index = Random.Range(0, tips.Length);

        if (index == lastTipIndex && tips.Length > 1)
            index = (index + 1) % tips.Length;

        lastTipIndex = index;

        if (loadingText != null)
            loadingText.text = tips[index];
    }
}
