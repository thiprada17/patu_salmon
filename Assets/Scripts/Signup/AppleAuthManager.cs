using System;
using System.Collections;
using System.Security.Cryptography;
using System.Text;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public sealed class AppleAuthManager : MonoBehaviour
{
#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void PlatuAppleSignIn(string gameObjectName, string rawNonce);
#endif

    [SerializeField] private AppleAuthConfig config;
    [SerializeField] private LanguagePopup languagePopup;

    private string pendingNonce;
    public bool IsLoggedIn { get; private set; }

    [Serializable]
    private sealed class AppleLoginRequest
    {
        public string identity_token;
        public string nonce;
    }

    [Serializable]
    private sealed class AppleLoginResponse
    {
        public string user_id;
        public string email;
        public string access_token;
        public string refresh_token;
        public bool has_profile;
        public bool is_new_user;
        public bool mock_mode;
        public string detail;
    }

    [Serializable]
    private sealed class NativeAppleResult
    {
        public string identity_token;
        public string email;
        public string given_name;
        public string family_name;
        public string error;
    }

    public void LoginWithApple()
    {
        if (config == null)
        {
            Debug.LogError("Apple Auth: Assign AppleAuthConfig in the Inspector.");
            return;
        }

#if UNITY_EDITOR
        if (config.UseMockInEditor)
        {
            pendingNonce = CreateNonce();
            StartCoroutine(SubmitIdentityToken(CreateMockIdentityToken(pendingNonce), pendingNonce));
            return;
        }
#endif

#if UNITY_IOS && !UNITY_EDITOR
        pendingNonce = CreateNonce();
        PlatuAppleSignIn(gameObject.name, pendingNonce);
#else
        Debug.LogWarning("Sign in with Apple is available in iOS builds. Enable Editor mock mode to test without Apple credentials.");
#endif
    }

#if UNITY_EDITOR
    // Exposes the real submit/session path to Unity Editor integration tests.
    public IEnumerator SubmitIdentityTokenForEditorTest(string identityToken, string nonce)
    {
        yield return SubmitIdentityToken(identityToken, nonce, navigateAfterLogin: false);
    }
#endif

    // Called from the AuthenticationServices native iOS plugin.
    public void OnNativeAppleSignIn(string json)
    {
        NativeAppleResult result = JsonUtility.FromJson<NativeAppleResult>(json);
        if (result == null || !string.IsNullOrEmpty(result.error) || string.IsNullOrEmpty(result.identity_token))
        {
            Debug.LogError("Apple Auth failed: " + (result != null ? result.error : "Native response was empty."));
            return;
        }

        StartCoroutine(SubmitIdentityToken(result.identity_token, pendingNonce));
    }

    private IEnumerator SubmitIdentityToken(
        string identityToken,
        string nonce,
        bool navigateAfterLogin = true)
    {
        AppleLoginRequest body = new AppleLoginRequest
        {
            identity_token = identityToken,
            nonce = nonce,
        };

        using (UnityWebRequest request = new UnityWebRequest(BackendConfig.BaseUrl + "/auth/apple", "POST"))
        {
            request.timeout = 15;
            request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(JsonUtility.ToJson(body)));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            UnityWebRequestAsyncOperation operation = request.SendWebRequest();
            while (!operation.isDone)
                yield return null;

            AppleLoginResponse response = null;
            if (!string.IsNullOrEmpty(request.downloadHandler.text))
                response = JsonUtility.FromJson<AppleLoginResponse>(request.downloadHandler.text);

            if (request.result != UnityWebRequest.Result.Success || response == null || string.IsNullOrEmpty(response.access_token))
            {
                Debug.LogError(
                    "Apple Auth request failed. Result=" + request.result +
                    ", HTTP=" + request.responseCode +
                    ", URL=" + request.url +
                    ", Error=" + request.error +
                    ", Body=" + request.downloadHandler.text);
                yield break;
            }

            PlayerPrefs.SetString("UserID", response.user_id ?? "");
            PlayerPrefs.SetString("AccessToken", response.access_token);
            PlayerPrefs.SetString("RefreshToken", response.refresh_token ?? "");
            if (!string.IsNullOrEmpty(response.email))
                PlayerPrefs.SetString("Email", response.email);
            PlayerPrefs.SetInt("IsMockAppleAuth", response.mock_mode ? 1 : 0);
            PlayerPrefs.SetInt("IsNewUser", response.is_new_user ? 1 : 0);
            PlayerPrefs.SetInt("HasProfile", response.has_profile ? 1 : 0);
            PlayerPrefs.Save();
            IsLoggedIn = true;

            if (!navigateAfterLogin)
                yield break;

            if (response.is_new_user || !response.has_profile)
            {
                if (languagePopup == null)
                {
                    Debug.LogError("Apple Auth: LanguagePopup is not assigned.");
                    yield break;
                }

                languagePopup.gameObject.SetActive(true);
                languagePopup.Open();
                yield break;
            }

            LoginManager.Instance.GetPlayerStats(
                stats =>
                {
                    if (stats == null)
                    {
                        Debug.LogError("Apple Auth: Player stats response is empty.");
                        return;
                    }

                    TutorialSession.Initialize(stats.work_days);
                    SceneManager.LoadScene(stats.work_days == 0 ? "map" : "loading");
                },
                error => Debug.LogError("Apple Auth: Could not load player stats: " + error)
            );
        }
    }

#if UNITY_EDITOR
    private string CreateMockIdentityToken(string nonce)
    {
        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        string hashedNonce = Sha256Hex(nonce);
        string header = "{\"alg\":\"none\",\"typ\":\"JWT\"}";
        string payload = "{\"iss\":\"https://appleid.apple.com.mock\",\"aud\":" + JsonString(config.AppleClientId) +
                         ",\"sub\":\"platu-salmon-editor-user\",\"email\":\"apple-mock@example.invalid\",\"nonce\":" +
                         JsonString(hashedNonce) + ",\"iat\":" + now + ",\"exp\":" + (now + 86400 * 30) + "}";
        return Base64Url(Encoding.UTF8.GetBytes(header)) + "." +
               Base64Url(Encoding.UTF8.GetBytes(payload)) + ".";
    }
#endif

    private static string CreateNonce()
    {
        return Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
    }

    private static string Sha256Hex(string value)
    {
        using (SHA256 sha = SHA256.Create())
        {
            byte[] hash = sha.ComputeHash(Encoding.UTF8.GetBytes(value));
            StringBuilder result = new StringBuilder(hash.Length * 2);
            foreach (byte item in hash)
                result.Append(item.ToString("x2"));
            return result.ToString();
        }
    }

    private static string Base64Url(byte[] value)
    {
        return Convert.ToBase64String(value)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static string JsonString(string value)
    {
        return "\"" + value.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
    }
}
