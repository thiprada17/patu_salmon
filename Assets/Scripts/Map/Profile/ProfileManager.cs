
using UnityEngine;

public class ProfileManager : MonoBehaviour
{
    [Header("Profile")]
    [SerializeField] private GameObject frameSalmon;
    [SerializeField] private GameObject framePlatu;

    public void ShowProfile()
    {
        string selectedCharacter =
            PlayerPrefs.GetString("SelectedCharacter", "salmon");

        bool isSalmon = selectedCharacter.ToLower() == "salmon";
        bool isPlatu = selectedCharacter.ToLower() == "platu";

        if (frameSalmon != null)
            frameSalmon.SetActive(isSalmon);

        if (framePlatu != null)
            framePlatu.SetActive(isPlatu);

        Debug.Log("Selected character: " + selectedCharacter);
    }

    private void Start()
    {
        ShowProfile();
    }
}