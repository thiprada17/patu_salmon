using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// ใช้แทน SceneManager.LoadScene(...) ทุกที่:  SceneFader.LoadScene("idcard");
// ไม่ต้องลากใส่ scene ไหน สคริปต์สร้างตัวเองอัตโนมัติ และอยู่ข้าม scene ได้
public class SceneFader : MonoBehaviour
{
    const float FadeTime = 0.25f;                                   // เวลาต่อขา (ออก/เข้า)
    static readonly Color FadeColor = new Color(0.93f, 0.96f, 1f);  // ฟ้าอ่อน เข้ากับฉากท้องฟ้า

    static SceneFader instance;
    CanvasGroup group;
    bool busy;

    public static void LoadScene(string sceneName)
    {
        if (instance == null) Create();
        if (!instance.busy) instance.StartCoroutine(instance.Run(sceneName));
    }

    static void Create()
    {
        var go = new GameObject("SceneFader");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<SceneFader>();

        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;                 // อยู่บนสุดเสมอ
        go.AddComponent<GraphicRaycaster>();       // กันกดปุ่มซ้ำระหว่างเปลี่ยนฉาก

        instance.group = go.AddComponent<CanvasGroup>();
        instance.group.alpha = 0f;
        instance.group.blocksRaycasts = false;

        var img = new GameObject("Fade").AddComponent<Image>();
        img.transform.SetParent(go.transform, false);
        img.color = FadeColor;
        var rt = img.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    IEnumerator Run(string sceneName)
    {
        busy = true;
        group.blocksRaycasts = true;

        // เริ่มโหลด scene ใหม่เบื้องหลังระหว่างที่จอกำลังจางออก
        var op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        yield return Fade(0f, 1f);

        while (op.progress < 0.9f) yield return null;
        op.allowSceneActivation = true;
        while (!op.isDone) yield return null;

        yield return Fade(1f, 0f);

        group.blocksRaycasts = false;
        busy = false;
    }

    IEnumerator Fade(float from, float to)
    {
        for (float t = 0f; t < FadeTime; t += Time.unscaledDeltaTime)
        {
            float k = Mathf.SmoothStep(0f, 1f, t / FadeTime);   // ช้า-เร็ว-ช้า ไม่กระตุก
            group.alpha = Mathf.Lerp(from, to, k);
            yield return null;
        }
        group.alpha = to;
    }
}