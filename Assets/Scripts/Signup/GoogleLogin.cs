using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

[Serializable]
public class GoogleLoginResponse
{
    public string login_url;
}

public class GoogleLogin : MonoBehaviour
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern int OpenGoogleAuthWindow(string url);

    [DllImport("__Internal")]
    private static extern void CloseGoogleAuthWindow();
#endif
    [SerializeField] private LanguagePopup languagePopup;

    private string lastHandledUrl = null;
    private bool loginRequestPending;

    private void Awake()
    {
        Application.deepLinkActivated += HandleDeepLink;

        if (!string.IsNullOrEmpty(Application.absoluteURL))
            HandleDeepLink(Application.absoluteURL);
    }

    private void OnDestroy()
    {
        Application.deepLinkActivated -= HandleDeepLink;
    }

    public void LoginWithGoogle()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (loginRequestPending)
            return;

        // Open and navigate the OAuth tab synchronously in the button click.
        loginRequestPending = true;
        if (OpenGoogleAuthWindow(BackendConfig.BaseUrl + "/auth/google/redirect?platform=web") == 0)
            Debug.LogError("Google Login: Browser blocked or could not navigate the sign-in window.");
        loginRequestPending = false;
        return;
#else
        loginRequestPending = true;
        StartCoroutine(StartGoogleLogin());
#endif
    }

    private IEnumerator StartGoogleLogin()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        const string platform = "web";
#elif UNITY_IOS
        const string platform = "ios";
#elif UNITY_ANDROID
        const string platform = "android";
#else
        const string platform = "web";
#endif
        string url = BackendConfig.BaseUrl + "/auth/google?platform=" + platform;
        Debug.Log("Request Google Login: " + url);

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Google Login Error: " + request.error);
                loginRequestPending = false;
                CloseLoginWindow();
                yield break;
            }

            GoogleLoginResponse response =
                JsonUtility.FromJson<GoogleLoginResponse>(request.downloadHandler.text);

            if (response == null || string.IsNullOrEmpty(response.login_url))
            {
                Debug.LogError("Login URL is empty");
                loginRequestPending = false;
                CloseLoginWindow();
                yield break;
            }

            Application.OpenURL(response.login_url);
            loginRequestPending = false;
        }
    }

    private void CloseLoginWindow()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        CloseGoogleAuthWindow();
#endif
    }

    [Serializable]
    private class AuthCallbackResponse
    {
        public string type;
        public string access_token;
        public string refresh_token;
        public string user_id;
        public string email;
        public bool has_profile;
        public bool is_new_user;
    }

    public void HandleOAuthMessage(string message)
    {
        AuthCallbackResponse response = JsonUtility.FromJson<AuthCallbackResponse>(message);
        if (response == null || response.type != "platu-google-login" || string.IsNullOrEmpty(response.access_token))
        {
            Debug.LogError("Google Login: Invalid browser callback message.");
            return;
        }

        SaveLoginTokens(response.access_token, response.refresh_token, response.user_id, response.email, response.has_profile, response.is_new_user);
    }

    private void HandleDeepLink(string url)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        // WebGL absoluteURL is the hosted game page (for example localhost or itch.io),
        // not a native deep link. WebGL OAuth returns through HandleOAuthMessage instead.
        return;
