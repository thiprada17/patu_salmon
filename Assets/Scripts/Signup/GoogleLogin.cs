using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class GoogleLoginResponse
{
    public string login_url;
}

public class GoogleLogin : MonoBehaviour
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern int OpenGoogleAuthWindow();

    [DllImport("__Internal")]
    private static extern void NavigateGoogleAuthWindow(string url);

    [DllImport("__Internal")]
    private static extern void CloseGoogleAuthWindow();
#endif
    [SerializeField] private LanguagePopup languagePopup;

    private string backendUrl = "https://patu-salmon.onrender.com";
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

        // Open synchronously inside the button click so the browser allows the OAuth tab.
        if (OpenGoogleAuthWindow() == 0)
        {
            Debug.LogError("Google Login: Browser blocked the sign-in window.");
            return;
        }
#endif
        loginRequestPending = true;
        StartCoroutine(StartGoogleLogin());
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
        string url = backendUrl + "/auth/google?platform=" + platform;
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

#if UNITY_WEBGL && !UNITY_EDITOR
            if (!IsSafeGoogleLoginUrl(response.login_url))
            {
                Debug.LogError("Google Login: Backend returned an invalid authorization URL.");
                loginRequestPending = false;
                CloseLoginWindow();
                yield break;
            }

            NavigateGoogleAuthWindow(response.login_url);
#else
            Application.OpenURL(response.login_url);
#endif
            loginRequestPending = false;
        }
    }

    private bool IsSafeGoogleLoginUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out Uri uri) &&
               uri.Scheme == Uri.UriSchemeHttps &&
               uri.AbsolutePath.IndexOf("/auth/v1/authorize", StringComparison.OrdinalIgnoreCase) >= 0;
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
    }

    public void HandleOAuthMessage(string message)
    {
        AuthCallbackResponse response = JsonUtility.FromJson<AuthCallbackResponse>(message);
        if (response == null || response.type != "platu-google-login" || string.IsNullOrEmpty(response.access_token))
        {
            Debug.LogError("Google Login: Invalid browser callback message.");
            return;
        }

        SaveLoginTokens(response.access_token, response.refresh_token);
    }

    private void HandleDeepLink(string url)
    {
        if (string.IsNullOrEmpty(url) || url == lastHandledUrl) return;

        Debug.Log("Received Deep Link: " + url);
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
        string url = backendUrl + "/auth/callback?code=" + UnityWebRequest.EscapeURL(code);
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

            SaveLoginTokens(response.access_token, response.refresh_token);
        }
    }

    private void SaveLoginTokens(string accessToken, string refreshToken)
    {
        PlayerPrefs.SetString("AccessToken", accessToken);
        if (!string.IsNullOrEmpty(refreshToken))
            PlayerPrefs.SetString("RefreshToken", refreshToken);
        PlayerPrefs.Save();
        Debug.Log("Google Login Success! Tokens saved.");
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

        OnLoginSuccess();
    }


    private string GetQueryParameter(string url, string parameter)
    {
        try
        {
            Uri uri = new Uri(url);
            string query = uri.Query;
            if (string.IsNullOrEmpty(query)) return null;

            foreach (string param in query.TrimStart('?').Split('&'))
            {
                string[] pair = param.Split(new char[] { '=' }, 2);
                if (pair.Length != 2) continue;

                if (Uri.UnescapeDataString(pair[0]) == parameter)
                    return Uri.UnescapeDataString(pair[1]);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Deep Link Parse Error: " + e.Message);
        }

        return null;
    }
    private void OnLoginSuccess()
    {
        Debug.Log("User logged in!");

        if (languagePopup == null)
        {
            Debug.LogError("languagePopup = NULL");
            return;
        }

        StartCoroutine(OpenPopupNextFrame());
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
        string url = backendUrl + "/auth/me";

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


