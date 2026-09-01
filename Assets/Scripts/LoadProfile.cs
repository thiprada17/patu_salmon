using UnityEngine;
using System.Collections;

public class LoadProfile : MonoBehaviour
{
    [SerializeField] SpriteRenderer profile;
    [SerializeField] Sprite[] characterSprites;

    [Header("เข้าฉาก")]
    [SerializeField] Transform cardRoot;
    [SerializeField] float startScale = 1.25f;
    [SerializeField] float settleTime = 0.35f;

    void Awake()
    {
        int id = PlayerPrefs.GetInt("SelectedCharacter", 0);
        if (profile != null && id < characterSprites.Length)
            profile.sprite = characterSprites[id];
    }

    void Start()
    {
        if (cardRoot != null) StartCoroutine(Settle());
    }

    IEnumerator Settle()
    {
        Vector3 target = cardRoot.localScale;
        Vector3 from = target * startScale;
        cardRoot.localScale = from;

        float e = 0f;
        while (e < settleTime)
        {
            e += Time.deltaTime;
            float p = 1f - Mathf.Pow(1f - (e / settleTime), 3f);
            cardRoot.localScale = Vector3.Lerp(from, target, p);
            yield return null;
        }
        cardRoot.localScale = target;
    }
}