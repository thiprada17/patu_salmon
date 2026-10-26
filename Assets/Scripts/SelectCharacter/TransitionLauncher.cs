using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class TransitionLauncher : MonoBehaviour
{
    public static TransitionLauncher Instance { get; private set; }

    [SerializeField] private string transitionScene = "TransitionScene";

    private bool isLoading;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void LoadScene(string targetScene)
    {
        if (isLoading)
            return;

        if (string.IsNullOrWhiteSpace(targetScene))
        {
            GameLogger.Error(
                "TransitionLauncher",
                "Target scene is empty."
            );
            return;
        }

        StartCoroutine(LoadTransitionScene(targetScene));
    }

    private IEnumerator LoadTransitionScene(string targetScene)
    {
        isLoading = true;

        Scene source = SceneManager.GetActiveScene();

        if (!Application.CanStreamedLevelBeLoaded(transitionScene))
        {
            GameLogger.Error(
                "TransitionLauncher",
                "Scene is missing from Build Profiles: " + transitionScene
            );

            isLoading = false;
            yield break;
        }

        Scene transition = SceneManager.GetSceneByName(transitionScene);

        if (!transition.IsValid() || !transition.isLoaded)
        {
            AsyncOperation operation = SceneManager.LoadSceneAsync(
                transitionScene,
                LoadSceneMode.Additive
            );

            if (operation == null)
            {
                GameLogger.Error(
                    "TransitionLauncher",
                    "Failed to start loading: " + transitionScene
                );

                isLoading = false;
                yield break;
            }

            while (!operation.isDone)
                yield return null;

            transition = SceneManager.GetSceneByName(transitionScene);
        }

        if (!transition.IsValid() || !transition.isLoaded)
        {
            GameLogger.Error(
                "TransitionLauncher",
                "Scene did not load correctly: " + transitionScene
            );

            isLoading = false;
            yield break;
        }

        // ปิด EventSystem และ AudioListener ใน TransitionScene
        // เพื่อไม่ให้ซ้ำกับ Scene ต้นทาง
        DisableTransitionDuplicates(transition);

        SceneTransition controller = null;

        foreach (GameObject root in transition.GetRootGameObjects())
        {
            controller = root.GetComponentInChildren<SceneTransition>(true);

            if (controller != null)
                break;
        }

        if (controller == null)
        {
            GameLogger.Error(
                "TransitionLauncher",
                "SceneTransition not found in " + transition.path
            );

            isLoading = false;
            yield break;
        }

        SceneManager.SetActiveScene(transition);

        controller.BeginTransition(targetScene, source.name);
    }

    private void DisableTransitionDuplicates(Scene transition)
    {
        foreach (GameObject root in transition.GetRootGameObjects())
        {
            EventSystem[] eventSystems =
                root.GetComponentsInChildren<EventSystem>(true);

            foreach (EventSystem eventSystem in eventSystems)
            {
                eventSystem.enabled = false;
            }

            AudioListener[] listeners =
                root.GetComponentsInChildren<AudioListener>(true);

            foreach (AudioListener listener in listeners)
            {
                listener.enabled = false;
            }
        }
    }
}