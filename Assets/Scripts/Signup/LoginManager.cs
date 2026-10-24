
using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class LoginManager : MonoBehaviour
{
    private const string backendUrl = "https://patu-salmon.onrender.com";

    private static LoginManager instance;

    public static LoginManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("LoginManager");
                instance = go.AddComponent<LoginManager>();
            }

            return instance;
        }
    }

    public static string UserId =>
        PlayerPrefs.GetString("UserID", "");


    public static bool HasAccessToken()
    {
        return !string.IsNullOrEmpty(
            PlayerPrefs.GetString("AccessToken", "")
        );
    }

    private void Awake()
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

    public void Continue(
        string email,
        string password,
        Action<AuthResponse> onSuccess,
        Action<string> onError)
    {
        string json = JsonUtility.ToJson(
            new AuthRequest
            {
                email = email,
                password = password
            }
        );

        StartCoroutine(PostJson(
            "/auth/continue",
            json,
            text =>
            {
                AuthResponse res =
                    JsonUtility.FromJson<AuthResponse>(text);

                if (res == null || string.IsNullOrEmpty(res.user_id))
                {
                    onError?.Invoke("Response is missing user_id.");
                    return;
                }

                SaveSession(res);

                GameLogger.Info(
                    "LoginManager",
                    "Continue OK. UserID=" + res.user_id +
                    " | New=" + res.is_new_user +
                    " | HasProfile=" + res.has_profile
                );

                onSuccess?.Invoke(res);
            },
            onError
        ));
    }

    /// <summary>
    /// ส่งข้อมูลผู้เล่นเริ่มต้นไป Backend
    /// เรียกใช้เฉพาะเมื่อผู้เล่นยังไม่มี Profile
    /// </summary>
    public void SendStarterPlayer(
        string username,
        string character,
        string language,
        Action onSuccess,
        Action<string> onError)
    {
        string userId = UserId;

        if (string.IsNullOrEmpty(userId))
        {
            onError?.Invoke("User ID is missing. Please log in again.");
            return;
        }

        string json = JsonUtility.ToJson(
            new StarterPlayerData
            {
                user_id = userId,
                username = username,
                character = character,
                language = language
            }
        );

        StartCoroutine(PostJson(
            "/player/starter",
            json,
            _ => onSuccess?.Invoke(),
            onError
        ));
    }

    // ---------- Player Stats ----------

    public void GetPlayerStats(
        Action<PlayerStatsResponse> onSuccess,
        Action<string> onError)
    {
        string userId = UserId;
        string token = PlayerPrefs.GetString("AccessToken", "");

        if (string.IsNullOrEmpty(userId))
        {
            onError?.Invoke("User ID is missing. Please log in again.");
            return;
        }

        if (string.IsNullOrEmpty(token))
        {
            onError?.Invoke("Access token is missing. Please log in again.");
            return;
        }

        StartCoroutine(GetPlayerStatsRequest(
            userId,
            token,
            onSuccess,
            onError
        ));
    }

    private IEnumerator GetPlayerStatsRequest(
        string userId,
        string token,
        Action<PlayerStatsResponse> onSuccess,
        Action<string> onError)
    {
        string url = backendUrl + "/player/stats/" +
                     UnityWebRequest.EscapeURL(userId);

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader(
                "Authorization",
                "Bearer " + token
            );

            yield return request.SendWebRequest();

            string body = request.downloadHandler.text;

            if (request.result != UnityWebRequest.Result.Success ||
                request.responseCode < 200 ||
                request.responseCode >= 300)
            {
                string error = string.IsNullOrEmpty(body)
                    ? request.error
                    : body;

                onError?.Invoke(
                    "Could not load player stats: " + error
                );

                yield break;
            }

            PlayerStatsResponse stats =
                JsonUtility.FromJson<PlayerStatsResponse>(body);

            if (stats == null ||
                string.IsNullOrEmpty(stats.user_id))
            {
                onError?.Invoke(
                    "Invalid player stats response."
                );

                yield break;
            }

            onSuccess?.Invoke(stats);
        }
    }

    // ---------- HTTP ----------

    private IEnumerator PostJson(
        string path,
        string json,
        Action<string> onDone,
        Action<string> onError)
    {
        using (UnityWebRequest request =
               new UnityWebRequest(backendUrl + path, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(
                Encoding.UTF8.GetBytes(json)
            );

            request.downloadHandler =
                new DownloadHandlerBuffer();

            request.SetRequestHeader(
                "Content-Type",
                "application/json"
            );

            yield return request.SendWebRequest();

            string body = request.downloadHandler.text;

            if (request.result != UnityWebRequest.Result.Success ||
                request.responseCode < 200 ||
                request.responseCode >= 300)
            {
                string error = ExtractError(request);

                onError?.Invoke(
                    string.IsNullOrEmpty(error) ? body : error
                );

                yield break;
            }

            onDone?.Invoke(body);
        }
    }

    private static string ExtractError(UnityWebRequest request)
    {
        try
        {
            ErrorBody body = JsonUtility.FromJson<ErrorBody>(
                request.downloadHandler.text
            );

            if (body != null &&
                !string.IsNullOrEmpty(body.detail))
            {
                return body.detail;
            }
        }
        catch
        {
            // ใช้ข้อความ Error สำรอง
        }

        return request.error;
    }

    // ---------- Session ----------

    private static void SaveSession(AuthResponse res)
    {
        PlayerPrefs.SetString("UserID", res.user_id);

        if (!string.IsNullOrEmpty(res.email))
        {
            PlayerPrefs.SetString("Email", res.email);
        }

        if (!string.IsNullOrEmpty(res.access_token))
        {
            PlayerPrefs.SetString("AccessToken", res.access_token);
            PlayerPrefs.SetString(
                "RefreshToken",
                res.refresh_token ?? ""
            );
        }

        PlayerPrefs.Save();
    }

    // ---------- DTOs ----------

    [Serializable]
    private class AuthRequest
    {
        public string email;
        public string password;
    }

    [Serializable]
    private class ErrorBody
    {
        public string detail;
    }

    [Serializable]
    private class StarterPlayerData
    {
        public string user_id;
        public string username;
        public string character;
        public string language;
    }

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

    [Serializable]
    public class PlayerStatsResponse
    {
        public string message;
        public string user_id;
        public int coin;
        public int work_days;
        public int work_streak;
        public int total_customers_served;
    }
}