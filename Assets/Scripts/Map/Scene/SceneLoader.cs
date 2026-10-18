using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        GameLogger.DebugLog("Loadsence", "twsttwts");
        SceneManager.LoadScene(sceneName);
    }
}
