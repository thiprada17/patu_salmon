
using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class ResourceLoading : MonoBehaviour
{
    [Header("Backend")]
    [SerializeField]
    private string backendUrl = "https://patu-salmon.onrender.com";


    public bool IsCheckDone { get; private set; }
    public bool IsSessionValid { get; private set; }

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
               UnityWebRequest.Get(backendUrl + "/auth/me"))
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
               new UnityWebRequest(backendUrl + "/auth/refresh", "POST"))
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

            IsSessionValid = true;
            IsCheckDone = true;
            isChecking = false;

            GoToLoading();
        }
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

        PlayerPrefs.Save();

        IsSessionValid = false;
        IsCheckDone = true;
        isChecking = false;
    }
}