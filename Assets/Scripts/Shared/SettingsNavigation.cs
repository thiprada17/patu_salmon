using UnityEngine;
using UnityEngine.SceneManagement;

public static class SettingsNavigation
{
    private static string returnScene = "map";

    public static void OpenFrom(string sourceScene)
    {
        returnScene = sourceScene;

        Debug.Log("Opening Settings. Return scene saved: " + returnScene);

        SceneManager.LoadScene("setting");
    }

    public static void GoBack()
    {
        Debug.Log("Back pressed. Returning to: " + returnScene);

        SceneManager.LoadScene(returnScene);
    }
}