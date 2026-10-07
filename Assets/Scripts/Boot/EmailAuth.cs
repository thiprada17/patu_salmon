using System;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

[Serializable]
public class EmailLoginRequest
{
    public string email;
    public string password;
}

[Serializable]
public class LoginResponse
{
    public string message;
    public string error;
    public string access_token;
    public string refresh_token;
}


public class EmailAuth : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private TMP_InputField emailInput;
    [SerializeField] private TMP_InputField passwordInput;

    [Header("Button")]
    [SerializeField] private Button signupButton;

    [Header("Server")]
    [SerializeField]
    private string apiUrl = "http://127.0.0.1:3000";


    private void Start()
    {
        signupButton.onClick.AddListener(
            OnSignupClicked
        );
    }


    private void OnDestroy()
    {
        signupButton.onClick.RemoveListener(
            OnSignupClicked
        );
    }


    // ========================================
    // SIGN UP BUTTON
    // ========================================

    private void OnSignupClicked()
    {
        string email =
            emailInput.text.Trim();

        string password =
            passwordInput.text;


        // -------------------------
        // Validate email
        // -------------------------

        if (string.IsNullOrEmpty(email))
        {
            Debug.LogError("Email is required.");
            return;
        }


        // -------------------------
        // Validate password
        // -------------------------

        if (string.IsNullOrEmpty(password))
        {
            Debug.LogError("Password is required.");
            return;
        }


        Debug.Log(
            "Signup: " + email
        );


        StartCoroutine(
            SignupRequest(
                email,
                password
            )
        );
    }


    // ========================================
    // SEND TO FASTAPI
    // ========================================

    private IEnumerator SignupRequest(
        string email,
        string password
    )
    {
        EmailLoginRequest data =
            new EmailLoginRequest
            {
                email = email,
                password = password
            };


        string json =
            JsonUtility.ToJson(data);


        byte[] body =
            Encoding.UTF8.GetBytes(json);


        using UnityWebRequest request =
            new UnityWebRequest(
                apiUrl + "/auth/email",
                "POST"
            );


        request.uploadHandler =
            new UploadHandlerRaw(body);

        request.downloadHandler =
            new DownloadHandlerBuffer();


        request.SetRequestHeader(
            "Content-Type",
            "application/json"
        );


        Debug.Log(
            "Sending signup request..."
        );


        yield return request.SendWebRequest();


        // ========================================
        // ERROR
        // ========================================

        if (
            request.result !=
            UnityWebRequest.Result.Success
        )
        {
            Debug.LogError(
                "Signup failed"
            );

            Debug.LogError(
                request.error
            );

            Debug.LogError(
                request.downloadHandler.text
            );

            yield break;
        }


        // ========================================
        // RESPONSE
        // ========================================

        string response =
            request.downloadHandler.text;


        Debug.Log(
            "Server response:"
        );

        Debug.Log(response);


        LoginResponse result =
            JsonUtility.FromJson<LoginResponse>(
                response
            );


        if (!string.IsNullOrEmpty(result.error))
        {
            Debug.LogError(
                result.error
            );

            yield break;
        }


        Debug.Log(
            "Signup/Login successful!"
        );


        // ----------------------------------------
        // Store tokens
        // ----------------------------------------

        if (!string.IsNullOrEmpty(
            result.access_token
        ))
        {
            PlayerPrefs.SetString(
                "access_token",
                result.access_token
            );
        }


        if (!string.IsNullOrEmpty(
            result.refresh_token
        ))
        {
            PlayerPrefs.SetString(
                "refresh_token",
                result.refresh_token
            );
        }


        PlayerPrefs.Save();


        Debug.Log(
            "Tokens saved."
        );


        // TODO:
        // Load your next Unity scene here.
        //
        // SceneManager.LoadScene("Home");
    }
}