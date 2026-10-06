using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LogoutPopup : MonoBehaviour
{
    [Header("Btn")]
    [SerializeField] Button cancelButton;
    [SerializeField] Button confirmButton;

    [Header("Scene")]
    [SerializeField] string loginScene = "MainMenu";  

    [Header("Pop")]
    [SerializeField] Transform panel;                  
    [SerializeField] float popTime = 0.2f;
    [SerializeField] float startScale = 0.85f;

    [Header("Punch")]
    [SerializeField] float punchScale = 1.12f;
    [SerializeField] float punchTime = 0.16f;

    bool isBusy;
    public GameObject popupRoot;
    Vector3 panelScale;
    bool scaleSaved;

    void Start()
    {
        if (cancelButton != null) cancelButton.onClick.AddListener(OnCancel);
        if (confirmButton != null) confirmButton.onClick.AddListener(OnConfirm);
    }

    void OnEnable()
    {
        isBusy = false;
        if (panel != null)
        {
            if (!scaleSaved) { panelScale = panel.localScale; scaleSaved = true; }
            StartCoroutine(PopIn());
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) OnCancel();
    }

    void OnCancel()
    {
        if (isBusy) return;
        isBusy = true;
        StartCoroutine(PunchThen(cancelButton, Close));
    }

    void OnConfirm()
    {
        if (isBusy) return;
        isBusy = true;
        StartCoroutine(PunchThen(confirmButton, () =>
        {
            PlayerPrefs.DeleteKey("Username");
            PlayerPrefs.Save();

            SceneManager.LoadScene(loginScene);  
        }));
    }

    void Close()
    {
        if (popupRoot != null) popupRoot.SetActive(false);
        else gameObject.SetActive(false);
    }

    IEnumerator PopIn()
    {
        Vector3 to = panelScale;
        Vector3 from = to * startScale;
        panel.localScale = from;

        float e = 0f;
        while (e < popTime)
        {
            e += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(e / popTime);
            k = 1f - Mathf.Pow(1f - k, 3f);
            panel.localScale = Vector3.LerpUnclamped(from, to, k);
            yield return null;
        }
        panel.localScale = to;
    }

    IEnumerator PunchThen(Button btn, System.Action done)
    {
        if (btn != null)
        {
            Transform t = btn.transform;
            Vector3 baseScale = t.localScale;
            Vector3 big = baseScale * punchScale;

            yield return Scale(t, baseScale, big, punchTime * 0.4f);
            yield return Scale(t, big, baseScale, punchTime * 0.6f);
        }
        done?.Invoke();
    }

    IEnumerator Scale(Transform t, Vector3 from, Vector3 to, float time)
    {
        float e = 0f;
        while (e < time)
        {
            e += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(e / time);
            k = 1f - Mathf.Pow(1f - k, 3f);
            t.localScale = Vector3.LerpUnclamped(from, to, k);
            yield return null;
        }
        t.localScale = to;
    }
}