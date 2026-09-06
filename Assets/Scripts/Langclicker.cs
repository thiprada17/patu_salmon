using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Collections;

// ตรวจการคลิกเอง ไม่พึ่ง Button / GraphicRaycaster
public class LangClicker : MonoBehaviour
{
    [System.Serializable]
    public class LangItem
    {
        public RectTransform rect;   // ตัวปุ่ม
        public Image image;          // Image ของปุ่ม
        public Sprite normal;
        public Sprite selected;
    }

    [Header("ปุ่มภาษา 0=ไทย 1=อังกฤษ 2=ญี่ปุ่น")]
    [SerializeField] LangItem[] items = new LangItem[3];

    [Header("ปุ่มอื่น")]
    [SerializeField] RectTransform enterRect;
    [SerializeField] RectTransform closeRect;

    [Header("popup")]
    [SerializeField] GameObject root;
    [SerializeField] Transform box;
    [SerializeField] CanvasGroup boxGroup;
    [SerializeField] CanvasGroup dimGroup;

    [Header("อ้างอิง")]
    [SerializeField] Canvas canvas;          // Canvas หลัก
    [SerializeField] SignupUI signupUI;

    [Header("next")]
    [SerializeField] string nextScene = "SetCharacter";
    [SerializeField] float delay = 0.25f;

    [Header("anim")]
    [SerializeField] float punchScale = 1.15f;
    [SerializeField] float punchTime = 0.18f;
    [SerializeField] float openTime = 0.22f;
    [SerializeField] float closeTime = 0.14f;
    [SerializeField] float startScale = 0.80f;
    [SerializeField] float overshoot = 1.04f;
    [SerializeField] float dimAlpha = 0.55f;

    [Header("default")]
    [SerializeField] int defaultIndex = 1;   // -1 = ยังไม่เลือก

    int selected = -1;
    bool busy = false;
    bool open = false;

    void Update()
    {
        if (!open || busy) return;
        if (!ClickedThisFrame()) return;

        Vector2 pos = PointerPos();

        // ปุ่มภาษา
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == null || items[i].rect == null) continue;
            if (Inside(items[i].rect, pos))
            {
                Select(i);
                StartCoroutine(Punch(items[i].rect));
                return;
            }
        }

        // กากบาท
        if (closeRect != null && Inside(closeRect, pos))
        {
            busy = true;
            StartCoroutine(Punch(closeRect));
            StartCoroutine(CloseAnim());
            return;
        }

        // Enter
        if (enterRect != null && Inside(enterRect, pos))
        {
            if (selected < 0) return;
            busy = true;
            StartCoroutine(Punch(enterRect));
            StartCoroutine(GoNext());
            return;
        }
    }

    // ---------- ตรวจคลิก ----------

    bool ClickedThisFrame()
    {
#if ENABLE_INPUT_SYSTEM
        var m = UnityEngine.InputSystem.Mouse.current;
        if (m != null && m.leftButton.wasPressedThisFrame) return true;

        var t = UnityEngine.InputSystem.Touchscreen.current;
        if (t != null && t.primaryTouch.press.wasPressedThisFrame) return true;

        return false;
#else
        return Input.GetMouseButtonDown(0);
#endif
    }

    Vector2 PointerPos()
    {
#if ENABLE_INPUT_SYSTEM
        var m = UnityEngine.InputSystem.Mouse.current;
        if (m != null) return m.position.ReadValue();

        var t = UnityEngine.InputSystem.Touchscreen.current;
        if (t != null) return t.primaryTouch.position.ReadValue();

        return Vector2.zero;
#else
        return Input.mousePosition;
#endif
    }

    bool Inside(RectTransform rt, Vector2 screenPos)
    {
        Camera cam = null;
        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            cam = canvas.worldCamera;

        return RectTransformUtility.RectangleContainsScreenPoint(rt, screenPos, cam);
    }

    // ---------- เปิด / ปิด ----------

    public void Open()
    {
        if (root == null) return;

        root.SetActive(true);
        StopAllCoroutines();

        busy = false;
        open = true;

        Select(defaultIndex);
        StartCoroutine(OpenAnim());
    }

    void Select(int index)
    {
        selected = index;

        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == null || items[i].image == null) continue;

            Sprite s = (i == index) ? items[i].selected : items[i].normal;
            if (s != null) items[i].image.sprite = s;
        }
    }

    // ---------- แอนิเมชัน ----------

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
        open = false;
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