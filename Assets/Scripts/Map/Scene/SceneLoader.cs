using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        GameLogger.DebugLog("Loadsence", "twsttwts");
        string currentScene = SceneManager.GetActiveScene().name;
        if ((currentScene == "map" && sceneName == "main_game") ||
            (currentScene == "main_game" && sceneName == "map"))
            MapGameLoading.Load(sceneName);
        else
            SceneManager.LoadScene(sceneName);
    }
}
