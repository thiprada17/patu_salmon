
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
    private bool tutorialOpened;

    public void OpenTutorialVDO()
    {
        if (isLoading || isClosing || tutorialOpened)
            return;

        StartCoroutine(LoadTutorial());
    }

    private IEnumerator LoadTutorial()
    {
        isLoading = true;

        // TutorialVideoCarousel pauses gameplay after it captures the current
        // time scale. Pausing here first would make it capture zero and leave
        // the main game frozen after the video scene closes.
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
            isLoading = false;
            yield break;
        }

        Scene mainScene =
            SceneManager.GetSceneByName(mainSceneName);

        if (mainScene.IsValid() && mainScene.isLoaded)
            SceneManager.SetActiveScene(tutorialScene);

        tutorialOpened = true;
        isLoading = false;

        Debug.Log("TutorialVDO opened. Game paused.");
    }

    public void CloseTutorialVDO()
    {
        if (isLoading || isClosing || !tutorialOpened)
            return;

        StartCoroutine(UnloadTutorial());
    }

    private IEnumerator UnloadTutorial()
    {
        isClosing = true;

        Scene tutorialScene =
            SceneManager.GetSceneByName(tutorialSceneName);

        Scene mainScene =
            SceneManager.GetSceneByName(mainSceneName);

        if (!mainScene.IsValid() || !mainScene.isLoaded)
        {
            Debug.LogError("Main scene is not loaded: " + mainSceneName);
            isClosing = false;
            yield break;
        }

        SceneManager.SetActiveScene(mainScene);

        if (tutorialScene.IsValid() && tutorialScene.isLoaded)
        {
            AsyncOperation operation =
                SceneManager.UnloadSceneAsync(tutorialScene);

            if (operation != null)
                yield return operation;
        }

        tutorialOpened = false;
        isClosing = false;

        TutorialSession.CompleteTutorial();

        Time.timeScale = 1f;

        Debug.Log("TutorialVDO closed. Game resumed.");
    }
}
