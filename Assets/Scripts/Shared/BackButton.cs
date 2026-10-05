using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class BackButton : MonoBehaviour
{
    public GameObject showPage;
    public GameObject hidePage;

    [SerializeField] string sceneName = "MainMenu";

    [Header("Punch")]
    [SerializeField] float punchScale = 1.15f;
    [SerializeField] float punchTime = 0.18f;

    bool isBusy;

    void OnEnable()
    {
        isBusy = false;
    }

    void OnMouseDown()
    {
        if (isBusy) return;
        isBusy = true;
        StartCoroutine(PunchThenGo());
    }

    IEnumerator PunchThenGo()
    {
        Vector3 baseScale = transform.localScale;
        Vector3 bigScale = baseScale * punchScale;

        yield return Scale(baseScale, bigScale, punchTime * 0.4f);
        yield return Scale(bigScale, baseScale, punchTime * 0.6f);

        if (showPage != null)
        {
            showPage.SetActive(true);
            hidePage.SetActive(false);
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }

    IEnumerator Scale(Vector3 from, Vector3 to, float time)
    {
        float e = 0f;
        while (e < time)
        {
            e += Time.deltaTime;
            float p = e / time;
            p = 1f - Mathf.Pow(1f - p, 3f);
            transform.localScale = Vector3.Lerp(from, to, p);
            yield return null;
        }
        transform.localScale = to;
    }
}