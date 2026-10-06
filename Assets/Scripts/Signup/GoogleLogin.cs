
using System;
using System.Collections;
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
    private string backendUrl = "http://10.0.2.2:8000";

    private void Awake()
    {
        Application.deepLinkActivated += HandleDeepLink;

        Debug.Log("GoogleLogin Awake");

        if (!string.IsNullOrEmpty(Application.absoluteURL))
        {
            Debug.Log("Initial Deep Link: " + Application.absoluteURL);
            HandleDeepLink(Application.absoluteURL);
        }
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

            Debug.Log("Backend Response:");
            Debug.Log(request.downloadHandler.text);

            GoogleLoginResponse response =
                JsonUtility.FromJson<GoogleLoginResponse>(
                    request.downloadHandler.text
                );

            if (response == null)
            {
                Debug.LogError("Cannot parse login response");
                yield break;
            }

            if (string.IsNullOrEmpty(response.login_url))
            {
                Debug.LogError("Login URL is empty");
                yield break;
            }

            Debug.Log("Opening Google Login:");
            Debug.Log(response.login_url);

            Application.OpenURL(response.login_url);
        }
    }

    private void HandleDeepLink(string url)
    {
        Debug.Log("Received Deep Link:");
        Debug.Log(url);

        if (!url.StartsWith("platusalmon://login-callback"))
        {
            Debug.LogWarning("Unknown deep link");
            return;
        }

        string accessToken = GetQueryParameter(
            url,
            "access_token"
        );

        string refreshToken = GetQueryParameter(
            url,
            "refresh_token"
        );

        if (string.IsNullOrEmpty(accessToken))
        {
            Debug.LogError("Access token not found");
            return;
        }

        PlayerPrefs.SetString(
            "access_token",
            accessToken
        );

        if (!string.IsNullOrEmpty(refreshToken))
        {
            PlayerPrefs.SetString(
                "refresh_token",
                refreshToken
            );
        }

        PlayerPrefs.Save();

        Debug.Log("Google Login Success!");
        Debug.Log("Access Token saved.");

        OnLoginSuccess();
    }

    private string GetQueryParameter(
        string url,
        string parameter
    )
    {
        try
        {
            Uri uri = new Uri(url);

            string query = uri.Query;

            if (string.IsNullOrEmpty(query))
            {
                return null;
            }

            query = query.TrimStart('?');

            string[] parameters = query.Split('&');

            foreach (string param in parameters)
            {
                string[] pair = param.Split(
                    new char[] { '=' },
                    2
                );

                if (pair.Length != 2)
                {
                    continue;
                }

                string key = Uri.UnescapeDataString(
                    pair[0]
                );

                string value = Uri.UnescapeDataString(
                    pair[1]
                );

                if (key == parameter)
                {
                    return value;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError(
                "Deep Link Parse Error: " + e.Message
            );
        }

        return null;
    }

    private void OnLoginSuccess()
    {
        Debug.Log("User logged in!");

        SceneManager.LoadScene("SelectCharacter");
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
            return;

        string url = Application.absoluteURL;

        if (!string.IsNullOrEmpty(url))
        {
            Debug.Log("Application Focus Deep Link: " + url);
            HandleDeepLink(url);
        }
    }
}
