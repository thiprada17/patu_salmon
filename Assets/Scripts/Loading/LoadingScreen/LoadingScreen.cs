
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;
using UnityEngine.Networking;

public class LoadingScreen : MonoBehaviour
{
    [Header("Info")]
    [SerializeField] private SpriteRenderer infoBoard;
    [SerializeField] private Sprite[] infoSprites;

    [Header("Loading Animation")]
    [SerializeField] private TMP_Text loadingText;
    [SerializeField] private float hopHeight = 10f;
    [SerializeField] private float hopTime = 0.25f;
    [SerializeField] private float hopGap = 0.06f;
    [SerializeField] private float restTime = 0.4f;

    [Header("Loading Duration")]
    [SerializeField] private float minimumLoadingTime = 3f;

    [Header("Scene")]
    [SerializeField] private string nextScene = "main_game";

    private bool isSending;
    private bool isRecoveringUserId;
    private bool isReadyToEnterGame;
    private bool hasAuthResponse;
    private float loadingStartTime;

    private LoginManager.AuthResponse authResponse;

    [System.Serializable]
    private class AuthMeResponse
    {
        public string user_id;
        public string email;
    }

    public static LoginManager.AuthResponse PendingAuthResponse { get; set; }

    private void Awake()
    {
        string character = PlayerPrefs.GetString("SelectedCharacter", "");

        GameLogger.Info("Loading", $"Selected character: {character}");

        if (infoBoard != null && infoSprites != null)
        {
            if (character == "platu" && infoSprites.Length > 0)
                infoBoard.sprite = infoSprites[0];
            else if (character == "salmon" && infoSprites.Length > 1)
                infoBoard.sprite = infoSprites[1];
        }

        if (PendingAuthResponse != null)
        {
            authResponse = PendingAuthResponse;
            PendingAuthResponse = null;
            hasAuthResponse = true;
        }
    }

    private void Start()
    {
        loadingStartTime = Time.time;

        if (loadingText != null)
            StartCoroutine(Hop());

        SendPlayerData();
    }

    private void Update()
    {
        if (!isReadyToEnterGame)
            return;

        if (Time.time - loadingStartTime < minimumLoadingTime)
            return;

        SceneManager.LoadScene(nextScene);
    }

    public void SetAuthResponse(LoginManager.AuthResponse response)
    {
        authResponse = response;
        hasAuthResponse = response != null;
    }

    public void SendPlayerData()
    {
        if (isSending || isReadyToEnterGame)
            return;

        string userId = LoginManager.UserId;

        if (string.IsNullOrWhiteSpace(userId))
        {
            string accessToken = PlayerPrefs.GetString("AccessToken", "");
            if (!string.IsNullOrEmpty(accessToken) && !isRecoveringUserId)
            {
                StartCoroutine(RecoverUserIdAndRetry(accessToken));
                return;
            }

            GameLogger.Error("Loading", "UserID is empty and could not be recovered from the saved session.");
            SetLoadingText("Unable to restore your login. Please log in again.");
            return;
        }

        string username = PlayerPrefs.GetString("Username", "");
        string character = PlayerPrefs.GetString("SelectedCharacter", "");
        string language = PlayerPrefs.GetString("Language", "");

        if (string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(character) ||
            string.IsNullOrWhiteSpace(language))
        {
            GameLogger.Error("Loading", "Player setup is incomplete. Returning to language selection.");
            ReturnToPlayerSetup();
            return;
        }

        if (!PlayerPrefs.HasKey("IsNewUser") ||
            !PlayerPrefs.HasKey("HasProfile"))
        {
            GameLogger.Error(
                "Loading",
                "Login status is missing. Returning to player setup."
            );

            ReturnToPlayerSetup();
            return;
        }

        bool isNewUser = PlayerPrefs.GetInt("IsNewUser") == 1;
        bool hasProfile = PlayerPrefs.GetInt("HasProfile") == 1;

        if (!isNewUser || hasProfile)
        {
            GameLogger.Info(
                "Loading",
                "Existing player detected. Skipping starter data."
            );

            PrepareToEnterGame();
            return;
        }

        string savedUserId =
            PlayerPrefs.GetString("StarterPlayerSavedUserID", "");

        if (savedUserId == userId)
        {
            GameLogger.Info(
                "Loading",
                "Starter player already saved for this user. Skipping save."
            );

            PrepareToEnterGame();
            return;
        }

        isSending = true;
        SetLoadingText("Saving player data...");

        LoginManager.Instance.SendStarterPlayer(
            username,
            character,
            language,
            onSuccess: () =>
            {
                PlayerPrefs.SetString("StarterPlayerSavedUserID", userId);
                PlayerPrefs.Save();

                GameLogger.Info(
                    "Loading",
                    "Starter player data saved successfully."
                );

                PrepareToEnterGame();
            },
            onError: error =>
            {
                isSending = false;

                GameLogger.Error(
                    "Loading",
                    "Failed to save starter player: " + error
                );

                SetLoadingText("Failed to save data. Please try again.");
            }
        );
    }

