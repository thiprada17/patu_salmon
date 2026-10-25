
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class ProfileManager : MonoBehaviour
{
    [Header("Profile")]
    [SerializeField] private GameObject frameSalmon;
    [SerializeField] private GameObject framePlatu;
    [SerializeField] private TMP_Text usernameText;

    [Header("Scene Loader")]
    [SerializeField] private string settingsSceneName = "Settings";
    [SerializeField] private string mapSceneName = "Map";

    private void Start()
    {
        Debug.Log("[ProfileManager] Start() called.");
        ShowProfile();
    }

    public void ShowProfile()
    {
        Debug.Log("[ProfileManager] ShowProfile() called.");

        // Show selected character
        string selectedCharacter =
            PlayerPrefs.GetString("SelectedCharacter", "salmon").ToLower();

        if (frameSalmon != null)
            frameSalmon.SetActive(selectedCharacter == "salmon");

        if (framePlatu != null)
            framePlatu.SetActive(selectedCharacter == "platu");

        // Load username
        string playerUsername = PlayerPrefs.GetString("Username", "Player");

        Debug.Log("[ProfileManager] Username key exists: " +
                  PlayerPrefs.HasKey("Username"));

        Debug.Log("[ProfileManager] Username loaded: '" +
                  playerUsername + "'");

        // Display username if a TMP reference is assigned
        if (usernameText != null)
        {
            usernameText.text = playerUsername;

            Debug.Log("[ProfileManager] Username displayed: " +
                      playerUsername);
        }
        else
        {
            Debug.Log("[ProfileManager] No username TMP assigned. " +
                      "Skipping username display.");
        }
    }

    public void OpenSettings()
    {
        Debug.Log("[ProfileManager] Opening Settings scene.");
        SceneManager.LoadScene(settingsSceneName);
    }

    public void CloseSettings()
    {
        Debug.Log("[ProfileManager] Returning to Map scene.");
        SceneManager.LoadScene(mapSceneName);
    }
}
