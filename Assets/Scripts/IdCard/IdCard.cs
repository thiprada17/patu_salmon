using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class IdCard : MonoBehaviour
{
    [Header("profile")]
    [SerializeField] SpriteRenderer profile;
    [SerializeField] Sprite[] characterSprites;
    [SerializeField] float targetHeight = 2.2f;

    [Header("card pop-in")]
    [SerializeField] Transform cardRoot;
    [SerializeField] Transform[] followers;
    [SerializeField] float popTime = 0.35f;
    [SerializeField] float startScale = 0.9f;
    [SerializeField] float riseDistance = 0.3f;

    [Header("enter")]
    [SerializeField] TMP_InputField input;
    [SerializeField] TextMeshProUGUI warning;
    [SerializeField] string nextScene = "SelectCharacter";
    [SerializeField] float punchScale = 1.12f;
    [SerializeField] float punchTime = 0.18f;

    Transform[] roots;
    Vector3[] basePos;
    Vector3[] baseScale;
    bool isBusy;

    void Awake()
    {
        ShowProfile();
        SetupCard();
    }

    void Start()
    {
        if (roots != null) StartCoroutine(PopIn());
    }

    void ShowProfile()
    {
        int id = PlayerPrefs.GetInt("SelectedCharacter", 0);
        if (profile == null || id < 0 || id >= characterSprites.Length) return;

        profile.sprite = characterSprites[id];

        float h = profile.sprite != null ? profile.sprite.bounds.size.y : 0f;
        if (h <= 0f) return;

        float s = targetHeight / h;
        profile.transform.localScale = new Vector3(s, s, 1f);
    }

    void SetupCard()
    {
        if (cardRoot == null) return;

        var list = new List<Transform> { cardRoot };
        if (followers != null) list.AddRange(followers);
        roots = list.ToArray();

        basePos = new Vector3[roots.Length];
        baseScale = new Vector3[roots.Length];
        for (int i = 0; i < roots.Length; i++)
        {
            basePos[i] = roots[i].localPosition;
            baseScale[i] = roots[i].localScale;
        }

        ApplyPop(0f);
    }

    IEnumerator PopIn()
    {
        for (float e = 0f; e < popTime; e += Time.deltaTime)
        {
            ApplyPop(EaseOut(e / popTime));
            yield return null;
        }
        ApplyPop(1f);
    }

    void ApplyPop(float k)
    {
        float s = Mathf.Lerp(startScale, 1f, k);
        Vector3 rise = Vector3.down * riseDistance * (1f - k);
        Vector3 pivot = basePos[0];

        for (int i = 0; i < roots.Length; i++)
        {
            Vector3 offset = basePos[i] - pivot;
            offset.x *= s;
            offset.y *= s;

            roots[i].localScale = baseScale[i] * s;
            roots[i].localPosition = pivot + offset + rise;
        }
    }

    void OnMouseDown()
    {
        if (isBusy) return;

        string name = input.text.Trim();
        if (string.IsNullOrEmpty(name))
        {
            if (warning != null)
            {
                warning.gameObject.SetActive(true);
                warning.text = "ใส่ชื่อก่อนน้า";
            }
            return;
        }

        isBusy = true;

        Debug.Log("Username: " + name);
        PlayerPrefs.SetString("Username", name);
        PlayerPrefs.Save();

        string character = PlayerPrefs.GetString("SelectedCharacter", "salmon");

        LoginManager.Instance.SendStarterPlayer(
            name,
            character,
            onSuccess: () =>
            {
                Debug.Log("Player data sent successfully!");
                StartCoroutine(PunchThenGo());
            },
            onError: err =>
            {
                Debug.LogError("Send player data failed: " + err);
                isBusy = false;
            });

    }

    IEnumerator PunchThenGo()
    {
        Vector3 b = transform.localScale;
        Vector3 big = b * punchScale;
        yield return ScaleTo(b, big, punchTime * 0.4f);
        yield return ScaleTo(big, b, punchTime * 0.6f);
        SceneFader.LoadScene(nextScene);
    }

    IEnumerator ScaleTo(Vector3 from, Vector3 to, float time)
    {
        for (float e = 0f; e < time; e += Time.deltaTime)
        {
            transform.localScale = Vector3.Lerp(from, to, EaseOut(e / time));
            yield return null;
        }
        transform.localScale = to;
    }

    static float EaseOut(float p) => 1f - Mathf.Pow(1f - Mathf.Clamp01(p), 3f);
}