
using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class ResourceLoading : MonoBehaviour
{
    public bool IsCheckDone { get; private set; }
    public bool IsSessionValid { get; private set; }
    public bool NeedsPlayerSetup { get; private set; }

    private bool isChecking;

    [Serializable]
    public class AuthMeResponse
    {
        public string user_id;
        public string email;
    }

    [Serializable]
    public class RefreshRequest
    {
        public string refresh_token;
    }

    [Serializable]
    public class RefreshResponse
    {
        public string user_id;
        public string email;
        public string access_token;
        public string refresh_token;
    }

    private void Start()
    {
        Debug.Log("[ResourceLoading] เริ่มตรวจสอบ Session");
        StartCoroutine(CheckSession());
    }

    private IEnumerator CheckSession()
    {
        if (isChecking)
            yield break;

        isChecking = true;
        IsCheckDone = false;
        IsSessionValid = false;

        string accessToken = PlayerPrefs.GetString("AccessToken", "");
        string refreshToken = PlayerPrefs.GetString("RefreshToken", "");
        string savedUserId = PlayerPrefs.GetString("UserID", "");

        Debug.Log(
            $"[ResourceLoading] AccessToken มีค่า: {!string.IsNullOrEmpty(accessToken)}"
        );

        Debug.Log(
            $"[ResourceLoading] RefreshToken มีค่า: {!string.IsNullOrEmpty(refreshToken)}"
        );

        Debug.Log(
            $"[ResourceLoading] UserID มีค่า: {!string.IsNullOrEmpty(savedUserId)}"
        );

        // ไม่มี Access Token หรือ UserID → Signup
        if (string.IsNullOrEmpty(accessToken) ||
            string.IsNullOrEmpty(savedUserId))
        {
            GoToSignup();
            yield break;
        }

        // 1. ตรวจสอบ Access Token
        Debug.Log("[ResourceLoading] GET /auth/me");

        using (UnityWebRequest request =
               UnityWebRequest.Get(BackendConfig.BaseUrl + "/auth/me"))
        {
            request.SetRequestHeader(
                "Authorization",
                "Bearer " + accessToken
            );

            yield return request.SendWebRequest();

            Debug.Log(
                $"[ResourceLoading] /auth/me HTTP {request.responseCode}"
            );

            Debug.Log(
                $"[ResourceLoading] /auth/me Result: {request.result}"
            );

            if (request.result == UnityWebRequest.Result.Success)
            {
                AuthMeResponse response =
                    JsonUtility.FromJson<AuthMeResponse>(
                        request.downloadHandler.text
                    );

                if (response != null &&
                    !string.IsNullOrEmpty(response.user_id))
                {
                    PlayerPrefs.SetString("UserID", response.user_id);

                    if (!string.IsNullOrEmpty(response.email))
                        PlayerPrefs.SetString("Email", response.email);

                    PlayerPrefs.Save();

                    Debug.Log(
                        $"[ResourceLoading] Token ใช้ได้ UserID={response.user_id}"
                    );

                    yield return RestorePlayerProfile(response.user_id, accessToken);

                    IsSessionValid = true;
                    IsCheckDone = true;
                    isChecking = false;

                    GoToLoading();
                    yield break;
                }

                Debug.LogWarning(
                    "[ResourceLoading] /auth/me สำเร็จ แต่ไม่มี UserID"
                );
            }
            else if (request.responseCode != 401 &&
                     request.responseCode != 403)
            {
                // Server หรือ Network มีปัญหา ไม่ลบ Session
                Debug.LogError(
                    $"[ResourceLoading] ตรวจ Session ไม่ได้: " +
                    $"{request.error}, HTTP {request.responseCode}"
                );

                Debug.LogError(
                    $"[ResourceLoading] Response: " +
                    $"{request.downloadHandler.text}"
                );

                IsCheckDone = true;
                isChecking = false;
                yield break;
            }
            else
            {
                Debug.LogWarning(
                    "[ResourceLoading] Access Token ใช้ไม่ได้ → ลอง Refresh"
                );
            }
        }

        // 2. ไม่มี Refresh Token → Signup
        if (string.IsNullOrEmpty(refreshToken))
        {
            GoToSignup();
            yield break;
        }

        // 3. ขอ Token ชุดใหม่
        Debug.Log("[ResourceLoading] POST /auth/refresh");

        string json = JsonUtility.ToJson(
            new RefreshRequest
            {
                refresh_token = refreshToken
            }
        );

        using (UnityWebRequest request =
               new UnityWebRequest(BackendConfig.BaseUrl + "/auth/refresh", "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(
                Encoding.UTF8.GetBytes(json)
            );

            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader(
                "Content-Type",
                "application/json"
            );

            yield return request.SendWebRequest();

            Debug.Log(
                $"[ResourceLoading] /auth/refresh HTTP {request.responseCode}"
            );

            Debug.Log(
                $"[ResourceLoading] /auth/refresh Result: {request.result}"
            );

            Debug.Log(
                $"[ResourceLoading] /auth/refresh Response: " +
                $"{request.downloadHandler.text}"
            );

            if (request.result != UnityWebRequest.Result.Success)
            {
                if (request.responseCode == 400 ||
                    request.responseCode == 401)
                {
                    Debug.LogWarning(
                        "[ResourceLoading] Refresh Token ใช้ไม่ได้ → Signup"
                    );

                    GoToSignup();
                }
                else
                {
                    // ปัญหา Server/Network ไม่ลบ Token
                    Debug.LogError(
                        "[ResourceLoading] Refresh ล้มเหลวจาก Server/Network"
                    );

                    IsCheckDone = true;
                    isChecking = false;
                }

                yield break;
            }

            RefreshResponse response =
                JsonUtility.FromJson<RefreshResponse>(
                    request.downloadHandler.text
                );

            if (response == null ||
                string.IsNullOrEmpty(response.access_token) ||
                string.IsNullOrEmpty(response.refresh_token))
            {
                Debug.LogError(
                    "[ResourceLoading] Refresh สำเร็จแต่ Token ไม่ครบ"
                );

                IsCheckDone = true;
                isChecking = false;
                yield break;
            }

            // บันทึก Token ชุดใหม่
            PlayerPrefs.SetString("AccessToken", response.access_token);
            PlayerPrefs.SetString("RefreshToken", response.refresh_token);

            if (!string.IsNullOrEmpty(response.user_id))
                PlayerPrefs.SetString("UserID", response.user_id);

            if (!string.IsNullOrEmpty(response.email))
                PlayerPrefs.SetString("Email", response.email);

            PlayerPrefs.Save();

            Debug.Log("[ResourceLoading] Refresh สำเร็จ");

            string refreshedUserId = string.IsNullOrWhiteSpace(response.user_id)
                ? PlayerPrefs.GetString("UserID", "")
                : response.user_id;
            yield return RestorePlayerProfile(refreshedUserId, response.access_token);

            IsSessionValid = true;
            IsCheckDone = true;
            isChecking = false;

            GoToLoading();
        }
    }

    private void CheckPlayerSetup()
    {
        string username = PlayerPrefs.GetString("Username", "").Trim();
        string character = PlayerPrefs.GetString("SelectedCharacter", "").Trim();
        string language = PlayerPrefs.GetString("Language", "").Trim().ToLowerInvariant();

        NeedsPlayerSetup = string.IsNullOrEmpty(username) ||
                           (character != "salmon" && character != "platu") ||
                           (language != "english" && language != "thai" && language != "japanese");

        if (NeedsPlayerSetup)
        {
            PlayerPrefs.SetInt("ResumePlayerSetup", 1);
            PlayerPrefs.SetInt("IsNewUser", 1);
            PlayerPrefs.SetInt("HasProfile", 0);
            Debug.LogWarning("[ResourceLoading] Player setup is incomplete. Returning to language and character setup.");
        }
        else if (!PlayerPrefs.HasKey("IsNewUser") || !PlayerPrefs.HasKey("HasProfile"))
        {
            string userId = PlayerPrefs.GetString("UserID", "");
            bool starterWasSaved = PlayerPrefs.GetString("StarterPlayerSavedUserID", "") == userId;
            PlayerPrefs.SetInt("IsNewUser", starterWasSaved ? 0 : 1);
            PlayerPrefs.SetInt("HasProfile", starterWasSaved ? 1 : 0);
        }

        PlayerPrefs.Save();
    }

    private IEnumerator RestorePlayerProfile(string userId, string accessToken)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            MarkSetupIncomplete();
            yield break;
        }

        string url = BackendConfig.BaseUrl + "/player/stats/" +
                     UnityWebRequest.EscapeURL(userId);
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("Authorization", "Bearer " + accessToken);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                LoginManager.PlayerStatsResponse profile =
                    JsonUtility.FromJson<LoginManager.PlayerStatsResponse>(request.downloadHandler.text);

                bool complete = profile != null
                    && !string.IsNullOrWhiteSpace(profile.username)
                    && !profile.username.StartsWith("Player_", StringComparison.OrdinalIgnoreCase)
                    && (profile.character == "salmon" || profile.character == "platu")
                    && (profile.language == "english" || profile.language == "thai" || profile.language == "japanese");

                if (complete)
                {
                    LoginManager.SavePlayerProfileLocally(profile);
                    PlayerPrefs.SetInt("IsNewUser", 0);
                    PlayerPrefs.SetInt("HasProfile", 1);
                    PlayerPrefs.DeleteKey("ResumePlayerSetup");
                    NeedsPlayerSetup = false;
                    PlayerPrefs.Save();
                    Debug.Log("[ResourceLoading] Restored saved player profile from server.");
                    yield break;
                }

                MarkSetupIncomplete();
                Debug.LogWarning("[ResourceLoading] Server profile is incomplete; player setup is required.");
                yield break;
            }

            // A temporary server/network failure must not be treated as a
            // missing profile; that would send returning players through setup.
            Debug.LogError(
                "[ResourceLoading] Could not restore player profile: " +
                request.error + " " + request.downloadHandler.text
            );
            NeedsPlayerSetup = false;
            if (!PlayerPrefs.HasKey("IsNewUser") || !PlayerPrefs.HasKey("HasProfile"))
            {
                PlayerPrefs.SetInt("IsNewUser", 0);
                PlayerPrefs.SetInt("HasProfile", 1);
            }
            PlayerPrefs.Save();
        }
    }

    private void MarkSetupIncomplete()
    {
        NeedsPlayerSetup = true;
        PlayerPrefs.SetInt("ResumePlayerSetup", 1);
        PlayerPrefs.SetInt("IsNewUser", 1);
        PlayerPrefs.SetInt("HasProfile", 0);
        PlayerPrefs.Save();
    }

    // มี Token ที่ผ่านการตรวจสอบ → Loading
    private void GoToLoading()
    {
        Debug.Log(
            $"[ResourceLoading] Session ถูกต้อง"
        );

    }

    // ไม่มี Session หรือ Refresh Token ใช้ไม่ได้ → Signup
    private void GoToSignup()
    {
        Debug.LogWarning(
            $"[ResourceLoading] ไม่มี Session ที่ใช้ได้"
        );

        PlayerPrefs.DeleteKey("AccessToken");
        PlayerPrefs.DeleteKey("RefreshToken");
        PlayerPrefs.DeleteKey("access_token");
        PlayerPrefs.DeleteKey("refresh_token");
        PlayerPrefs.DeleteKey("UserID");
        PlayerPrefs.DeleteKey("ResumePlayerSetup");

        PlayerPrefs.Save();

        IsSessionValid = false;
        IsCheckDone = true;
        isChecking = false;
    }
}
