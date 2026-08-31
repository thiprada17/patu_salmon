using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class CardSelect : MonoBehaviour
{
    [SerializeField] GameObject glow;
    [SerializeField] int characterId;
    [SerializeField] string nextScene = "GameScene";
    [SerializeField] float delay = 0.6f;

    [Header("เด้ง")]
    [SerializeField] Transform card;
    [SerializeField] float punchScale = 1.15f;
    [SerializeField] float punchTime = 0.18f;

    static bool locked = false;

    void Start()
    {
        locked = false;
        if (glow != null) glow.SetActive(false);
    }

    void OnMouseDown()
    {
        Debug.Log("กดโดนแล้ว: " + gameObject.name);
        if (locked) return;
        locked = true;

        if (glow != null) glow.SetActive(true);
        PlayerPrefs.SetInt("SelectedCharacter", characterId);
        PlayerPrefs.Save();

        StartCoroutine(Punch(transform));
        if (card != null) StartCoroutine(Punch(card));

        StartCoroutine(GoNext());
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

    IEnumerator GoNext()
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(nextScene);
    }
}