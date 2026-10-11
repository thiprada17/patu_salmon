using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class GoogleLoginResponse
{
    public string login_url;
}

public class GoogleLogin : MonoBehaviour
{
    [SerializeField] private LanguagePopup languagePopup;

    private string backendUrl = "http://127.0.0.1:8000";
    private string lastHandledUrl = null;

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
        StartCoroutine(StartGoogleLogin());
    }

    private IEnumerator StartGoogleLogin()
    {
        string url = backendUrl + "/auth/google";
        Debug.Log("Request Google Login: " + url);

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Google Login Error: " + request.error);
                yield break;
            }

            GoogleLoginResponse response =
                JsonUtility.FromJson<GoogleLoginResponse>(request.downloadHandler.text);

            if (response == null || string.IsNullOrEmpty(response.login_url))
            {
                Debug.LogError("Login URL is empty");
                yield break;
            }

            Application.OpenURL(response.login_url);
        }
    }

    private void HandleDeepLink(string url)
    {
        if (string.IsNullOrEmpty(url) || url == lastHandledUrl) return;

        Debug.Log("Received Deep Link: " + url);

        if (!url.StartsWith("platusalmon://login-callback"))
        {
            Debug.LogWarning("Unknown deep link");
            return;
        }

        string accessToken = GetQueryParameter(url, "access_token");
        string refreshToken = GetQueryParameter(url, "refresh_token");

        if (string.IsNullOrEmpty(accessToken))
        {
            Debug.LogError("Access token not found");
            return;
        }

        lastHandledUrl = url;

        PlayerPrefs.SetString("access_token", accessToken);
        if (!string.IsNullOrEmpty(refreshToken))
            PlayerPrefs.SetString("refresh_token", refreshToken);
        PlayerPrefs.Save();

        Debug.Log("Google Login Success! Token saved.");

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

            if (response == null || string.IsNullOrEmpty(response.id))
            {
                Debug.LogError("Backend response ไม่มี user ID");
                yield break;
            }

            PlayerPrefs.SetString("UserID", response.id);

            if (!string.IsNullOrEmpty(response.email))
                PlayerPrefs.SetString("Email", response.email);

            PlayerPrefs.Save();

            Debug.Log("Google UserID saved: " + response.id);
        }
    }

}


