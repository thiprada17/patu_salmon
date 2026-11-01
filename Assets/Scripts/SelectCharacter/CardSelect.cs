using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

public class CardSelect : MonoBehaviour
{
    [Header("Character")]
    [SerializeField] private string characterName = "salmon";
    [SerializeField] private string nextScene = "idcard";

    [Header("Selection")]
    [SerializeField] private GameObject glow;
    [SerializeField] private float delay = 0.2f;
    [SerializeField] private float glowFadeInDuration = 0.12f;
    [SerializeField] private float glowFadeOutDuration = 0.28f;
    [SerializeField] private float selectedCardPunchScale = 1.08f;
    [SerializeField] private float selectedCardPunchDuration = 0.4f;

    [Header("Card Animators")]
    [SerializeField] private Animator salmonAnimator;
    [SerializeField] private Animator platuAnimator;

    [Header("Animation State Names")]
    [SerializeField] private string salmonCenterState = "cardsalmon";
    [SerializeField] private string platuCenterState = "cardplatu";
    [SerializeField] private string salmonOutState = "cardsalmonout";
    [SerializeField] private string platuOutState = "cardplatuout";

    [SerializeField] private float animTimeout = 10f;

    private static CardSelect activeSelection;
    private bool isSelecting;

    private void Awake()
    {
        if (glow != null)
            glow.SetActive(false);

    }

    private void OnEnable()
    {
        if (activeSelection != null && !activeSelection)
            activeSelection = null;
    }

