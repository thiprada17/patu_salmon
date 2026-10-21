
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

    [Header("Scene")]
    [SerializeField] private string nextScene = "main_game";

    private bool isSending;

    private void Awake()
    {
        string character = PlayerPrefs.GetString("SelectedCharacter", "");

        GameLogger.Info("Loading", $"Selected character: {character}");

        if (infoBoard != null && infoSprites != null)
        {
            if (character == "platu")
                infoBoard.sprite = infoSprites[0];
            else if (character == "salmon")
                infoBoard.sprite = infoSprites[1];
        }
    }

    private void Start()
    {
        if (loadingText != null)
            StartCoroutine(Hop());

        SendPlayerData();
    }

    public void SendPlayerData()
    {
        if (isSending)
            return;

        string userId = LoginManager.UserId;
        string username = PlayerPrefs.GetString("Username", "");
        string character = PlayerPrefs.GetString("SelectedCharacter", "");
        string language = PlayerPrefs.GetString("Language", "english");

        if (string.IsNullOrWhiteSpace(userId))
        {
            GameLogger.Error(
                "Loading",
                "UserID is empty. Login/Signup must save the real Supabase UUID."
            );

            SetLoadingText("User ID not found. Please log in again.");
            return;
        }

        if (string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(character))
        {
            GameLogger.Error(
                "Loading",
                "Username or character is empty."
            );

            SetLoadingText("Incomplete player information.");
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
                GameLogger.Info(
                    "Loading",
                    "Starter player data saved successfully."
                );

              
                SetLoadingText("Entering game...");
                
                SceneManager.LoadScene(nextScene);
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

    private void SetLoadingText(string message)
    {
        if (loadingText != null)
            loadingText.text = message;
    }

    private IEnumerator Hop()
    {
        while (true)
        {
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

                    float offset = 0f;

                    if (p > 0f && p < 1f)
                        offset = Mathf.Sin(p * Mathf.PI) * hopHeight;

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
