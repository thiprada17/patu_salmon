using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButtonLoader : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    public void LoadSceneOverlay(string sceneName)
    {
        if (SceneManager.GetSceneByName(sceneName).isLoaded) return;  

        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
    }
}