using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class TutorialTransition : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string tutorialSceneName = "TutorialVDO";
    [SerializeField] private string mainSceneName = "main_game";

    private bool isLoading;
    private bool isClosing;
    private float previousTimeScale = 1f;

    public void OpenTutorialVDO()
    {
        if (isLoading || isClosing)
            return;

        StartCoroutine(LoadTutorial());
    }

    private IEnumerator LoadTutorial()
    {
        isLoading = true;

        // Remember the current game speed, then pause the game.
        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        Scene tutorialScene =
            SceneManager.GetSceneByName(tutorialSceneName);

        if (!tutorialScene.isLoaded)
        {
            AsyncOperation operation = SceneManager.LoadSceneAsync(
                tutorialSceneName,
                LoadSceneMode.Additive
            );

            if (operation == null)
            {
                Debug.LogError(
                    "Failed to load tutorial scene: " + tutorialSceneName
                );

                Time.timeScale = previousTimeScale;
                isLoading = false;
                yield break;
            }

            yield return operation;
        }

        tutorialScene =
            SceneManager.GetSceneByName(tutorialSceneName);

        if (!tutorialScene.IsValid() || !tutorialScene.isLoaded)
        {
            Debug.LogError("Tutorial scene did not load correctly.");

            Time.timeScale = previousTimeScale;
            isLoading = false;
            yield break;
        }

        SceneManager.SetActiveScene(tutorialScene);

        Debug.Log("TutorialVDO opened. Game paused.");

        isLoading = false;
    }

    public void CloseTutorialVDO()
    {
        if (isLoading || isClosing)
            return;

        StartCoroutine(UnloadTutorial());
    }

    private IEnumerator UnloadTutorial()
    {
        isClosing = true;

        Scene tutorialScene =
            SceneManager.GetSceneByName(tutorialSceneName);

        if (tutorialScene.IsValid() && tutorialScene.isLoaded)
        {
            Scene mainScene =
                SceneManager.GetSceneByName(mainSceneName);

            if (!mainScene.IsValid() || !mainScene.isLoaded)
            {
                Debug.LogError("Main scene is not loaded: " + mainSceneName);
                isClosing = false;
                yield break;
            }

            SceneManager.SetActiveScene(mainScene);

            AsyncOperation operation =
                SceneManager.UnloadSceneAsync(tutorialScene);

            if (operation != null)
                yield return operation;
        }

        // Resume the game after TutorialVDO closes.
        Time.timeScale = previousTimeScale;

        Debug.Log("TutorialVDO closed. Game resumed.");

        isClosing = false;
    }
}