#endif
        if (string.IsNullOrEmpty(url) || url == lastHandledUrl) return;

        Debug.Log("Received app auth deep link.");
        if (!url.StartsWith("platusalmon://login-callback", StringComparison.OrdinalIgnoreCase))
        {
            Debug.LogWarning("Unknown deep link");
            return;
        }

        string code = GetQueryParameter(url, "code");
        if (!string.IsNullOrEmpty(code))
        {
            lastHandledUrl = url;
            StartCoroutine(ExchangeOAuthCode(code));
            return;
        }

        string accessToken = GetQueryParameter(url, "access_token");
        string refreshToken = GetQueryParameter(url, "refresh_token");
        if (string.IsNullOrEmpty(accessToken))
        {
            Debug.LogError("Access token or authorization code not found");
            return;
        }

        lastHandledUrl = url;
        SaveLoginTokens(accessToken, refreshToken);
    }

    private IEnumerator ExchangeOAuthCode(string code)
    {
        string url = BackendConfig.BaseUrl + "/auth/callback?code=" + UnityWebRequest.EscapeURL(code);
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Google OAuth callback exchange failed: " + request.error + " " + request.downloadHandler.text);
                yield break;
            }

            AuthCallbackResponse response = JsonUtility.FromJson<AuthCallbackResponse>(request.downloadHandler.text);
            if (response == null || string.IsNullOrEmpty(response.access_token))
            {
                Debug.LogError("Google OAuth callback did not return an access token.");
                yield break;
            }

            SaveLoginTokens(response.access_token, response.refresh_token, response.user_id, response.email, response.has_profile, response.is_new_user);
        }
    }

    private void SaveLoginTokens(
        string accessToken,
        string refreshToken,
        string userId = null,
        string email = null,
        bool hasProfile = false,
        bool isNewUser = true)
    {
        PlayerPrefs.SetString("AccessToken", accessToken);
        if (!string.IsNullOrEmpty(refreshToken))
            PlayerPrefs.SetString("RefreshToken", refreshToken);

        if (!string.IsNullOrEmpty(userId))
            PlayerPrefs.SetString("UserID", userId);
        else
            PlayerPrefs.DeleteKey("UserID");

        if (!string.IsNullOrEmpty(email))
            PlayerPrefs.SetString("Email", email);

        PlayerPrefs.Save();
        Debug.Log("Google Login Success! Tokens and user identity saved.");

        if (!string.IsNullOrEmpty(userId))
            OnLoginSuccess(hasProfile, isNewUser);
        else
            StartCoroutine(GetUserIdAndOpenPopup(accessToken));
    }

    private IEnumerator GetUserIdAndOpenPopup(string accessToken)
    {
        yield return StartCoroutine(GetUserId(accessToken));

        // ตรวจสอบว่าได้บันทึก UserID แล้วหรือไม่
        if (string.IsNullOrEmpty(PlayerPrefs.GetString("UserID", "")))
        {
            Debug.LogError("ไม่สามารถดึง UserID ได้");
            yield break;
        }

        OnLoginSuccess(false, true);
    }


    private string GetQueryParameter(string url, string parameter)
    {
        try
        {
            Uri uri = new Uri(url);
            string[] parameterSets = { uri.Query, uri.Fragment.TrimStart('#') };

            foreach (string parameterSet in parameterSets)
            {
                if (string.IsNullOrEmpty(parameterSet)) continue;

                foreach (string item in parameterSet.TrimStart('?', '#').Split('&'))
                {
                    string[] pair = item.Split(new char[] { '=' }, 2);
                    if (pair.Length != 2) continue;

                    string key = Uri.UnescapeDataString(pair[0].Replace("+", " "));
                    if (key == parameter)
                        return Uri.UnescapeDataString(pair[1].Replace("+", " "));
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Deep Link Parse Error: " + e.Message);
        }

        return null;
    }
    private void OnLoginSuccess(bool hasProfile, bool isNewUser)
    {
        PlayerPrefs.SetInt("IsNewUser", isNewUser ? 1 : 0);
        PlayerPrefs.SetInt("HasProfile", hasProfile ? 1 : 0);
        PlayerPrefs.Save();

        Debug.Log("Google Login Success. has_profile=" + hasProfile + ", is_new_user=" + isNewUser);

        if (isNewUser || !hasProfile)
        {
            if (languagePopup == null)
            {
                Debug.LogError("languagePopup = NULL");
                return;
            }

            StartCoroutine(OpenPopupNextFrame());
            return;
        }

        LoginManager.Instance.GetPlayerStats(
            stats =>
            {
                if (stats == null)
                {
                    Debug.LogError("Google Login: Player stats response is empty.");
                    return;
                }

                TutorialSession.Initialize(stats.work_days);
                SceneManager.LoadScene(stats.work_days == 0 ? "map" : "loading");
            },
            error => Debug.LogError("Google Login: Could not load player stats: " + error)
        );
    }

    private IEnumerator OpenPopupNextFrame()
    {
        yield return null;

        languagePopup.gameObject.SetActive(true);
        languagePopup.Open();

        Debug.Log("Popup Open called");
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus) return;

        string url = Application.absoluteURL;
        if (!string.IsNullOrEmpty(url))
            HandleDeepLink(url);
    }

    [System.Serializable]
    public class AuthMeResponse
    {
        public string id;
        public string user_id;
        public string email;
    }

    private IEnumerator GetUserId(string accessToken)
    {
        string url = BackendConfig.BaseUrl + "/auth/me";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader(
                "Authorization",
                "Bearer " + accessToken
            );

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Get user ID failed: " + request.error);
                Debug.LogError(request.downloadHandler.text);
                yield break;
            }

            AuthMeResponse response =
                JsonUtility.FromJson<AuthMeResponse>(
                    request.downloadHandler.text
                );

            if (response == null || (string.IsNullOrEmpty(response.id) && string.IsNullOrEmpty(response.user_id)))
            {
                Debug.LogError("Backend response ไม่มี user ID");
                yield break;
            }

            string userId = string.IsNullOrEmpty(response.user_id) ? response.id : response.user_id;
            PlayerPrefs.SetString("UserID", userId);

            if (!string.IsNullOrEmpty(response.email))
                PlayerPrefs.SetString("Email", response.email);

            PlayerPrefs.Save();

            Debug.Log("Google UserID saved: " + userId);
        }
    }

}


