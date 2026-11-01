
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance { get; private set; }

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private RectTransform planeRoot;
    [SerializeField] private Transform planeAnimation;
    [SerializeField] private Animator leaf1Animator;
    [SerializeField] private Animator leaf2Animator;
    private CanvasGroup leaf1CanvasGroup;
    private CanvasGroup leaf2CanvasGroup;

    [Header("Animation States")]
    [SerializeField] private string inState = "flyin";
    [SerializeField] private string outState = "flyout";
    [SerializeField] private string idleState = "idle";
    [SerializeField] private string planeMoveState = "plane_move";
    [SerializeField] private int planeMoveLayer = 1;
    [SerializeField] private string leaf1State = "leaf1";
    [SerializeField] private string leaf2State = "leaf2";

    [Header("Settings")]
    [SerializeField] private float timeout = 5f;
    [SerializeField, Range(0.5f, 0.98f)] private float leafFadeStartNormalized = 0.8f;
    [SerializeField] private float leafFadeDuration = 0.35f;
    [SerializeField, Range(0.5f, 0.95f)] private float sourceUiFadeStartNormalized = 0.72f;
    [SerializeField] private float sourceUiFadeDuration = 0.4f;
    [SerializeField] private float destinationUiFadeDuration = 0.7f;

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

        if (leaf1Animator == null || leaf2Animator == null)
        {
            foreach (Animator childAnimator in GetComponentsInChildren<Animator>(true))
            {
                if (childAnimator.gameObject.name == "leaf1")
                    leaf1Animator = childAnimator;
                else if (childAnimator.gameObject.name == "leaf2")
                    leaf2Animator = childAnimator;
            }
        }

        leaf1CanvasGroup = GetOrAddCanvasGroup(leaf1Animator);
        leaf2CanvasGroup = GetOrAddCanvasGroup(leaf2Animator);
        BringLeavesToFront();

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

    public void SetRenderCamera(Camera renderCamera)
    {
        Canvas transitionCanvas = GetComponent<Canvas>();
        if (transitionCanvas != null &&
            transitionCanvas.renderMode == RenderMode.ScreenSpaceCamera &&
            renderCamera != null)
        {
            transitionCanvas.worldCamera = renderCamera;
        }
    }

    private IEnumerator TransitionRoutine()
    {
        isRunning = true;

        // เครื่องบินบินเข้ามาบังฉากเดิม
        Play(0, inState);
        StartCoroutine(FadeSelectCharacterUiNearFlyInEnd(inState));
        PlayLeaf(leaf1Animator, leaf1State, 0);
        // leaf2 has its own controller, where its state is on Base Layer.
        PlayLeaf(leaf2Animator, leaf2State, 0);
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

        CanvasGroup[] destinationCanvasGroups =
            PrepareSceneCanvasGroups(destination, out float[] destinationCanvasAlphas);

        SceneManager.SetActiveScene(destination);
        Camera destinationCamera = FindSceneCamera(destination);
        if (destinationCamera != null)
            SetRenderCamera(destinationCamera);

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
        StartCoroutine(FadeCanvasGroupsIn(
            destinationCanvasGroups,
            destinationCanvasAlphas,
            destinationUiFadeDuration
        ));
        yield return FadeLeavesNearEnd(outState);
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

    private Camera FindSceneCamera(Scene scene)
    {
        Camera fallback = null;

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (Camera candidate in root.GetComponentsInChildren<Camera>(true))
            {
                if (!candidate.isActiveAndEnabled)
                    continue;

                if (candidate.CompareTag("MainCamera"))
                    return candidate;

                if (fallback == null)
                    fallback = candidate;
            }
        }

        return fallback;
    }

    private CanvasGroup GetOrAddCanvasGroup(Animator leafAnimator)
    {
        if (leafAnimator == null)
            return null;

        CanvasGroup group = leafAnimator.GetComponent<CanvasGroup>();
        if (group == null)
            group = leafAnimator.gameObject.AddComponent<CanvasGroup>();

        group.alpha = 1f;
        return group;
    }

    private void BringLeavesToFront()
    {
        if (leaf1Animator != null)
            leaf1Animator.transform.SetAsLastSibling();

        if (leaf2Animator != null)
            leaf2Animator.transform.SetAsLastSibling();
    }

    private IEnumerator FadeLeavesNearEnd(string stateName)
    {
        if (animator == null || (leaf1CanvasGroup == null && leaf2CanvasGroup == null))
            yield break;

        int hash = Animator.StringToHash(stateName);
        float elapsed = 0f;

        while (elapsed < timeout)
        {
            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
            if (state.shortNameHash == hash &&
                state.normalizedTime >= leafFadeStartNormalized &&
                !animator.IsInTransition(0))
            {
                break;
            }

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        float leaf1StartAlpha = leaf1CanvasGroup != null ? leaf1CanvasGroup.alpha : 0f;
        float leaf2StartAlpha = leaf2CanvasGroup != null ? leaf2CanvasGroup.alpha : 0f;
        float fadeElapsed = 0f;
        float duration = Mathf.Max(0.01f, leafFadeDuration);

        while (fadeElapsed < duration)
        {
            fadeElapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(fadeElapsed / duration);
            float eased = progress * progress * (3f - 2f * progress);
            float alpha = 1f - eased;

            if (leaf1CanvasGroup != null)
                leaf1CanvasGroup.alpha = leaf1StartAlpha * alpha;
            if (leaf2CanvasGroup != null)
                leaf2CanvasGroup.alpha = leaf2StartAlpha * alpha;

            yield return null;
        }

        if (leaf1CanvasGroup != null)
            leaf1CanvasGroup.alpha = 0f;
        if (leaf2CanvasGroup != null)
            leaf2CanvasGroup.alpha = 0f;
    }

    private IEnumerator FadeSelectCharacterUiNearFlyInEnd(string stateName)
    {
        if (sourceScene != "SelectCharacter")
            yield break;

        Scene source = SceneManager.GetSceneByName(sourceScene);
        if (!source.IsValid() || !source.isLoaded)
            yield break;

        CanvasGroup[] canvasGroups = GetRootCanvasGroups(source);
        List<SpriteRenderer> titleRenderers = new List<SpriteRenderer>();
        foreach (GameObject root in source.GetRootGameObjects())
        {
            foreach (SpriteRenderer spriteRenderer in root.GetComponentsInChildren<SpriteRenderer>(true))
            {
                if (spriteRenderer.gameObject.name.StartsWith("Characters_Title", System.StringComparison.OrdinalIgnoreCase))
                    titleRenderers.Add(spriteRenderer);
            }
        }

        if (canvasGroups.Length == 0 && titleRenderers.Count == 0)
            yield break;

        int hash = Animator.StringToHash(stateName);
        float elapsed = 0f;
        while (elapsed < timeout)
        {
            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
            if (state.shortNameHash == hash &&
                state.normalizedTime >= sourceUiFadeStartNormalized &&
                !animator.IsInTransition(0))
            {
                break;
            }

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        float[] canvasStartAlphas = new float[canvasGroups.Length];
        for (int i = 0; i < canvasGroups.Length; i++)
        {
            canvasStartAlphas[i] = canvasGroups[i].alpha;
            canvasGroups[i].interactable = false;
            canvasGroups[i].blocksRaycasts = false;
        }

        float[] titleStartAlphas = new float[titleRenderers.Count];
        for (int i = 0; i < titleRenderers.Count; i++)
            titleStartAlphas[i] = titleRenderers[i].color.a;

        float fadeElapsed = 0f;
        float duration = Mathf.Max(0.01f, sourceUiFadeDuration);
        while (fadeElapsed < duration)
        {
            fadeElapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(fadeElapsed / duration);
            float eased = progress * progress * (3f - 2f * progress);
            float alpha = 1f - eased;

            for (int i = 0; i < canvasGroups.Length; i++)
                if (canvasGroups[i] != null)
                    canvasGroups[i].alpha = canvasStartAlphas[i] * alpha;

            for (int i = 0; i < titleRenderers.Count; i++)
            {
                if (titleRenderers[i] == null)
                    continue;

                Color color = titleRenderers[i].color;
                color.a = titleStartAlphas[i] * alpha;
                titleRenderers[i].color = color;
            }

            yield return null;
        }

        foreach (CanvasGroup group in canvasGroups)
            if (group != null)
                group.alpha = 0f;

        foreach (SpriteRenderer titleRenderer in titleRenderers)
        {
            if (titleRenderer == null)
                continue;

            Color color = titleRenderer.color;
            color.a = 0f;
            titleRenderer.color = color;
        }
    }

    private CanvasGroup[] PrepareSceneCanvasGroups(Scene scene, out float[] targetAlphas)
    {
        CanvasGroup[] groups = GetRootCanvasGroups(scene);
        targetAlphas = new float[groups.Length];

        for (int i = 0; i < groups.Length; i++)
        {
            targetAlphas[i] = groups[i].alpha;
            groups[i].alpha = 0f;
            groups[i].interactable = false;
            groups[i].blocksRaycasts = false;
        }

        return groups;
    }

    private CanvasGroup[] GetRootCanvasGroups(Scene scene)
    {
        List<CanvasGroup> groups = new List<CanvasGroup>();
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (Canvas canvas in root.GetComponentsInChildren<Canvas>(true))
            {
                Transform parent = canvas.transform.parent;
                if (parent != null && parent.GetComponentInParent<Canvas>() != null)
                    continue;

                CanvasGroup group = canvas.GetComponent<CanvasGroup>();
                if (group == null)
                    group = canvas.gameObject.AddComponent<CanvasGroup>();

                groups.Add(group);
            }
        }

        return groups.ToArray();
    }

    private IEnumerator FadeCanvasGroupsIn(
        CanvasGroup[] groups,
        float[] targetAlphas,
        float duration)
    {
        if (groups == null || targetAlphas == null || groups.Length == 0)
            yield break;

        float elapsed = 0f;
        float fadeDuration = Mathf.Max(0.01f, duration);
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / fadeDuration);
            float eased = progress * progress * (3f - 2f * progress);

            for (int i = 0; i < groups.Length; i++)
            {
                if (groups[i] == null)
                    continue;

                groups[i].alpha = Mathf.Lerp(0f, targetAlphas[i], eased);
            }

            yield return null;
        }

        for (int i = 0; i < groups.Length; i++)
        {
            if (groups[i] == null)
                continue;

            groups[i].alpha = targetAlphas[i];
            groups[i].interactable = true;
            groups[i].blocksRaycasts = true;
        }
    }

    private void PlayLeaf(Animator leafAnimator, string stateName, int layer)
    {
        if (leafAnimator == null)
        {
            GameLogger.Warn("SceneTransition", stateName + " Animator is not assigned.");
            return;
        }

        int hash = Animator.StringToHash(stateName);
        if (layer >= leafAnimator.layerCount || !leafAnimator.HasState(layer, hash))
        {
            GameLogger.Error("SceneTransition", "Leaf state not found: " + stateName + " on layer " + layer);
            return;
        }

        leafAnimator.SetLayerWeight(layer, 1f);
        leafAnimator.Play(hash, layer, 0f);
        leafAnimator.Update(0f);
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