    private void PrepareToEnterGame()
    {
        isReadyToEnterGame = true;
        SetLoadingText("Entering game...");
    }

    private void ReturnToPlayerSetup()
    {
        PlayerPrefs.SetInt("IsNewUser", 1);
        PlayerPrefs.SetInt("HasProfile", 0);
        PlayerPrefs.SetInt("ResumePlayerSetup", 1);
        PlayerPrefs.Save();
        SceneManager.LoadScene("signup");
    }

    private IEnumerator RecoverUserIdAndRetry(string accessToken)
    {
        isRecoveringUserId = true;
        SetLoadingText("Restoring your login...");

        using (UnityWebRequest request = UnityWebRequest.Get("https://patu-salmon.onrender.com/auth/me"))
        {
            request.SetRequestHeader("Authorization", "Bearer " + accessToken);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                AuthMeResponse response = JsonUtility.FromJson<AuthMeResponse>(request.downloadHandler.text);
                if (response != null && !string.IsNullOrWhiteSpace(response.user_id))
                {
                    PlayerPrefs.SetString("UserID", response.user_id);
                    if (!string.IsNullOrEmpty(response.email))
                        PlayerPrefs.SetString("Email", response.email);
                    PlayerPrefs.Save();

                    GameLogger.Info("Loading", "Recovered UserID from the saved access token.");
                    isRecoveringUserId = false;
                    SendPlayerData();
                    yield break;
                }
            }

            GameLogger.Error("Loading", "Could not recover UserID: " + request.error + " " + request.downloadHandler.text);
        }

        isRecoveringUserId = false;
        SetLoadingText("Unable to restore your login. Please log in again.");
    }

    private void SetLoadingText(string message)
    {
        if (loadingText != null)
            loadingText.text = message;
    }

    private IEnumerator Hop()
    {
        while (true)
        {
            if (loadingText == null)
                yield break;

            loadingText.ForceMeshUpdate();
            TMP_TextInfo info = loadingText.textInfo;
            int count = info.characterCount;

            float total = hopGap * count + hopTime;
            float elapsed = 0f;

            while (elapsed < total)
            {
                elapsed += Time.deltaTime;
                loadingText.ForceMeshUpdate();
                info = loadingText.textInfo;

                for (int i = 0; i < info.characterCount; i++)
                {
                    if (!info.characterInfo[i].isVisible)
                        continue;

                    float start = i * hopGap;
                    float p = (elapsed - start) / hopTime;

                    float offset = p > 0f && p < 1f
                        ? Mathf.Sin(p * Mathf.PI) * hopHeight
                        : 0f;

                    int mat = info.characterInfo[i].materialReferenceIndex;
                    int vert = info.characterInfo[i].vertexIndex;
                    Vector3[] verts = info.meshInfo[mat].vertices;

                    for (int j = 0; j < 4; j++)
                        verts[vert + j] += new Vector3(0f, offset, 0f);
                }

                for (int i = 0; i < info.meshInfo.Length; i++)
                {
                    info.meshInfo[i].mesh.vertices = info.meshInfo[i].vertices;
                    loadingText.UpdateGeometry(info.meshInfo[i].mesh, i);
                }

                yield return null;
            }

            yield return new WaitForSeconds(restTime);
        }
    }
}
