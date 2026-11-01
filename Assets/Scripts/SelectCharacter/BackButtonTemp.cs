using UnityEngine;
using UnityEngine.SceneManagement;

public class BackButtonTemp : MonoBehaviour
{
    [SerializeField] private string sceneName = "SelectCharacter";

    private void Start()
    {
        Debug.Log("BackButtonTemp is running!");
    }

    public void GoBack()
    {
        Debug.Log("BACK BUTTON CLICKED!");

        if (SceneManager.GetActiveScene().name == "idcard")
        {
            SceneManager.LoadScene(sceneName);
            return;
        }

        SettingsNavigation.GoBack();
    }
}
