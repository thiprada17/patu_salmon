using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

/// ที่เดียวที่คุยกับ Backend (continue / signup / starter player)
/// สร้างตัวเองอัตโนมัติและอยู่ข้ามซีน ไม่ต้องวางใน Scene
public class LoginManager : MonoBehaviour
{
    const string DefaultBackendUrl = "http://127.0.0.1:8000";

    static LoginManager instance;
    public static LoginManager Instance
    {
        get
        {
            if (instance == null)
            {
                var go = new GameObject("LoginManager");
                instance = go.AddComponent<LoginManager>();
            }
            return instance;
        }
    }

    [SerializeField] string backendUrl = DefaultBackendUrl;

    public static string UserId => PlayerPrefs.GetString("UserID", "");

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ---------- Public API ----------

    /// ปุ่มเดียว: Backend ตรวจเองว่าบัญชีเก่าหรือใหม่ (POST /auth/continue)
    public void Continue(string email, string password,
                         Action<AuthResponse> onSuccess, Action<string> onError)
    {
        string json = JsonUtility.ToJson(new AuthRequest { email = email, password = password });

        StartCoroutine(PostJson("/auth/continue", json,
            text =>
            {
                var res = JsonUtility.FromJson<AuthResponse>(text);
                if (res == null || string.IsNullOrEmpty(res.user_id))
                {
                    onError?.Invoke("Response ไม่มี user_id");
                    return;
                }

                SaveSession(res);
                onSuccess?.Invoke(res);
            },
            onError));
    }

    public void SendStarterPlayer(string username, string character,
                                  Action onSuccess, Action<string> onError)
    {
        string json = JsonUtility.ToJson(new StarterPlayerData
        {
            userID = UserId,
            username = username,
            character = character
        });

        Debug.Log("Sending player data: " + json);
        StartCoroutine(PostJson("/player/starter", json,
            _ => onSuccess?.Invoke(),
            onError));
    }

    // ---------- Internals ----------

    IEnumerator PostJson(string path, string json,
                         Action<string> onDone, Action<string> onError)
    {
        Debug.LogWarning("[API] Sending: " + path);

        using (var request = new UnityWebRequest(backendUrl + path, "POST"))
        {
            request.uploadHandler =
                new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            string body = request.downloadHandler.text;

            Debug.LogWarning("[API] HTTP Status: " + request.responseCode);
            Debug.LogWarning("[API] Response Body: " + body);
            Debug.LogWarning("[API] Error: " + request.error);

            if (request.responseCode < 200 || request.responseCode >= 300)
            {
                onError?.Invoke(body);
                yield break;
            }

            onDone?.Invoke(body);
        }
    }

    // FastAPI ตอบ error เป็น {"detail": "..."} -> ดึงข้อความมาแสดง
    static string ExtractError(UnityWebRequest request)
    {
        try
        {
            var body = JsonUtility.FromJson<ErrorBody>(request.downloadHandler.text);
            if (body != null && !string.IsNullOrEmpty(body.detail)) return body.detail;
        }
        catch { /* detail อาจเป็น list (422) ให้ fallback */ }

        return request.error;
    }

    static void SaveSession(AuthResponse res)
    {
        PlayerPrefs.SetString("UserID", res.user_id);
        if (!string.IsNullOrEmpty(res.email)) PlayerPrefs.SetString("Email", res.email);
        if (!string.IsNullOrEmpty(res.access_token))
        {
            PlayerPrefs.SetString("AccessToken", res.access_token);
            PlayerPrefs.SetString("RefreshToken", res.refresh_token ?? "");
        }
        PlayerPrefs.Save();
    }

    // ---------- DTOs ----------

    [Serializable] class AuthRequest { public string email; public string password; }
    [Serializable] class ErrorBody { public string detail; }
    [Serializable] class StarterPlayerData { public string userID; public string username; public string character; }

    [Serializable]
    public class AuthResponse
    {
        public string message;
        public string user_id;
        public string email;
        public string access_token;
        public string refresh_token;
        public bool is_new_user;
        public bool has_profile;
        public bool needs_email_confirmation;
    }
}