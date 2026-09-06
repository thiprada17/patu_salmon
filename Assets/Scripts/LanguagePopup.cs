using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class LanguagePopup : MonoBehaviour
{
    [System.Serializable]
    public class LangButton
    {
        public Button button;
        public Sprite normal;      // รูปปุ่มปกติ (ฟ้าอ่อน)
        public Sprite selected;    // รูปปุ่มตอนถูกเลือก (ฟ้าเข้ม)
    }

    [Header("popup")]
    [SerializeField] GameObject root;          // language_popup
    [SerializeField] Transform box;            // popup_box
    [SerializeField] CanvasGroup boxGroup;     // CanvasGroup ที่ popup_box
    [SerializeField] CanvasGroup dimGroup;     // CanvasGroup ที่ dim

    [Header("lang (0=ไทย 1=อังกฤษ 2=ญี่ปุ่น)")]
    [SerializeField] LangButton[] langButtons = new LangButton[3];

    [Header("btn")]
    [SerializeField] Button enterButton;
    [SerializeField] Button closeButton;       // กากบาท

    [Header("signup (ไว้ปลดล็อกตอนกดกากบาท)")]
    [SerializeField] SignupUI signupUI;

    [Header("next")]
    [SerializeField] string nextScene = "SetCharacter";
    [SerializeField] float delay = 0.25f;

    [Header("punch")]
    [SerializeField] float punchScale = 1.15f;
    [SerializeField] float punchTime = 0.18f;

    [Header("popup anim")]
    [SerializeField] float openTime = 0.22f;
    [SerializeField] float closeTime = 0.14f;
    [SerializeField] float startScale = 0.80f;
    [SerializeField] float overshoot = 1.04f;
    [SerializeField] float dimAlpha = 0.55f;

    [Header("default")]
    [SerializeField] int defaultIndex = -1;     // -1 = ยังไม่เลือก

    int selected = -1;
    bool busy = false;
    bool ready = false;

    void Setup()
    {
        if (ready) return;
        ready = true;

        for (int i = 0; i < langButtons.Length; i++)
        {
            int index = i;   // ก็อปตัวแปรก่อน ไม่งั้นทุกปุ่มจะชี้ตัวสุดท้าย
            LangButton lb = langButtons[i];
            if (lb == null || lb.button == null) continue;

            lb.button.transition = Selectable.Transition.None;  // กัน Color Tint ทับสี
            lb.button.onClick.AddListener(() =>
            {
                Debug.Log("กดปุ่มที่ " + index);
                Select(index);
                StartCoroutine(Punch(langButtons[index].button.transform));
            });
        }

        if (enterButton != null) enterButton.onClick.AddListener(OnEnterClick);
        if (closeButton != null) closeButton.onClick.AddListener(OnCloseClick);
    }

    // ---------- เปิด / ปิด ----------

    public void Open()
    {
        if (root == null) return;

        root.SetActive(true);
        Setup();

        StopAllCoroutines();
        busy = false;

        Select(defaultIndex);
        StartCoroutine(OpenAnim());
    }

    void OnCloseClick()
    {
        if (busy) return;

        busy = true;
        StartCoroutine(CloseAnim());
    }

    void OnEnterClick()
    {
        if (busy || selected < 0) return;

        busy = true;
        if (enterButton != null) StartCoroutine(Punch(enterButton.transform));
        StartCoroutine(GoNext());
    }

    // ---------- เลือกภาษา ----------

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

    // ---------- แอนิเมชัน ----------

    IEnumerator OpenAnim()
    {
        float e = 0f;

        if (box != null) box.localScale = Vector3.one * startScale;
        if (boxGroup != null) boxGroup.alpha = 0f;
        if (dimGroup != null) dimGroup.alpha = 0f;

        float phase1 = openTime * 0.65f;
        while (e < phase1)
        {
            e += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(e / phase1);
            float ease = 1f - Mathf.Pow(1f - p, 3f);

            if (box != null) box.localScale = Vector3.one * Mathf.Lerp(startScale, overshoot, ease);
            if (boxGroup != null) boxGroup.alpha = ease;
            if (dimGroup != null) dimGroup.alpha = ease * dimAlpha;
            yield return null;
        }

        e = 0f;
        float phase2 = openTime * 0.35f;
        while (e < phase2)
        {
            e += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(e / phase2);
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

        if (root != null) root.SetActive(false);
        if (signupUI != null) signupUI.Unlock();
        busy = false;
    }

    IEnumerator GoNext()
    {
        yield return new WaitForSecondsRealtime(delay);
        SceneManager.LoadScene(nextScene);
    }

    IEnumerator Punch(Transform t)
    {
        Vector3 baseScale = Vector3.one;
        Vector3 bigScale = baseScale * punchScale;
        yield return ScaleTo(t, baseScale, bigScale, punchTime * 0.4f);
        yield return ScaleTo(t, bigScale, baseScale, punchTime * 0.6f);
        t.localScale = baseScale;
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