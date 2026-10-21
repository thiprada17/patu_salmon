using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class IdCard : MonoBehaviour
{
    [System.Serializable]
    public class CharacterCardData
    {
        public string id;
        public Sprite portrait;
        public string displayName;

        [TextArea]
        public string description;
    }

    [Header("Character Data")]
    [SerializeField] private CharacterCardData[] characters;

    [Header("Card UI")]
    [SerializeField] private Image profile;
    [SerializeField] private TMP_Text characterNameText;
    [SerializeField] private TMP_Text characterDescriptionText;

    [Header("Enter Animation (clip ที่สร้างใหม่)")]
    [SerializeField] private Animator cardAnimator;
    [Tooltip("ว่างไว้ได้ ถ้าตั้ง clip เป็น Default State อยู่แล้ว")]
    [SerializeField] private string enterStateName = "idcardin";

    [Header("Punch (ตอนกดเข้า)")]
    [SerializeField] private RectTransform cardRoot;
    [SerializeField] private float punchScale = 1.12f;
    [SerializeField] private float punchTime = 0.18f;

    [Header("Enter")]
    [SerializeField] private TMP_InputField input;
    [SerializeField] private TMP_Text warning;
    [SerializeField] private string nextScene = "SelectCharacter";

    private bool isBusy;
    private CharacterCardData selectedCharacter;

    private void Awake()
    {
        if (warning != null)
            warning.gameObject.SetActive(false);

        LoadCharacter();
    }

    private void Start()
    {
        PlayEnterAnimation();
    }

    private void PlayEnterAnimation()
    {
        if (cardAnimator == null || selectedCharacter == null)
            return;

        if (string.IsNullOrWhiteSpace(enterStateName))
            return; // ใช้ Default State ของ Animator เล่นเอง

        int hash = Animator.StringToHash(enterStateName);

        if (!cardAnimator.HasState(0, hash))
        {
            GameLogger.Error(
                "IdCard",
                "ไม่พบ Animation State: " + enterStateName
            );
            return;
        }

        cardAnimator.Play(hash, 0, 0f);
    }

    private void LoadCharacter()
    {
        string id = PlayerPrefs.GetString(
            "SelectedCharacter", ""
        ).Trim().ToLowerInvariant();

        selectedCharacter = null;

        if (characters != null)
        {
            foreach (CharacterCardData data in characters)
            {
                if (data == null || string.IsNullOrWhiteSpace(data.id))
                    continue;

                if (data.id.Trim().ToLowerInvariant() == id)
                {
                    selectedCharacter = data;
                    break;
                }
            }
        }

        if (selectedCharacter == null)
        {
            GameLogger.Error(
                "IdCard",
                "Character data not found for ID: " + id
            );

            ShowWarning("ไม่พบข้อมูลตัวละคร กรุณากลับไปเลือกใหม่");
            return;
        }

        // กำหนดรูปให้ UI Image
        if (profile != null)
        {
            profile.sprite = selectedCharacter.portrait;
            profile.preserveAspect = true;

            if (profile.sprite == null)
            {
                GameLogger.Error(
                    "IdCard",
                    "Portrait is missing for: " + id
                );
            }
        }

        // กำหนดข้อความของตัวละคร
        if (characterNameText != null)
            characterNameText.text = selectedCharacter.displayName;

        if (characterDescriptionText != null)
            characterDescriptionText.text =
                selectedCharacter.description;

        GameLogger.Info(
            "IdCard",
            "Loaded character: " + selectedCharacter.id
        );
    }

    public void OnEnterClick()
    {
        if (isBusy)
            return;

        if (selectedCharacter == null)
        {
            ShowWarning("ไม่พบข้อมูลตัวละคร กรุณากลับไปเลือกใหม่");
            return;
        }

        string username = input != null
            ? input.text.Trim()
            : "";

        if (string.IsNullOrWhiteSpace(username))
        {
            ShowWarning("ใส่ชื่อก่อนน้า");
            return;
        }

        if (LoginManager.Instance == null)
        {
            GameLogger.Error(
                "IdCard",
                "LoginManager instance not found."
            );

            ShowWarning("ระบบยังไม่พร้อม กรุณาลองใหม่");
            return;
        }

        isBusy = true;

        if (warning != null)
            warning.gameObject.SetActive(false);

        PlayerPrefs.SetString("Username", username);
        PlayerPrefs.Save();

        string language = PlayerPrefs.GetString(
            "Language", "english"
        );

        GameLogger.Info(
            "IdCard",
            "Sending starter player data."
        );

        LoginManager.Instance.SendStarterPlayer(
            username,
            selectedCharacter.id,
            language,
            onSuccess: () =>
            {
                GameLogger.Info(
                    "IdCard",
                    "Player data sent successfully."
                );

                StartCoroutine(EnterAfterSuccess());
            },
            onError: error =>
            {
                GameLogger.Error(
                    "IdCard",
                    "Send player data failed: " + error
                );

                isBusy = false;
                ShowWarning("บันทึกข้อมูลไม่สำเร็จ กรุณาลองใหม่");
            }
        );
    }

    private IEnumerator EnterAfterSuccess()
    {
        if (cardRoot != null)
            yield return PunchCard();

        if (SceneTransition.Instance != null)
            SceneTransition.Instance.LoadScene(nextScene);
        else
        {
            GameLogger.Warn("IdCard", "SceneTransition ไม่พบ โหลดแบบตัดทันที");
            SceneManager.LoadScene(nextScene);
        }
    }

    private IEnumerator PunchCard()
    {
        Vector3 original = cardRoot.localScale;
        Vector3 enlarged = original * punchScale;

        yield return ScaleCard(
            original, enlarged, punchTime * 0.4f
        );

        yield return ScaleCard(
            enlarged, original, punchTime * 0.6f
        );

        cardRoot.localScale = original;
    }

    private IEnumerator ScaleCard(
        Vector3 from, Vector3 to, float duration)
    {
        if (duration <= 0f)
        {
            cardRoot.localScale = to;
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            cardRoot.localScale = Vector3.Lerp(
                from,
                to,
                EaseOut(elapsed / duration)
            );

            yield return null;
        }

        cardRoot.localScale = to;
    }

    private void ShowWarning(string message)
    {
        if (warning == null)
            return;

        warning.text = message;
        warning.gameObject.SetActive(true);
    }

    private static float EaseOut(float value)
    {
        float p = Mathf.Clamp01(value);
        return 1f - Mathf.Pow(1f - p, 3f);
    }
}