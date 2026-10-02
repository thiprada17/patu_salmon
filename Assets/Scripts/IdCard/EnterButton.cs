using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using DG.Tweening;

public class EnterButton : MonoBehaviour
{
    public TMP_InputField inputField;
    public string nextScene;

    int lastFrame = -1;
    bool leaving;

    void Awake()
    {
        Button b = GetComponent<Button>();
        if (b != null) b.onClick.AddListener(OnClick);
        if (inputField == null)
            inputField = FindFirstObjectByType<TMP_InputField>(FindObjectsInactive.Include);
    }

    public void OnClick()
    {
        if (leaving || lastFrame == Time.frameCount) return;
        lastFrame = Time.frameCount;

        string username = inputField != null ? inputField.text.Trim() : "";
        if (username == "")
        {
            Debug.Log("EnterButton: ยังไม่ได้พิมพ์ชื่อ");
            transform.DOKill(true);
            transform.DOShakeRotation(0.3f, new Vector3(0, 0, 10));
            return;
        }
        if (!Application.CanStreamedLevelBeLoaded(nextScene))
        {
            Debug.LogError("EnterButton: โหลด scene \"" + nextScene + "\" ไม่ได้ เช็กชื่อและ Build Profiles");
            return;
        }

        leaving = true;
        GameData.username = username;
        transform.DOKill(true);
        Sequence s = DOTween.Sequence();
        s.Append(transform.DOScale(0.88f, 0.08f));
        s.Append(transform.DOScale(1f, 0.15f).SetEase(Ease.OutBack));
        s.OnComplete(() => SceneManager.LoadScene(nextScene));
    }
}