using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Dedicated loading screen for travel between the map and gameplay.</summary>
public sealed class MapGameLoading : MonoBehaviour
{
    private const string LoadingSceneName = "MapGameLoading";
    private static string destinationScene;
    private static bool requestPending;

    [Header("Comic bubbles, in display order")]
    [SerializeField] private RectTransform[] textBubbles = new RectTransform[3];
    [SerializeField, Min(0.05f)] private float popDuration = 0.32f;
    [SerializeField, Min(0f)] private float bubbleStagger = 0.18f;
    [Header("Loading Text")]
    [SerializeField] private TMP_Text loadingText;
    [SerializeField, Min(0f)] private float textHopHeight = 10f;
    [SerializeField, Min(0.01f)] private float textHopDuration = 0.25f;
    [SerializeField, Min(0f)] private float textHopGap = 0.045f;
    [SerializeField, Min(0f)] private float textHopRest = 0.4f;

    public static void Load(string sceneName)
    {
        if (requestPending || (sceneName != "map" && sceneName != "main_game"))
            return;

        destinationScene = sceneName;
        requestPending = true;
        Time.timeScale = 1f;
        SceneManager.LoadScene(LoadingSceneName);
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        SetDestinationText();
        if (loadingText != null)
            StartCoroutine(HopLoadingText());

        StartCoroutine(LoadDestination());
    }

    private void SetDestinationText()
    {
        if (loadingText == null)
            return;

        loadingText.text = destinationScene == "map"
            ? "Flying to Town Map..."
            : "Flying to Work place...";
    }

    private IEnumerator HopLoadingText()
    {
        while (loadingText != null)
        {
            loadingText.ForceMeshUpdate();
            int characterCount = loadingText.textInfo.characterCount;
            float cycleDuration = textHopGap * characterCount + textHopDuration;
            float elapsed = 0f;

            while (elapsed < cycleDuration && loadingText != null)
            {
                elapsed += Time.unscaledDeltaTime;
                loadingText.ForceMeshUpdate();
                TMP_TextInfo info = loadingText.textInfo;

                for (int i = 0; i < info.characterCount; i++)
                {
                    if (!info.characterInfo[i].isVisible)
                        continue;

                    float progress = (elapsed - i * textHopGap) / textHopDuration;
                    float offset = progress > 0f && progress < 1f
                        ? Mathf.Sin(progress * Mathf.PI) * textHopHeight
                        : 0f;

                    int materialIndex = info.characterInfo[i].materialReferenceIndex;
                    int vertexIndex = info.characterInfo[i].vertexIndex;
                    Vector3[] vertices = info.meshInfo[materialIndex].vertices;
                    for (int vertex = 0; vertex < 4; vertex++)
                        vertices[vertexIndex + vertex] += Vector3.up * offset;
                }

                for (int i = 0; i < info.meshInfo.Length; i++)
                {
                    info.meshInfo[i].mesh.vertices = info.meshInfo[i].vertices;
                    loadingText.UpdateGeometry(info.meshInfo[i].mesh, i);
                }

                yield return null;
            }

            if (textHopRest > 0f)
                yield return new WaitForSecondsRealtime(textHopRest);
        }
    }

    private IEnumerator LoadDestination()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(destinationScene);
        if (operation == null)
        {
            Debug.LogError("Could not load destination scene: " + destinationScene);
            requestPending = false;
            yield break;
        }

        operation.allowSceneActivation = false;
        yield return StartCoroutine(PopTextBubbles());

        while (operation.progress < 0.9f)
            yield return null;

        yield return null;
        operation.allowSceneActivation = true;
        while (!operation.isDone)
            yield return null;

        destinationScene = null;
        requestPending = false;
        Destroy(gameObject);
    }

    private IEnumerator PopTextBubbles()
    {
        if (textBubbles == null || textBubbles.Length == 0)
        {
            Debug.LogWarning("No text bubbles are assigned in the MapGameLoading Inspector.");
            yield break;
        }

        var finalScales = new Vector3[textBubbles.Length];
        for (int i = 0; i < textBubbles.Length; i++)
        {
            if (textBubbles[i] == null)
                continue;

            finalScales[i] = textBubbles[i].localScale;
            textBubbles[i].localScale = Vector3.zero;
        }

        bool hasPlayedBubble = false;
        for (int i = 0; i < textBubbles.Length; i++)
        {
            RectTransform bubble = textBubbles[i];
            if (bubble == null)
                continue;

            if (hasPlayedBubble && bubbleStagger > 0f)
                yield return new WaitForSecondsRealtime(bubbleStagger);

            hasPlayedBubble = true;
            float elapsed = 0f;
            while (elapsed < popDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / popDuration);
                float pop = 1f + 2.70158f * Mathf.Pow(t - 1f, 3f) +
                            1.70158f * Mathf.Pow(t - 1f, 2f);
                bubble.localScale = Vector3.LerpUnclamped(Vector3.zero, finalScales[i], pop);
                yield return null;
            }

            bubble.localScale = finalScales[i];
        }
    }

}