    private void Update()
    {
        if (isSelecting || activeSelection != null)
            return;

        // Mobile touch
        if (Touchscreen.current != null)
        {
            var touch = Touchscreen.current.primaryTouch;

            if (touch.press.wasPressedThisFrame)
            {
                int touchId = touch.touchId.ReadValue();

                if (EventSystem.current != null &&
                    EventSystem.current.IsPointerOverGameObject(touchId))
                    return;

                TrySelect(touch.position.ReadValue());
            }

            return;
        }

        // PC mouse
        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (EventSystem.current != null &&
                EventSystem.current.IsPointerOverGameObject())
                return;

            TrySelect(Mouse.current.position.ReadValue());
        }
    }

    private void TrySelect(Vector2 screenPosition)
    {
        Camera cam = Camera.main;

        if (cam == null)
        {
            GameLogger.Error("CardSelect", "Main Camera not found.");
            return;
        }

        Vector3 world = cam.ScreenToWorldPoint(
            new Vector3(
                screenPosition.x,
                screenPosition.y,
                Mathf.Abs(cam.transform.position.z)
            )
        );

        Collider2D hit = Physics2D.OverlapPoint(
            new Vector2(world.x, world.y)
        );

        if (hit == null ||
            (hit.transform != transform &&
             !hit.transform.IsChildOf(transform)))
            return;

        SelectCard();
    }

    // เรียกจากปุ่ม Select ใน Inspector ได้
    public void SelectCard()
    {
        SelectCard(characterName);
    }

    public void SelectSalmonCard()
    {
        GameLogger.Info("CardSelect", "Salmon select button pressed.");
        SelectCard("salmon");
    }

    public void SelectPlatuCard()
    {
        GameLogger.Info("CardSelect", "Platu select button pressed.");
        SelectCard("platu");
    }

    private void SelectCard(string selectedCharacter)
    {
        if (isSelecting || activeSelection != null)
            return;

        if (salmonAnimator == null || platuAnimator == null)
        {
            GameLogger.Error(
                "CardSelect",
                "กรุณากำหนด Salmon Animator และ Platu Animator"
            );
            return;
        }

        bool isSalmon = selectedCharacter.Trim()
            .Equals("salmon", StringComparison.OrdinalIgnoreCase);

        bool isPlatu = selectedCharacter.Trim()
            .Equals("platu", StringComparison.OrdinalIgnoreCase);

        if (!isSalmon && !isPlatu)
        {
            GameLogger.Error(
                "CardSelect",
                "characterName ต้องเป็น salmon หรือ platu"
            );
            return;
        }

        activeSelection = this;
        isSelecting = true;

        PlayerPrefs.SetString("SelectedCharacter", isSalmon ? "salmon" : "platu");
        PlayerPrefs.Save();

        GameLogger.Info(
            "CardSelect",
            "Selected character: " + selectedCharacter
        );

        StartCoroutine(SelectRoutine(isSalmon));
    }

    private IEnumerator SelectRoutine(bool isSalmon)
    {
        Animator selectedAnimator = isSalmon ? salmonAnimator : platuAnimator;
        GameObject selectedGlow = FindGlow(selectedAnimator) ?? glow;

        BringCardToFront(selectedAnimator);

        if (selectedGlow != null)
        {
            selectedGlow.SetActive(true);
            StartCoroutine(FadeGlow(selectedGlow, true, glowFadeInDuration));
        }

        StartCoroutine(PunchCard(selectedAnimator.transform));

        yield return new WaitForSecondsRealtime(delay);

        string salmonState = isSalmon
            ? salmonCenterState
            : salmonOutState;

        string platuState = isSalmon
            ? platuOutState
            : platuCenterState;

        // สั่งเล่นแอนิเมชันทั้งสองการ์ดในเฟรมเดียวกัน
        if (!PlayState(salmonAnimator, salmonState))
        {
            CancelSelection();
            yield break;
        }

        if (!PlayState(platuAnimator, platuState))
        {
            CancelSelection();
            yield break;
        }

        // รอจนกว่าแอนิเมชันทั้งสองจะเล่นจบ
        yield return WaitForAnimations(salmonState, platuState);

        if (selectedGlow != null)
            yield return FadeGlow(selectedGlow, false, glowFadeOutDuration);

        LoadNextScene();
    }

    private GameObject FindGlow(Animator cardAnimator)
    {
        if (cardAnimator == null)
            return null;

        foreach (Transform child in cardAnimator.GetComponentsInChildren<Transform>(true))
        {
            if (child.name.StartsWith("glow", StringComparison.OrdinalIgnoreCase))
                return child.gameObject;
        }

        return null;
    }

    private void BringCardToFront(Animator cardAnimator)
    {
        if (cardAnimator == null)
            return;

        SortingGroup sortingGroup = cardAnimator.GetComponent<SortingGroup>();
        if (sortingGroup == null)
            sortingGroup = cardAnimator.gameObject.AddComponent<SortingGroup>();

        sortingGroup.sortingOrder = 1000;
    }

    private IEnumerator PunchCard(Transform card)
    {
        if (card == null)
            yield break;

        Vector3 originalScale = card.localScale;
        Vector3 peakScale = originalScale * selectedCardPunchScale;
        float halfDuration = Mathf.Max(0.01f, selectedCardPunchDuration * 0.45f);
        float returnDuration = Mathf.Max(0.01f, selectedCardPunchDuration * 0.55f);

        yield return ScaleCard(card, originalScale, peakScale, halfDuration);
        yield return ScaleCard(card, peakScale, originalScale, returnDuration);
        card.localScale = originalScale;
    }

    private IEnumerator ScaleCard(Transform card, Vector3 from, Vector3 to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / duration);
            float eased = progress * progress * (3f - 2f * progress);
            card.localScale = Vector3.LerpUnclamped(from, to, eased);
            yield return null;
        }

        card.localScale = to;
    }

    private IEnumerator FadeGlow(GameObject targetGlow, bool fadeIn, float duration)
    {
        if (targetGlow == null)
            yield break;

        SpriteRenderer[] renderers = targetGlow.GetComponentsInChildren<SpriteRenderer>(true);
        if (renderers.Length == 0)
        {
            if (!fadeIn)
                targetGlow.SetActive(false);
            yield break;
        }

        float[] startAlphas = new float[renderers.Length];
        float[] endAlphas = new float[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            Color color = renderers[i].color;
            endAlphas[i] = fadeIn ? color.a : 0f;
            startAlphas[i] = fadeIn ? 0f : color.a;
            if (fadeIn)
            {
                color.a = 0f;
                renderers[i].color = color;
            }
        }

        float elapsed = 0f;
        float fadeDuration = Mathf.Max(0.01f, duration);
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / fadeDuration);
            float eased = progress * progress * (3f - 2f * progress);

            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] == null)
                    continue;

                Color color = renderers[i].color;
                color.a = Mathf.Lerp(startAlphas[i], endAlphas[i], eased);
                renderers[i].color = color;
            }

            yield return null;
        }

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null)
                continue;

            Color color = renderers[i].color;
            color.a = endAlphas[i];
            renderers[i].color = color;
        }

        if (!fadeIn)
            targetGlow.SetActive(false);
    }

    private bool PlayState(Animator animator, string stateName)
    {
        int stateHash = Animator.StringToHash(stateName);

        if (!animator.HasState(0, stateHash))
        {
            GameLogger.Error(
                "CardSelect",
                "ไม่พบ Animation State: " + stateName
            );
            return false;
        }

        animator.Play(stateHash, 0, 0f);
        return true;
    }

    private IEnumerator WaitForAnimations(
        string salmonState,
        string platuState)
    {
        // ให้ Animator อัปเดตสถานะหลังจากสั่ง Play
        yield return null;

        float elapsed = 0f;

        while (elapsed < animTimeout)
        {
            bool salmonDone = IsAnimationFinished(salmonAnimator, salmonState);
            bool platuDone = IsAnimationFinished(platuAnimator, platuState);

            if (salmonDone && platuDone)
                yield break;

            elapsed += Time.deltaTime;
            yield return null;
        }

        GameLogger.Warn(
            "CardSelect",
            "Animation wait timed out. Loading next scene."
        );
    }

    private bool IsAnimationFinished(
        Animator animator,
        string stateName)
    {
        if (animator == null)
            return true;

        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);

        if (!state.IsName(stateName))
            return false;

        return state.normalizedTime >= 1f &&
               !animator.IsInTransition(0);
    }

    private void CancelSelection()
    {
        activeSelection = null;
        isSelecting = false;
    }


    private void LoadNextScene()
    {
        if (string.IsNullOrWhiteSpace(nextScene))
        {
            GameLogger.Error("CardSelect", "Next scene is empty.");
            CancelSelection();
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(nextScene))
        {
            GameLogger.Error(
                "CardSelect",
                "Scene not found in Build Profiles: " + nextScene
            );
            CancelSelection();
            return;
        }

        if (TransitionLauncher.Instance != null)
        {
            TransitionLauncher.Instance.LoadScene(nextScene);
        }
        else
        {
            GameLogger.Error(
                "CardSelect",
                "TransitionLauncher ไม่พบใน SelectCharacter"
            );

            CancelSelection();
        }
    }

}
