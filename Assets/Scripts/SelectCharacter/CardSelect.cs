using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class CardSelect : MonoBehaviour
{
    [Header("Character")]
    [SerializeField] private string characterName = "salmon";
    [SerializeField] private string nextScene = "idcard";

    [Header("Selection")]
    [SerializeField] private GameObject glow;
    [SerializeField] private float delay = 0.2f;

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

        bool isSalmon = characterName.Trim()
            .Equals("salmon", StringComparison.OrdinalIgnoreCase);

        bool isPlatu = characterName.Trim()
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

        PlayerPrefs.SetString(
            "SelectedCharacter",
            isSalmon ? "salmon" : "platu"
        );
        PlayerPrefs.Save();

        GameLogger.Info(
            "CardSelect",
            "Selected character: " + characterName
        );

        StartCoroutine(SelectRoutine(isSalmon));
    }

    private IEnumerator SelectRoutine(bool isSalmon)
    {
        if (glow != null)
            glow.SetActive(true);

        yield return new WaitForSeconds(delay);

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

        LoadNextScene();
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