using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using TMPro;

public class Setting : MonoBehaviour
{
    public GameObject settingPopup;

    [Header("Username")]
    [SerializeField] TMP_InputField usernameInput;
    [SerializeField] Button editButton;
    [SerializeField] int maxUsernameLength = 12;

    [Header("ID Card")]
    [SerializeField] Image characterImage;
    [SerializeField] Sprite salmonSprite;
    [SerializeField] Sprite platuSprite;


    [Header("Sound")]
    [SerializeField] Slider effectSlider;
    [SerializeField] Slider backgroundSlider;
    [SerializeField] AudioMixer mixer;
    [SerializeField] string effectParameter = "SFXVolume";
    [SerializeField] string backgroundParameter = "BGMVolume";

    const int MaxLevel = 5;


    [Header("Language")]
    [SerializeField] Button languageButton;
    [SerializeField] TMP_Text languageValue;
    [SerializeField] GameObject dropdownPanel;
    [SerializeField] Button[] langButtons;
    [SerializeField] GameObject[] langChecks;

    readonly string[] langNames = { "Thai", "English", "Japanese" };
    readonly string[] langCodes = { "th", "en", "ja" };


    [Header("Logout")]
    [SerializeField] Button logoutButton;

    [Header("Back")]
    [SerializeField] Button backButton;
    [SerializeField] string backScene = "MainMenu";


    [Header("Punch")]
    [SerializeField] float punchScale = 1.15f;
    [SerializeField] float punchTime = 0.18f;

    int langIndex;
    bool isBusy;

    readonly Dictionary<Transform, Vector3> handleBase = new Dictionary<Transform, Vector3>();
    readonly Dictionary<Transform, Coroutine> handlePunch = new Dictionary<Transform, Coroutine>();


    void Start()
    {
        SetCharacterImage();

        NoClip(languageValue);

        if (usernameInput != null)
        {
            NoClip(usernameInput.textComponent);
            usernameInput.lineType = TMP_InputField.LineType.SingleLine;
            usernameInput.characterLimit = maxUsernameLength;
            usernameInput.text = PlayerPrefs.GetString("Username", usernameInput.text);
            usernameInput.readOnly = true;
            usernameInput.onEndEdit.AddListener(EndEditUsername);
        }

        SetupSlider(effectSlider, "SfxVolume", "SfxOn", effectParameter);
        SetupSlider(backgroundSlider, "BgmVolume", "BgmOn", backgroundParameter);

        langIndex = Mathf.Clamp(PlayerPrefs.GetInt("LanguageIndex", 0), 0, langNames.Length - 1);
        SetLanguageLabel(langIndex);
        RefreshChecks();

        if (dropdownPanel != null) dropdownPanel.SetActive(false);


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
        if (languageButton != null) languageButton.onClick.AddListener(ToggleDropdown);
        if (logoutButton != null) logoutButton.onClick.AddListener(OnLogout);
        if (backButton != null) backButton.onClick.AddListener(OnBack);
    }

    void SetCharacterImage()
    {
        if (characterImage == null) return;

        string character = GameData.selectedCharacter;
        if (string.IsNullOrEmpty(character)) character = PlayerPrefs.GetString("Character", "salmon");

        Sprite s = character == "platu" ? platuSprite : salmonSprite;
        if (s != null) characterImage.sprite = s;
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
        t.textWrappingMode = TextWrappingModes.NoWrap;
        t.overflowMode = TextOverflowModes.Overflow;
    }


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


    void SetupSlider(Slider slider, string volumeKey, string onKey, string parameter)
    {
        if (slider == null) return;

        slider.wholeNumbers = false;
        slider.minValue = 0f;
        slider.maxValue = 1f;

        float fallback = PlayerPrefs.GetInt(onKey, 1) == 1 ? 1f : 0f;
        float volume = Mathf.Clamp01(PlayerPrefs.GetFloat(volumeKey, fallback));

        slider.SetValueWithoutNotify(volume);
        ApplyVolume(parameter, volume);

        slider.onValueChanged.AddListener(v => OnVolumeChanged(volumeKey, onKey, parameter, v));
    }

    void OnVolumeChanged(string volumeKey, string onKey, string parameter, float volume)
    {
        PlayerPrefs.SetFloat(volumeKey, volume);
        PlayerPrefs.SetInt(onKey, volume > 0.001f ? 1 : 0);
        ApplyVolume(parameter, volume);
    }

    void ApplyVolume(string parameter, float volume)
    {
        if (mixer == null) return;
        float db = volume <= 0.001f ? -80f : Mathf.Log10(volume) * 20f;
        mixer.SetFloat(parameter, db);
    }

    void OnDisable()
    {
        PlayerPrefs.Save();
    }

    void PunchHandle(Slider slider)
    {
        Transform t = slider.handleRect;
        if (t == null) return;

        if (!handleBase.ContainsKey(t)) handleBase[t] = t.localScale;
        if (handlePunch.TryGetValue(t, out Coroutine running) && running != null) StopCoroutine(running);

        t.localScale = handleBase[t];
        handlePunch[t] = StartCoroutine(Punch(t));
    }


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


    void OnLogout()
    {
        if (isBusy) return;
        isBusy = true;
        StartCoroutine(PunchThen(logoutButton.transform, () =>
        {
            settingPopup.SetActive(true);
            isBusy = false;

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
            k = 1f - Mathf.Pow(1f - k, 3f);
            t.localScale = Vector3.LerpUnclamped(from, to, k);
            yield return null;
        }
        t.localScale = to;
    }
}