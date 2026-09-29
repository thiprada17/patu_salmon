using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CanvasGroup))]
public class PopupRecipe : MonoBehaviour
{
    [Header("element")]
    [SerializeField] Transform box;          
    [SerializeField] Button openButton;  
    [SerializeField] Button closeButton;     
    [SerializeField] Button backdropButton;  

    [Header("open")]
    [SerializeField] float openTime = 0.25f;
    [SerializeField] float fromScale = 0.7f;
    [SerializeField] float overshoot = 1.06f;  

    [Header("close")]
    [SerializeField] float closeTime = 0.15f;
    [SerializeField] float toScale = 0.85f;

    [Header("other")]
    [SerializeField] bool openOnStart = false;
    [SerializeField] bool unloadSceneOnClose = false;  
    [SerializeField] bool closeWithEscape = true;

    CanvasGroup cg;
    Coroutine running;
    bool isOpen;

    public bool IsOpen => isOpen;

    void Awake()
    {
        cg = GetComponent<CanvasGroup>();
        if (box == null) box = transform;

        if (openButton != null) openButton.onClick.AddListener(Open);
        if (closeButton != null) closeButton.onClick.AddListener(Close);
        if (backdropButton != null) backdropButton.onClick.AddListener(Close);

        if (openOnStart) Open();
        else SetClosedInstant();
    }

    void Update()
    {
        if (closeWithEscape && isOpen && Input.GetKeyDown(KeyCode.Escape)) Close();
    }

    public void Open()
    {
        if (isOpen) return;
        isOpen = true;

        gameObject.SetActive(true);
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(OpenRoutine());
    }

    public void Close()
    {
        if (!isOpen) return;
        isOpen = false;

        if (running != null) StopCoroutine(running);
        running = StartCoroutine(CloseRoutine());
    }

    public void Toggle()
    {
        if (isOpen) Close();
        else Open();
    }

    IEnumerator OpenRoutine()
    {
        cg.blocksRaycasts = true;
        cg.interactable = true;

        Vector3 full = Vector3.one;
        Vector3 small = full * fromScale;
        Vector3 big = full * overshoot;

        float t1 = openTime * 0.7f;
        float e = 0f;
        while (e < t1)
        {
            e += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(e / t1);
            float eased = 1f - Mathf.Pow(1f - k, 3f);
            box.localScale = Vector3.LerpUnclamped(small, big, eased);
            cg.alpha = eased;
            yield return null;
        }

        float t2 = openTime * 0.3f;
        e = 0f;
        while (e < t2)
        {
            e += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(e / t2);
            box.localScale = Vector3.LerpUnclamped(big, full, k);
            yield return null;
        }

        box.localScale = full;
        cg.alpha = 1f;
        running = null;
    }

    IEnumerator CloseRoutine()
    {
        cg.blocksRaycasts = false;
        cg.interactable = false;

        Vector3 from = box.localScale;
        Vector3 to = Vector3.one * toScale;

        float e = 0f;
        while (e < closeTime)
        {
            e += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(e / closeTime);
            box.localScale = Vector3.LerpUnclamped(from, to, k);
            cg.alpha = 1f - k;
            yield return null;
        }

        SetClosedInstant();
        running = null;

        if (unloadSceneOnClose)
        {
            Scene self = gameObject.scene;
            if (SceneManager.sceneCount > 1 && self.isLoaded)
                SceneManager.UnloadSceneAsync(self);
        }
    }

    void SetClosedInstant()
    {
        cg.alpha = 0f;
        cg.blocksRaycasts = false;
        cg.interactable = false;
        box.localScale = Vector3.one * toScale;
        gameObject.SetActive(false);
    }
}