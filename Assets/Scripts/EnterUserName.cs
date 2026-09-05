using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class EnterName : MonoBehaviour
{
    [SerializeField] TMP_InputField input;
    [SerializeField] string nextScene = "SetCharacter";
    [SerializeField] TextMeshProUGUI warning;    // ข้อความเตือน ปิดไว้ก่อน

    [Header("เด้ง")]
    [SerializeField] float punchScale = 1.12f;
    [SerializeField] float punchTime = 0.18f;

    bool isBusy;

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
        PlayerPrefs.SetString("Username", name);
        PlayerPrefs.Save();
        StartCoroutine(PunchThenGo());
    }

    IEnumerator PunchThenGo()
    {
        Vector3 b = transform.localScale;
        Vector3 big = b * punchScale;
        yield return Scale(b, big, punchTime * 0.4f);
        yield return Scale(big, b, punchTime * 0.6f);
        SceneManager.LoadScene(nextScene);
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