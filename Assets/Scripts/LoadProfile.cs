using UnityEngine;
using System.Collections;

public class LoadProfile : MonoBehaviour
{
    [SerializeField] SpriteRenderer profile;
    [SerializeField] Sprite[] characterSprites;

    [Header("เข้าฉาก")]
    [SerializeField] Transform cardRoot;
    [SerializeField] float fadeInTime = 0.4f;
    [SerializeField] float riseDistance = 0.3f;   // ลอยขึ้นพร้อมจางเข้า (0 = ไม่ลอย)

    [Header("ขนาดรูป")]
    [SerializeField] float targetHeight = 2.2f;

    SpriteRenderer[] sprites;
    TMPro.TMP_Text[] texts;
    Color[] sBase;
    Color[] tBase;
    Vector3 basePos;

    void Awake()
    {
        int id = PlayerPrefs.GetInt("SelectedCharacter", 0);
        if (profile != null && id < characterSprites.Length)
        {
            profile.sprite = characterSprites[id];
            FitSize();
        }

        if (cardRoot == null) return;

        basePos = cardRoot.localPosition;

        // เก็บสีเดิมแล้วซ่อนทันที ก่อนเฟรมแรกจะถูกวาด
        sprites = cardRoot.GetComponentsInChildren<SpriteRenderer>(true);
        texts = cardRoot.GetComponentsInChildren<TMPro.TMP_Text>(true);

        sBase = new Color[sprites.Length];
        for (int i = 0; i < sprites.Length; i++)
        {
            sBase[i] = sprites[i].color;
            SetAlpha(sprites[i], 0f);
        }

        tBase = new Color[texts.Length];
        for (int i = 0; i < texts.Length; i++)
        {
            tBase[i] = texts[i].color;
            SetAlpha(texts[i], 0f);
        }

        cardRoot.localPosition = basePos + Vector3.down * riseDistance;
    }

    void FitSize()
    {
        if (profile.sprite == null) return;

        float h = profile.sprite.bounds.size.y;
        if (h <= 0f) return;

        float s = targetHeight / h;
        profile.transform.localScale = new Vector3(s, s, 1f);
    }

    void Start()
    {
        if (cardRoot != null) StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        float e = 0f;

        while (e < fadeInTime)
        {
            e += Time.deltaTime;

            float p = Mathf.Clamp01(e / fadeInTime);
            float a = 1f - Mathf.Pow(1f - p, 2f);   // เข้าเร็ว จบนุ่ม

            for (int i = 0; i < sprites.Length; i++)
                SetAlpha(sprites[i], sBase[i].a * a);

            for (int i = 0; i < texts.Length; i++)
                SetAlpha(texts[i], tBase[i].a * a);

            cardRoot.localPosition = basePos + Vector3.down * riseDistance * (1f - a);

            yield return null;
        }

        for (int i = 0; i < sprites.Length; i++) sprites[i].color = sBase[i];
        for (int i = 0; i < texts.Length; i++) texts[i].color = tBase[i];

        cardRoot.localPosition = basePos;
    }

    void SetAlpha(SpriteRenderer sr, float a)
    {
        Color c = sr.color;
        c.a = a;
        sr.color = c;
    }

    void SetAlpha(TMPro.TMP_Text t, float a)
    {
        Color c = t.color;
        c.a = a;
        t.color = c;
    }
}