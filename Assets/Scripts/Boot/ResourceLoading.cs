
using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class ResourceLoading : MonoBehaviour
{
    [SerializeField] private string backendUrl = "https://patu-salmon.onrender.com";

    public bool IsCheckDone { get; private set; }
    public bool IsSessionValid { get; private set; }

    private bool isChecking = false;

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
        Debug.Log("[ResourceLoading] Start ตรวจสอบ Session");
        StartCoroutine(CheckSession());
    }

    private IEnumerator CheckSession()
    {
        if (isChecking)
        {
            Debug.LogWarning("[ResourceLoading] มีการตรวจ Session อยู่แล้ว ข้ามรอบนี้");
            yield break;
        }

        isChecking = true;
        IsCheckDone = false;
        IsSessionValid = false;

        string accessToken = PlayerPrefs.GetString("AccessToken", "");
        string refreshToken = PlayerPrefs.GetString("RefreshToken", "");
        string savedUserId = PlayerPrefs.GetString("UserID", "");

        Debug.Log($"[ResourceLoading] AccessToken มีค่า: {!string.IsNullOrEmpty(accessToken)}");
        Debug.Log($"[ResourceLoading] RefreshToken มีค่า: {!string.IsNullOrEmpty(refreshToken)}");
        Debug.Log($"[ResourceLoading] UserID มีค่า: {!string.IsNullOrEmpty(savedUserId)}");

        if (string.IsNullOrEmpty(accessToken) ||
            string.IsNullOrEmpty(savedUserId))
        {
            Debug.LogWarning("[ResourceLoading] ไม่มี AccessToken หรือ UserID → signup");
            GoToLogin();
            isChecking = false;
            yield break;
        }

        // 1. ตรวจ Access Token
        Debug.Log("[ResourceLoading] GET /auth/me");

        using (UnityWebRequest request =
               UnityWebRequest.Get(backendUrl + "/auth/me"))
        {
            request.SetRequestHeader("Authorization", "Bearer " + accessToken);
            yield return request.SendWebRequest();

            Debug.Log($"[ResourceLoading] /auth/me HTTP {request.responseCode}");
            Debug.Log($"[ResourceLoading] /auth/me Result: {request.result}");

            if (request.result == UnityWebRequest.Result.Success)
            {
                AuthMeResponse response =
                    JsonUtility.FromJson<AuthMeResponse>(request.downloadHandler.text);

                if (response != null && !string.IsNullOrEmpty(response.user_id))
                {
                    Debug.Log($"[ResourceLoading] AccessToken ใช้ได้ UserID={response.user_id}");
                    PlayerPrefs.SetString("UserID", response.user_id);
                    PlayerPrefs.Save();

                    IsSessionValid = true;
                    IsCheckDone = true;
                    isChecking = false;
                    yield break;
                }

                Debug.LogWarning("[ResourceLoading] /auth/me สำเร็จ แต่ข้อมูล UserID ไม่ครบ");
            }
            else if (request.responseCode != 401 &&
                     request.responseCode != 403)
            {
                Debug.LogError($"[ResourceLoading] ตรวจ Session ไม่ได้: {request.error}, HTTP {request.responseCode}");
                Debug.LogError($"[ResourceLoading] Response: {request.downloadHandler.text}");

                // ปัญหาเครือข่าย/เซิร์ฟเวอร์ ไม่ลบ Session
                IsCheckDone = true;
                isChecking = false;
                yield break;
            }
            else
            {
                Debug.LogWarning("[ResourceLoading] AccessToken หมดอายุหรือไม่ถูกต้อง → ลอง Refresh");
            }
        }

        // 2. Refresh Token
        if (string.IsNullOrEmpty(refreshToken))
        {
            Debug.LogWarning("[ResourceLoading] ไม่มี RefreshToken → signup");
            GoToLogin();
            isChecking = false;
            yield break;
        }

        Debug.Log("[ResourceLoading] POST /auth/refresh เริ่มทำงาน");

        string json = JsonUtility.ToJson(
            new RefreshRequest { refresh_token = refreshToken });

        using (UnityWebRequest request =
               new UnityWebRequest(backendUrl + "/auth/refresh", "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(
                Encoding.UTF8.GetBytes(json));
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            Debug.Log($"[ResourceLoading] /auth/refresh HTTP {request.responseCode}");
            Debug.Log($"[ResourceLoading] /auth/refresh Result: {request.result}");
            Debug.Log($"[ResourceLoading] /auth/refresh Response: {request.downloadHandler.text}");

            if (request.result != UnityWebRequest.Result.Success)
            {
                if (request.responseCode == 400 ||
                    request.responseCode == 401)
                {
                    Debug.LogWarning("[ResourceLoading] Refresh Token ใช้ไม่ได้หรือถูกใช้ไปแล้ว → signup");
                    GoToLogin();
                }
                else
                {
                    Debug.LogError("[ResourceLoading] Refresh ผิดพลาดจาก Server/Network จะไม่ลบ Token");
                    IsCheckDone = true;
                }

                isChecking = false;
                yield break;
            }

            RefreshResponse response =
                JsonUtility.FromJson<RefreshResponse>(request.downloadHandler.text);

            if (response == null ||
                string.IsNullOrEmpty(response.access_token) ||
                string.IsNullOrEmpty(response.refresh_token))
            {
                Debug.LogError("[ResourceLoading] Refresh สำเร็จแต่ไม่มี Token ครบ");
                IsCheckDone = true;
                isChecking = false;
                yield break;
            }

            PlayerPrefs.SetString("AccessToken", response.access_token);
            PlayerPrefs.SetString("RefreshToken", response.refresh_token);

            if (!string.IsNullOrEmpty(response.user_id))
                PlayerPrefs.SetString("UserID", response.user_id);

            if (!string.IsNullOrEmpty(response.email))
                PlayerPrefs.SetString("Email", response.email);

            PlayerPrefs.Save();

            Debug.Log("[ResourceLoading] Refresh สำเร็จ บันทึก Token ชุดใหม่แล้ว");
            IsSessionValid = true;
            IsCheckDone = true;
            isChecking = false;
        }
    }

    private void GoToLogin()
    {
        Debug.LogWarning("[ResourceLoading] ลบ Session ที่บันทึกไว้ → signup");

        PlayerPrefs.DeleteKey("AccessToken");
        PlayerPrefs.DeleteKey("RefreshToken");
        PlayerPrefs.DeleteKey("access_token");
        PlayerPrefs.DeleteKey("refresh_token");
        PlayerPrefs.DeleteKey("UserID");
        PlayerPrefs.Save();

        IsSessionValid = false;
        IsCheckDone = true;
    }
}