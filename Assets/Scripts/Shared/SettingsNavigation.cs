using UnityEngine;
using UnityEngine.SceneManagement;

public static class SettingsNavigation
{
    private static string returnScene = "map";

    public static void OpenFrom(string sourceScene)
    {
        if (sourceScene == "map" || sourceScene == "main_game")
            returnScene = sourceScene;

        Debug.Log("Opening Settings. Return scene saved: " + returnScene);

        SceneManager.LoadScene("setting");
    }

    public static void GoBack()
    {
        Debug.Log("Back pressed. Returning to: " + returnScene);

        string sceneToLoad = returnScene;
        returnScene = "map";
        SceneManager.LoadScene(sceneToLoad);
    }
}
