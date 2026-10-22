
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

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
    private bool isReadyToEnterGame;
    private float loadingStartTime;

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

    public void SendPlayerData()
    {
        if (isSending || isReadyToEnterGame)
            return;

        string userId = LoginManager.UserId;

        if (string.IsNullOrWhiteSpace(userId))
        {
            GameLogger.Error("Loading", "UserID is empty. Please log in again.");
            SetLoadingText("User ID not found. Please log in again.");
            return;
        }

        string savedUserId =
            PlayerPrefs.GetString("StarterPlayerSavedUserID", "");

        if (savedUserId == userId)
        {
            GameLogger.Info("Loading", "Starter player already saved. Skipping save.");
            PrepareToEnterGame();
            return;
        }

        string username = PlayerPrefs.GetString("Username", "");
        string character = PlayerPrefs.GetString("SelectedCharacter", "");
        string language = PlayerPrefs.GetString("Language", "english");

        if (string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(character))
        {
            GameLogger.Error("Loading", "Username or character is empty.");
            SetLoadingText("Incomplete player information.");
            return;
        }

        if (LoginManager.Instance == null)
        {
            GameLogger.Error("Loading", "LoginManager instance not found.");
            SetLoadingText("Login service unavailable. Please try again.");
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

                GameLogger.Info("Loading", "Starter player data saved successfully.");

                PrepareToEnterGame();
            },
            onError: error =>
            {
                isSending = false;

                GameLogger.Error("Loading", "Failed to save starter player: " + error);
                SetLoadingText("Failed to save data. Please try again.");
            }
        );
    }

    private void PrepareToEnterGame()
    {
        isReadyToEnterGame = true;
        SetLoadingText("Entering game...");
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
