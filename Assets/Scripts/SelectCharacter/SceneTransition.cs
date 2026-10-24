
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance { get; private set; }

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private RectTransform planeRoot;
    [SerializeField] private Transform planeAnimation;

    [Header("Animation States")]
    [SerializeField] private string inState = "flyin";
    [SerializeField] private string outState = "flyout";
    [SerializeField] private string idleState = "idle";
    [SerializeField] private string planeMoveState = "plane_move";
    [SerializeField] private int planeMoveLayer = 1;

    [Header("Settings")]
    [SerializeField] private float timeout = 5f;

    private bool isRunning;
    private string targetScene;
    private string sourceScene;
    private bool requestStarted;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (animator == null)
            animator = GetComponentInChildren<Animator>(true);

        if (planeRoot == null)
            planeRoot = GetComponent<RectTransform>();

        if (animator != null && planeAnimation == null)
            planeAnimation = animator.transform;

        if (animator == null)
        {
            GameLogger.Error("SceneTransition", "Animator is not assigned.");
            return;
        }

        Play(0, idleState);
        Play(planeMoveLayer, planeMoveState);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void BeginTransition(string destination, string source)
    {
        if (isRunning || requestStarted)
            return;

        if (string.IsNullOrWhiteSpace(destination))
        {
            GameLogger.Error("SceneTransition", "Destination scene is empty.");
            return;
        }

        targetScene = destination;
        sourceScene = source;
        requestStarted = true;

        StartCoroutine(TransitionRoutine());
    }

    private IEnumerator TransitionRoutine()
    {
        isRunning = true;

        // เครื่องบินบินเข้ามาบังฉากเดิม
        Play(0, inState);
        yield return WaitFinished(inState);

        // โหลดฉากปลายทางไว้ด้านหลัง TransitionScene
        AsyncOperation loadOperation =
            SceneManager.LoadSceneAsync(targetScene, LoadSceneMode.Additive);

        if (loadOperation == null)
        {
            GameLogger.Error(
                "SceneTransition",
                "Cannot load destination: " + targetScene
            );

            isRunning = false;
            requestStarted = false;
            yield break;
        }

        while (!loadOperation.isDone)
            yield return null;

        Scene destination = SceneManager.GetSceneByName(targetScene);

        if (!destination.IsValid() || !destination.isLoaded)
        {
            GameLogger.Error(
                "SceneTransition",
                "Destination scene failed to load: " + targetScene
            );

            isRunning = false;
            requestStarted = false;
            yield break;
        }

        SceneManager.SetActiveScene(destination);

        // รอให้ฉากปลายทางเริ่ม Render
        yield return null;
        yield return new WaitForEndOfFrame();

        // ปิดฉากต้นทางหลังโหลดฉากปลายทางสำเร็จแล้ว
        Scene source = SceneManager.GetSceneByName(sourceScene);

        if (source.IsValid() && source.isLoaded &&
            source != destination && source != gameObject.scene)
        {
            AsyncOperation unloadSource =
                SceneManager.UnloadSceneAsync(source);

            if (unloadSource != null)
            {
                while (!unloadSource.isDone)
                    yield return null;
            }
        }

        // เครื่องบินบินออก เผยฉาก idcard
        Play(0, outState);
        yield return WaitFinished(outState);

        // ปิด TransitionScene หลังแอนิเมชันจบ
        Scene transitionScene = gameObject.scene;

        if (transitionScene.IsValid() && transitionScene.isLoaded)
        {
            AsyncOperation unloadTransition =
                SceneManager.UnloadSceneAsync(transitionScene);

            if (unloadTransition != null)
            {
                while (!unloadTransition.isDone)
                    yield return null;
            }
        }
    }

    private bool Play(int layer, string stateName)
    {
        if (animator == null)
            return false;

        int hash = Animator.StringToHash(stateName);

        if (!animator.HasState(layer, hash))
        {
            GameLogger.Error(
                "SceneTransition",
                "State not found: " + stateName + " on layer " + layer
            );
            return false;
        }

        animator.Play(hash, layer, 0f);
        animator.Update(0f);
        return true;
    }

    private IEnumerator WaitFinished(string stateName)
    {
        if (animator == null)
            yield break;

        int hash = Animator.StringToHash(stateName);
        float elapsed = 0f;

        yield return null;

        while (elapsed < timeout)
        {
            AnimatorStateInfo state =
                animator.GetCurrentAnimatorStateInfo(0);

            if (state.shortNameHash == hash &&
                state.normalizedTime >= 1f &&
                !animator.IsInTransition(0))
            {
                yield break;
            }

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        GameLogger.Warn(
            "SceneTransition",
            "Animation timed out: " + stateName
        );
    }
}
