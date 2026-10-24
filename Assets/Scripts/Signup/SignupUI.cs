
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class SignupUI : MonoBehaviour
{
    [Header("input")]
    [SerializeField] TMP_InputField emailInput;
    [SerializeField] TMP_InputField passwordInput;

    [Header("error_text")]
    [SerializeField] TMP_Text emailError;
    [SerializeField] TMP_Text passwordError;

    [Header("error_gameobj")]
    [SerializeField] GameObject emailErrorGroup;
    [SerializeField] GameObject passwordErrorGroup;

    [Header("btn")]
    [SerializeField] Button signupButton;
    [SerializeField] Button googleButton;

    [Header("popup")]
    [SerializeField] LanguagePopup languagePopup;

    [Header("next scene")]
    //[SerializeField] string nextScene = "SelectCharacter";
    //[SerializeField] string existingPlayerScene = "loading";
    //[SerializeField] float delay = 0.25f;

    [Header("punch")]
    [SerializeField] float punchScale = 1.15f;
    [SerializeField] float punchTime = 0.18f;

    [Header("color")]
    [SerializeField] Color borderNormal = new Color(0.80f, 0.80f, 0.80f);
    [SerializeField] Color borderOk = new Color(0.55f, 0.85f, 0.55f);
    [SerializeField] Color borderError = new Color(0.90f, 0.35f, 0.20f);
    [SerializeField] Color messageError = Color.white;

    [Header("hover")]
    [SerializeField] float hoverLighten = 0.15f;
    [SerializeField] float pressDarken = 0.10f;

    private static bool locked = false;

    void Start()
    {
        locked = false;

        HookClearOnFocus(emailInput, emailError, emailErrorGroup);
        HookClearOnFocus(passwordInput, passwordError, passwordErrorGroup);

        if (passwordInput != null)
        {
            passwordInput.onDeselect.AddListener(_ =>
            {
                if (string.IsNullOrEmpty(passwordInput.text)) return;
                Paint(passwordInput, borderOk);
            });
        }

        if (emailInput != null)
        {
            emailInput.onDeselect.AddListener(_ =>
            {
                string email = emailInput.text.Trim();

                if (string.IsNullOrEmpty(email)) return;

                if (!CheckEmail(email))
                {
                    ShowError(emailInput, emailError, emailErrorGroup, "Please enter a valid email.");
                }
                else
                {
                    ClearError(emailInput, emailError, emailErrorGroup);
                    Paint(emailInput, borderOk);
                }
            });
        }

        if (signupButton != null) signupButton.onClick.AddListener(OnSignupClick);

        ClearError(emailInput, emailError, emailErrorGroup);
        ClearError(passwordInput, passwordError, passwordErrorGroup);
    }

    public void Unlock()
    {
        locked = false;
    }

    public void OnSignupClick()
    {
        if (locked) return;
        if (signupButton != null) StartCoroutine(Punch(signupButton.transform));

        if (emailInput == null || passwordInput == null)
        {
            Debug.LogError("SignupUI: Input field is missing.");
            return;
        }

        string email = emailInput.text.Trim();
        string password = passwordInput.text;
        bool valid = true;

        if (string.IsNullOrEmpty(password))
        {
            ShowError(passwordInput, passwordError, passwordErrorGroup, "Password is required.");
            valid = false;
        }
        else
        {
            ClearError(passwordInput, passwordError, passwordErrorGroup);
            Paint(passwordInput, borderOk);
        }

        if (string.IsNullOrEmpty(email))
        {
            ShowError(emailInput, emailError, emailErrorGroup, "Email is required.");
            valid = false;
        }
        else if (!CheckEmail(email))
        {
            ShowError(emailInput, emailError, emailErrorGroup, "Please enter a valid email.");
            valid = false;
        }
        else
        {
            ClearError(emailInput, emailError, emailErrorGroup);
            Paint(emailInput, borderOk);
        }

        if (!valid) return;

        locked = true;
        if (signupButton != null) signupButton.interactable = false;

        LoginManager.Instance.Continue(
            email,
            password,
            OnContinueSuccess,
            OnContinueFailed
        );
    }

    // ---------- callbacks จาก LoginManager ----------

    void OnContinueFailed(string error)
    {
        Debug.LogError("Continue failed: " + error);
        ShowError(passwordInput, passwordError, passwordErrorGroup, error);
        ReleaseButton();
    }

    void ReleaseButton()
    {
        locked = false;

        if (signupButton != null)
            signupButton.interactable = true;
    }

    public void OnContinueSuccess(LoginManager.AuthResponse res)
    {
        if (res == null)
        {
            Debug.LogError("Continue failed: response is empty.");
            ReleaseButton();
            return;
        }

        Debug.Log(
            $"Continue OK. new={res.is_new_user} profile={res.has_profile}"
        );

        if (res.needs_email_confirmation)
        {
            Debug.Log("Please confirm email first");
            ReleaseButton();
            return;
        }

        // Save login status for LoadingScreen.
        PlayerPrefs.SetInt("IsNewUser", res.is_new_user ? 1 : 0);
        PlayerPrefs.SetInt("HasProfile", res.has_profile ? 1 : 0);
        PlayerPrefs.Save();

        if (res.is_new_user || !res.has_profile)
        {
            // Keep the existing new-player flow.
            if (languagePopup != null)
            {
                languagePopup.gameObject.SetActive(true);
                languagePopup.Open();
            }
            else
            {
                Debug.LogError("LanguagePopup is not assigned!");
                ReleaseButton();
            }
        }
        else
        {
            // Existing player: load stats before choosing a scene.
            LoginManager.Instance.GetPlayerStats(
                OnPlayerStatsLoaded,
                OnPlayerStatsFailed
            );
        }
    }

    private void OnPlayerStatsLoaded(LoginManager.PlayerStatsResponse stats)
    {
        if (stats == null)
        {
            OnPlayerStatsFailed("Player stats response is empty.");
            return;
        }

        TutorialSession.Initialize(stats.work_days);

        Debug.Log(
            $"[SignupUI] Initialized: work_days={stats.work_days}, " +
            $"FirstTimePlayer={TutorialSession.IsFirstTimePlayer}"
        );

        if (stats.work_days == 0)
        {
            SceneManager.LoadScene("map");
        }
        else
        {
            SceneManager.LoadScene("loading");
        }
    }

    private void OnPlayerStatsFailed(string error)
    {
        Debug.LogError("[SignupUI] Failed to load player stats: " + error);

        ShowError(
            passwordInput,
            passwordError,
            passwordErrorGroup,
            "Could not load player data. Please try again."
        );

        ReleaseButton();
    }

    // ---------- UI helpers ----------

    void HookClearOnFocus(TMP_InputField field, TMP_Text label, GameObject group)
    {
        if (field == null) return;

        field.onSelect.AddListener(_ => ClearError(field, label, group));
    }

    void ShowError(TMP_InputField field, TMP_Text label, GameObject group, string message)
    {
        Paint(field, borderError);

        if (label != null)
        {
            label.text = message;
            label.color = messageError;
        }

        if (group != null)
            group.SetActive(true);
        else if (label != null)
            label.gameObject.SetActive(true);
        else
            Debug.Log(message);
    }

    void ClearError(TMP_InputField field, TMP_Text label, GameObject group)
    {
        Paint(field, borderNormal);

        if (label != null)
            label.text = "";

        if (group != null)
            group.SetActive(false);
        else if (label != null)
            label.gameObject.SetActive(false);
    }

    void Paint(TMP_InputField field, Color border)
    {
        if (field == null) return;

        ColorBlock colors = field.colors;
        colors.normalColor = border;
        colors.highlightedColor = Color.Lerp(border, Color.white, hoverLighten);
        colors.selectedColor = Color.Lerp(border, Color.white, hoverLighten);
        colors.pressedColor = Color.Lerp(border, Color.black, pressDarken);
        colors.disabledColor = border;
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.1f;

        field.colors = colors;
    }

    bool CheckEmail(string email)
    {
        email = email.Trim();

        int at = email.IndexOf('@');

        return at > 0
            && email.IndexOf('.', at) > at + 1
            && !email.EndsWith(".");
    }

    IEnumerator Punch(Transform target)
    {
        Vector3 originalScale = target.localScale;
        Vector3 enlargedScale = originalScale * punchScale;

        yield return Scale(target, originalScale, enlargedScale, punchTime * 0.4f);
        yield return Scale(target, enlargedScale, originalScale, punchTime * 0.6f);

        target.localScale = originalScale;
    }

    IEnumerator Scale(Transform target, Vector3 from, Vector3 to, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            progress = 1f - Mathf.Pow(1f - progress, 3f);
            target.localScale = Vector3.Lerp(from, to, progress);
            yield return null;
        }

        target.localScale = to;
    }
}
