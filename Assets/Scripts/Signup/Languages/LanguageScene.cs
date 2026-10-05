using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class LanguageScene : MonoBehaviour
{
    [System.Serializable]
    public class LangButton
    {
        public Button button;
        public Sprite normal;      
        public Sprite selected;    
    }
    public GameObject currentPage;
    public GameObject nextPage;
    public GameObject signup;

    [Header("lang btn")]
    [SerializeField] LangButton[] langButtons = new LangButton[3];

    [Header("other btn")]
    [SerializeField] Button enterButton;
    [SerializeField] Button closeButton;

    [Header("animation")]
    [SerializeField] RectTransform box;       
    [SerializeField] CanvasGroup boxGroup;   
    [SerializeField] CanvasGroup dimGroup;    


    [SerializeField] float delay = 0.25f;

    [Header("punch")]
    [SerializeField] float punchScale = 1.15f;
    [SerializeField] float punchTime = 0.18f;

    [Header("popup")]
    [SerializeField] float openTime = 0.22f;
    [SerializeField] float closeTime = 0.14f;
    [SerializeField] float startScale = 0.80f;
    [SerializeField] float overshoot = 1.04f;
    [SerializeField] float dimAlpha = 0.55f;

    [Header("default")]
    [SerializeField] int defaultIndex = 1;     

    int selected = -1;
    bool busy = false;

    void OnEnable()
    {
        for (int i = 0; i < langButtons.Length; i++)
        {
            int index = i;
            LangButton lb = langButtons[i];
            if (lb == null || lb.button == null) continue;

            lb.button.transition = Selectable.Transition.None;
            lb.button.onClick.AddListener(() =>
            {
                Select(index);
                StartCoroutine(Punch(langButtons[index].button.transform));
            });
        }

        if (enterButton != null) enterButton.onClick.AddListener(OnEnter);
        if (closeButton != null) closeButton.onClick.AddListener(OnClose);

        Select(defaultIndex);
        StartCoroutine(OpenAnim());
    }

    void Select(int index)
    {
        selected = index;

        for (int i = 0; i < langButtons.Length; i++)
        {
            LangButton lb = langButtons[i];
            if (lb == null || lb.button == null) continue;

            Image img = lb.button.GetComponent<Image>();
            if (img == null) continue;

            Sprite s = (i == index) ? lb.selected : lb.normal;
            if (s != null) img.sprite = s;
        }

        if (enterButton != null) enterButton.interactable = (selected >= 0);
    }

    void OnEnter()
    {
        if (busy || selected < 0) return;

        busy = true;
        if (enterButton != null) StartCoroutine(Punch(enterButton.transform));
        StartCoroutine(GoNext());
    }

    void OnClose()
    {
        if (busy) return;

        busy = true;
        if (closeButton != null) StartCoroutine(Punch(closeButton.transform));
        StartCoroutine(CloseAndUnload());
    }

    IEnumerator GoNext()
    {
        yield return new WaitForSecondsRealtime(delay);

        SignupUI ui = FindFirstObjectByType<SignupUI>();
        if (ui != null) ui.Unlock();

        busy = false;
        nextPage.SetActive(true);
        signup.SetActive(false);
        currentPage.SetActive(false);
    }

    IEnumerator CloseAndUnload()
    {
        yield return CloseAnim();

        SignupUI signup = FindFirstObjectByType<SignupUI>();
        if (signup != null) signup.Unlock();

        busy = false;
        currentPage.SetActive(false);
    }


    IEnumerator OpenAnim()
    {
        float e = 0f;

        if (box != null) box.localScale = Vector3.one * startScale;
        if (boxGroup != null) boxGroup.alpha = 0f;
        if (dimGroup != null) dimGroup.alpha = 0f;

        float p1 = openTime * 0.65f;
        while (e < p1)
        {
            e += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(e / p1);
            float ease = 1f - Mathf.Pow(1f - p, 3f);

            if (box != null) box.localScale = Vector3.one * Mathf.Lerp(startScale, overshoot, ease);
            if (boxGroup != null) boxGroup.alpha = ease;
            if (dimGroup != null) dimGroup.alpha = ease * dimAlpha;
            yield return null;
        }

        e = 0f;
        float p2 = openTime * 0.35f;
        while (e < p2)
        {
            e += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(e / p2);
            float ease = 1f - Mathf.Pow(1f - p, 3f);

            if (box != null) box.localScale = Vector3.one * Mathf.Lerp(overshoot, 1f, ease);
            yield return null;
        }

        if (box != null) box.localScale = Vector3.one;
        if (boxGroup != null) boxGroup.alpha = 1f;
        if (dimGroup != null) dimGroup.alpha = dimAlpha;
    }

    IEnumerator CloseAnim()
    {
        float e = 0f;
        float from = box != null ? box.localScale.x : 1f;

        while (e < closeTime)
        {
            e += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(e / closeTime);

            if (box != null) box.localScale = Vector3.one * Mathf.Lerp(from, startScale, p);
            if (boxGroup != null) boxGroup.alpha = 1f - p;
            if (dimGroup != null) dimGroup.alpha = dimAlpha * (1f - p);
            yield return null;
        }
    }

    IEnumerator Punch(Transform t)
    {
        Vector3 a = Vector3.one;
        Vector3 b = a * punchScale;
        yield return ScaleTo(t, a, b, punchTime * 0.4f);
        yield return ScaleTo(t, b, a, punchTime * 0.6f);
        t.localScale = a;
    }

    IEnumerator ScaleTo(Transform t, Vector3 from, Vector3 to, float time)
    {
        float e = 0f;
        while (e < time)
        {
            e += Time.unscaledDeltaTime;
            float p = e / time;
            p = 1f - Mathf.Pow(1f - p, 3f);
            t.localScale = Vector3.Lerp(from, to, p);
            yield return null;
        }
        t.localScale = to;
    }
}