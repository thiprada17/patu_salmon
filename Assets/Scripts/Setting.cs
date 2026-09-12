using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class SettingScreen : MonoBehaviour
{
    // ---------- USERNAME ----------
    [Header("Username")]
    [SerializeField] TMP_InputField usernameInput;
    [SerializeField] Button editButton;             // edit_icon
    [SerializeField] int maxUsernameLength = 12;

    // ---------- SOUND ----------
    [Header("Sound")]
    [SerializeField] Button effectButton;
    [SerializeField] TMP_Text effectValue;
    [SerializeField] Button backgroundButton;
    [SerializeField] TMP_Text backgroundValue;

    // ---------- LANGUAGE ----------
    [Header("Language")]
    [SerializeField] Button languageButton;   // ตัว "Thai ▼" กดแล้วเปิด/ปิด
    [SerializeField] TMP_Text languageValue;  // ข้อความ "Thai"
    [SerializeField] GameObject dropdownPanel;   // กล่อง dropdown ที่วางไว้ในฉาก
    [SerializeField] Button[] langButtons;       // ปุ่มแต่ละภาษา เรียง Thai, English, Japanese
    [SerializeField] GameObject[] langChecks;    // ติ๊กถูกของแต่ละภาษา เรียงให้ตรงกับด้านบน

    readonly string[] langNames = { "Thai", "English", "Japanese" };
    readonly string[] langCodes = { "th", "en", "ja" };

    // ---------- SCENES ----------
    [Header("Logout")]
    [SerializeField] Button logoutButton;
    [SerializeField] string logoutPopupScene = "Setting_A1";
    [SerializeField] bool popupAdditive = true;

    [Header("Back")]
    [SerializeField] Button backButton;
    [SerializeField] string backScene = "MainMenu";

    // ---------- เด้ง ----------
    [Header("เด้ง")]
    [SerializeField] float punchScale = 1.15f;
    [SerializeField] float punchTime = 0.18f;

    int langIndex;
    bool isBusy;

    // =====================================================
    void Start()
    {
        NoClip(languageValue);
        NoClip(effectValue);
        NoClip(backgroundValue);

        if (usernameInput != null)
        {
            NoClip(usernameInput.textComponent);
            usernameInput.lineType = TMP_InputField.LineType.SingleLine;
            usernameInput.characterLimit = maxUsernameLength;
            usernameInput.text = PlayerPrefs.GetString("Username", usernameInput.text);
            usernameInput.readOnly = true;
            usernameInput.onEndEdit.AddListener(EndEditUsername);
        }

        SetToggleLabel(effectValue, PlayerPrefs.GetInt("SfxOn", 1) == 1);
        SetToggleLabel(backgroundValue, PlayerPrefs.GetInt("BgmOn", 1) == 1);

        langIndex = Mathf.Clamp(PlayerPrefs.GetInt("LanguageIndex", 0), 0, langNames.Length - 1);
        SetLanguageLabel(langIndex);
        RefreshChecks();

        if (dropdownPanel != null) dropdownPanel.SetActive(false);

        // ผูกปุ่มภาษาแต่ละตัว
        if (langButtons != null)
        {
            for (int i = 0; i < langButtons.Length; i++)
            {
                if (langButtons[i] == null) continue;
                int captured = i;
                langButtons[i].onClick.AddListener(() => ChooseLanguage(captured));
            }
        }

        if (editButton != null) editButton.onClick.AddListener(BeginEditUsername);
        if (effectButton != null) effectButton.onClick.AddListener(() => ToggleSound("SfxOn", effectValue, effectButton));
        if (backgroundButton != null) backgroundButton.onClick.AddListener(() => ToggleSound("BgmOn", backgroundValue, backgroundButton));
        if (languageButton != null) languageButton.onClick.AddListener(ToggleDropdown);
        if (logoutButton != null) logoutButton.onClick.AddListener(OnLogout);
        if (backButton != null) backButton.onClick.AddListener(OnBack);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (dropdownPanel != null && dropdownPanel.activeSelf) CloseDropdown();
            else OnBack();
        }
    }

    void NoClip(TMP_Text t)
    {
        if (t == null) return;
        t.enableWordWrapping = false;
        t.overflowMode = TextOverflowModes.Overflow;
    }

    // =====================================================
    // USERNAME
    // =====================================================
    void BeginEditUsername()
    {
        if (usernameInput == null) return;

        usernameInput.readOnly = false;
        usernameInput.Select();
        usernameInput.ActivateInputField();
        usernameInput.caretPosition = usernameInput.text.Length;

        if (editButton != null) StartCoroutine(Punch(editButton.transform));
    }

    void EndEditUsername(string value)
    {
        value = value.Trim();

        if (value.Length > 0)
        {
            PlayerPrefs.SetString("Username", value);
            PlayerPrefs.Save();
        }
        else
        {
            usernameInput.text = PlayerPrefs.GetString("Username", "username");
        }

        usernameInput.readOnly = true;
    }

    // =====================================================
    // SOUND
    // =====================================================
    void ToggleSound(string key, TMP_Text label, Button btn)
    {
        bool next = PlayerPrefs.GetInt(key, 1) == 0;
        PlayerPrefs.SetInt(key, next ? 1 : 0);
        PlayerPrefs.Save();

        SetToggleLabel(label, next);
        if (btn != null) StartCoroutine(Punch(btn.transform));

        // ตอนจะต่อเสียงจริง ค่อยมาเติมตรงนี้
        // AudioManager.Instance.SetSfx(next);
    }

    void SetToggleLabel(TMP_Text label, bool on)
    {
        if (label != null) label.text = on ? "ON" : "OFF";
    }

    // =====================================================
    // LANGUAGE
    // =====================================================
    void ToggleDropdown()
    {
        if (dropdownPanel == null) return;
        dropdownPanel.SetActive(!dropdownPanel.activeSelf);
    }

    void CloseDropdown()
    {
        if (dropdownPanel != null) dropdownPanel.SetActive(false);
    }

    void ChooseLanguage(int index)
    {
        langIndex = Mathf.Clamp(index, 0, langNames.Length - 1);

        PlayerPrefs.SetInt("LanguageIndex", langIndex);
        PlayerPrefs.SetString("Language", langCodes[langIndex]);
        PlayerPrefs.Save();

        SetLanguageLabel(langIndex);
        RefreshChecks();
        CloseDropdown();
    }

    // เปิดติ๊กถูกเฉพาะภาษาที่เลือก ที่เหลือปิด
    void RefreshChecks()
    {
        if (langChecks == null) return;

        for (int i = 0; i < langChecks.Length; i++)
        {
            if (langChecks[i] != null) langChecks[i].SetActive(i == langIndex);
        }
    }

    void SetLanguageLabel(int index)
    {
        if (languageValue == null) return;
        languageValue.text = langNames[Mathf.Clamp(index, 0, langNames.Length - 1)];
    }

    // =====================================================
    // SCENE
    // =====================================================
    void OnLogout()
    {
        if (isBusy) return;
        isBusy = true;
        StartCoroutine(PunchThen(logoutButton.transform, () =>
        {
            if (popupAdditive)
            {
                SceneManager.LoadScene(logoutPopupScene, LoadSceneMode.Additive);
                isBusy = false;
            }
            else
            {
                SceneManager.LoadScene(logoutPopupScene);
            }
        }));
    }

    void OnBack()
    {
        if (isBusy) return;
        isBusy = true;
        StartCoroutine(PunchThen(backButton.transform, () =>
        {
            SceneManager.LoadScene(backScene);
        }));
    }

    // =====================================================
    // ANIMATION
    // =====================================================
    IEnumerator Punch(Transform t)
    {
        Vector3 baseScale = t.localScale;
        Vector3 bigScale = baseScale * punchScale;
        yield return Scale(t, baseScale, bigScale, punchTime * 0.4f);
        yield return Scale(t, bigScale, baseScale, punchTime * 0.6f);
    }

    IEnumerator PunchThen(Transform t, System.Action done)
    {
        yield return Punch(t);
        done?.Invoke();
    }

    IEnumerator Scale(Transform t, Vector3 from, Vector3 to, float time)
    {
        float e = 0f;
        while (e < time)
        {
            e += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(e / time);
            k = 1f - Mathf.Pow(1f - k, 3f); // ease-out cubic
            t.localScale = Vector3.LerpUnclamped(from, to, k);
            yield return null;
        }
        t.localScale = to;
    }
}