using UnityEngine;
using System.Collections;

public class LoadProfile : MonoBehaviour
{
    [SerializeField] SpriteRenderer profile;
    [SerializeField] Sprite[] characterSprites;

    [Header("เข้าฉาก")]
    [SerializeField] Transform cardRoot;
    [SerializeField] float fadeInTime = 0.4f;

    [Header("ขนาดรูป")]
    [SerializeField] float targetHeight = 2.2f;

    void Awake()
    {
        int id = PlayerPrefs.GetInt("SelectedCharacter", 0);
        if (profile != null && id < characterSprites.Length)
        {
            profile.sprite = characterSprites[id];
            FitSize();
        }
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
        SpriteRenderer[] sprites = cardRoot.GetComponentsInChildren<SpriteRenderer>(true);
        TMPro.TMP_Text[] texts = cardRoot.GetComponentsInChildren<TMPro.TMP_Text>(true);

        Color[] sBase = new Color[sprites.Length];
        for (int i = 0; i < sprites.Length; i++) sBase[i] = sprites[i].color;

        Color[] tBase = new Color[texts.Length];
        for (int i = 0; i < texts.Length; i++) tBase[i] = texts[i].color;

        float e = 0f;
        while (e < fadeInTime)
        {
            e += Time.deltaTime;
            float a = e / fadeInTime;

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

        for (int i = 0; i < sprites.Length; i++) sprites[i].color = sBase[i];
        for (int i = 0; i < texts.Length; i++) texts[i].color = tBase[i];
    }
}