using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class SignupUI : MonoBehaviour
{
    public GameObject Language;

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
    [SerializeField] string nextScene = "SetCharacter";
    [SerializeField] float delay = 0.25f;

    [Header("punch")]
    [SerializeField] float punchScale = 1.15f;
    [SerializeField] float punchTime = 0.18f;

    [Header("color")]
    [SerializeField] Color borderNormal = new Color(0.80f, 0.80f, 0.80f);
    [SerializeField] Color borderOk = new Color(0.55f, 0.85f, 0.55f);
    [SerializeField] Color borderError = new Color(0.90f, 0.35f, 0.20f);
    [SerializeField] Color messageError = new Color(1f, 1f, 1f);

    [Header("hover")]
    [SerializeField] float hoverLighten = 0.15f;
    [SerializeField] float pressDarken = 0.10f;

    static bool locked = false;

    void Start()
    {
        locked = false;


        HookClearOnFocus(emailInput, emailError, emailErrorGroup);
        HookClearOnFocus(passwordInput, passwordError, passwordErrorGroup);



        if (passwordInput != null)
        {
            passwordInput.onDeselect.AddListener(_ =>
            {
                if (passwordInput.text == "") return;
                Paint(passwordInput, borderOk);
            });
        }

        if (emailInput != null)
        {
            emailInput.onDeselect.AddListener(_ =>
            {
                string s = emailInput.text.Trim();
                if (s == "") return;

                if (!CheckEmail(s))
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
        if (googleButton != null) googleButton.onClick.AddListener(OnGoogleClick);


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


        string e = emailInput.text.Trim();
        string p = passwordInput.text;

        bool pass = true;



        if (p == "")
        {
            ShowError(passwordInput, passwordError, passwordErrorGroup, "Password is required.");
            pass = false;
        }
        else
        {
            ClearError(passwordInput, passwordError, passwordErrorGroup);
            Paint(passwordInput, borderOk);
        }

        if (e == "")
        {
            ShowError(emailInput, emailError, emailErrorGroup, "Email is required.");
            pass = false;
        }
        else if (!CheckEmail(e))
        {
            ShowError(emailInput, emailError, emailErrorGroup, "Please enter a valid email.");
            pass = false;
        }
        else
        {
            ClearError(emailInput, emailError, emailErrorGroup);
            Paint(emailInput, borderOk);
        }

        if (!pass) return;

        locked = true;


        PlayerPrefs.SetString("Email", e);
        PlayerPrefs.Save();

        Language.SetActive(true);
    }

    public void OnGoogleClick()
    {
        if (locked) return;

        if (googleButton != null) StartCoroutine(Punch(googleButton.transform));

        locked = true;

        Language.SetActive(true);
    }

    IEnumerator GoNext()
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(nextScene);
    }

    void HookClearOnFocus(TMP_InputField field, TMP_Text label, GameObject group)
    {
        if (field == null) return;
        field.onSelect.AddListener(_ => ClearError(field, label, group));
    }

    void ShowError(TMP_InputField field, TMP_Text label, GameObject group, string msg)
    {
        Paint(field, borderError);

        if (label != null)
        {
            label.text = msg;
            label.color = messageError;
        }

        if (group != null) group.SetActive(true);
        else if (label != null) label.gameObject.SetActive(true);
        else Debug.Log(msg);
    }

    void ClearError(TMP_InputField field, TMP_Text label, GameObject group)
    {
        Paint(field, borderNormal);

        if (label != null) label.text = "";

        if (group != null) group.SetActive(false);
        else if (label != null) label.gameObject.SetActive(false);
    }

    void Paint(TMP_InputField field, Color border)
    {
        if (field == null) return;

        ColorBlock cb = field.colors;
        cb.normalColor = border;
        cb.highlightedColor = Color.Lerp(border, Color.white, hoverLighten);
        cb.selectedColor = Color.Lerp(border, Color.white, hoverLighten);
        cb.pressedColor = Color.Lerp(border, Color.black, pressDarken);
        cb.disabledColor = border;
        cb.colorMultiplier = 1f;
        cb.fadeDuration = 0.1f;
        field.colors = cb;
    }

    bool CheckEmail(string s)
    {
        s = s.Trim();
        int at = s.IndexOf('@');
        return at > 0 && s.IndexOf('.', at) > at + 1 && !s.EndsWith(".");
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
}