using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class CardSelect : MonoBehaviour
{
    [SerializeField] GameObject glow;
    [SerializeField] string characterName;
    [SerializeField] string nextScene = "idcard";
    [SerializeField] float delay = 0.2f;

    [Header("punch")]
    [SerializeField] Transform card;
    [SerializeField] float punchScale = 0.9f;
    [SerializeField] float punchTime = 0.12f;

    [Header("animation center")]
    [SerializeField] Vector3 centerPos = Vector3.zero;
    [SerializeField] bool useScreenCenter = true;
    [SerializeField] Vector2 centerOffset = Vector2.zero;
    [SerializeField] Transform cardVisual;
    [SerializeField] float moveTime = 0.6f;
    [SerializeField] float zoomScale = 1.25f;

    [Header("animation in card")]
    [SerializeField] Transform selectButton;
    [SerializeField] float hideButtonTime = 0.2f;
    [SerializeField] Transform contentRoot;
    [SerializeField] Vector2 contentOffset = Vector2.zero;

    [Header("fade")]
    [SerializeField] Transform[] otherStuff;
    [SerializeField] float fadeOthers = 0.35f;
    [SerializeField] float glowFadeTime = 0.25f;

    [Header("cut")]
    [SerializeField] float cutAtPercent = 0.85f;

    static bool locked = false;

    void Start()
    {
        locked = false;
        if (glow != null) glow.SetActive(false);
    }

    void Update()
    {
        if (locked) return;

        // คลิกด้วยเมาส์
        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            TrySelect(Mouse.current.position.ReadValue());
        }

        // แตะหน้าจอมือถือ
        if (Touchscreen.current != null &&
            Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            TrySelect(
                Touchscreen.current.primaryTouch.position.ReadValue()
            );
        }
    }

    void TrySelect(Vector2 screenPosition)
    {
        // ไม่เลือกการ์ดเมื่อแตะ UI
        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
            return;

        Camera cam = Camera.main;
        if (cam == null) return;

        Vector2 worldPosition = cam.ScreenToWorldPoint(screenPosition);

        Collider2D hit = Physics2D.OverlapPoint(worldPosition);

        if (hit == null || !hit.transform.IsChildOf(transform) &&
            hit.transform != transform)
            return;

        if (locked) return;
        locked = true;

        if (glow != null) glow.SetActive(true);

        PlayerPrefs.SetString("SelectedCharacter", characterName);
        PlayerPrefs.Save();

        GameLogger.Info("CardSelect", "Selected character: " + characterName);

        StartCoroutine(Punch(transform));
        StartCoroutine(GoNext());
    }

    IEnumerator GoNext()
    {
        yield return new WaitForSeconds(delay);

        if (otherStuff != null)
        {
            foreach (var t in otherStuff)
                if (t != null) StartCoroutine(FadeGroup(t, fadeOthers));
        }

        if (glow != null)
            StartCoroutine(FadeGroup(glow.transform, glowFadeTime));

        if (selectButton != null)
            StartCoroutine(FadeGroup(selectButton, hideButtonTime));

        StartCoroutine(ArrangeContent());

        if (card == null)
        {
            SceneManager.LoadScene(nextScene);
            yield break;
        }

        Vector3 fromPos = card.position;
        Vector3 toPos;

        if (useScreenCenter)
        {
            Vector3 c = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0f));
            Vector3 wanted = new Vector3(c.x + centerOffset.x, c.y + centerOffset.y, fromPos.z);
            Vector3 diff = (cardVisual != null) ? card.position - cardVisual.position : Vector3.zero;
            toPos = wanted + diff;
        }
        else
        {
            toPos = new Vector3(centerPos.x, centerPos.y, fromPos.z);
        }

        Vector3 fromScale = card.localScale;
        Vector3 toScale = fromScale * zoomScale;
        Quaternion fromRot = card.rotation;

        float e = 0f;
        while (e < moveTime)
        {
            e += Time.deltaTime;
            float raw = e / moveTime;
            float p = 1f - Mathf.Pow(1f - raw, 3f);

            card.position = Vector3.Lerp(fromPos, toPos, p);
            card.localScale = Vector3.Lerp(fromScale, toScale, p);
            card.rotation = Quaternion.Lerp(fromRot, Quaternion.identity, p);

            if (raw >= cutAtPercent)
            {
                SceneManager.LoadScene(nextScene);
                yield break;
            }

            yield return null;
        }

        SceneManager.LoadScene(nextScene);
    }

    IEnumerator ArrangeContent()
    {
        if (cardVisual == null || contentRoot == null) yield break;

        Bounds b = new Bounds();
        bool first = true;
        foreach (var r in contentRoot.GetComponentsInChildren<Renderer>())
        {
            if (selectButton != null && r.transform.IsChildOf(selectButton)) continue;
            if (first) { b = r.bounds; first = false; }
            else b.Encapsulate(r.bounds);
        }
        if (first) yield break;

        Vector3 cardCenter = GetCenter(cardVisual);
        Vector3 shift = cardCenter - b.center
            + new Vector3(contentOffset.x, contentOffset.y, 0f);

        Vector3 from = contentRoot.localPosition;
        Vector3 to = from + shift;

        float e = 0f;
        while (e < moveTime)
        {
            e += Time.deltaTime;
            float p = 1f - Mathf.Pow(1f - (e / moveTime), 3f);
            contentRoot.localPosition = Vector3.Lerp(from, to, p);
            yield return null;
        }
        contentRoot.localPosition = to;
    }

    Vector3 GetCenter(Transform t)
    {
        Renderer r = t.GetComponent<Renderer>();
        return (r != null) ? r.bounds.center : t.position;
    }

    IEnumerator FadeGroup(Transform root, float time)
    {
        SpriteRenderer[] sprites = root.GetComponentsInChildren<SpriteRenderer>(true);
        TMPro.TMP_Text[] texts = root.GetComponentsInChildren<TMPro.TMP_Text>(true);

        Color[] sBase = new Color[sprites.Length];
        for (int i = 0; i < sprites.Length; i++) sBase[i] = sprites[i].color;

        Color[] tBase = new Color[texts.Length];
        for (int i = 0; i < texts.Length; i++) tBase[i] = texts[i].color;

        float e = 0f;
        while (e < time)
        {
            e += Time.deltaTime;
            float a = 1f - (e / time);

            for (int i = 0; i < sprites.Length; i++)
            {
                Color c = sBase[i]; c.a = sBase[i].a * a; sprites[i].color = c;
            }
            for (int i = 0; i < texts.Length; i++)
            {
                Color c = tBase[i]; c.a = tBase[i].a * a; texts[i].color = c;
            }
            yield return null;
        }
    }

    IEnumerator Punch(Transform t)
    {
        Vector3 baseScale = t.localScale;
        Vector3 bigScale = baseScale * punchScale;
        yield return Scale(t, baseScale, bigScale, punchTime * 0.4f);
        yield return Scale(t, bigScale, baseScale, punchTime * 0.6f);
        t.localScale = baseScale;
    }

    IEnumerator Scale(Transform t, Vector3 from, Vector3 to, float time)
    {
        float e = 0f;
        while (e < time)
        {
            e += Time.deltaTime;
            float p = e / time;
            p = 1f - Mathf.Pow(1f - p, 3f);
            t.localScale = Vector3.Lerp(from, to, p);
            yield return null;
        }
        t.localScale = to;
    }
}