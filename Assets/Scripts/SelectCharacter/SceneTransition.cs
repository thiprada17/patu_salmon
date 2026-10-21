
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance { get; private set; }

    [SerializeField] private Animator animator;

    [Header("Layer 0: การเคลื่อนที่")]
    [SerializeField] private string inState = "flyin";
    [SerializeField] private string outState = "flyout";
    [SerializeField] private string idleState = "idle";

    [Header("Layer 1: เครื่องบินขยับ (วนตลอด)")]
    [SerializeField] private string planeMoveState = "plane_move";
    [SerializeField] private int planeMoveLayer = 1;

    [Header("Animation Settings")]
    [SerializeField] private float timeout = 5f;

    private bool isLoading;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (animator == null)
        {
            GameLogger.Error(
                "SceneTransition",
                "Animator is not assigned."
            );
            return;
        }

        Play(idleState);
        PlayPlaneMove();
    }

    public void LoadScene(string sceneName)
    {
        if (isLoading)
        {
            GameLogger.Warn(
                "SceneTransition",
                "Scene transition is already running."
            );
            return;
        }

        if (string.IsNullOrWhiteSpace(sceneName))
        {
            GameLogger.Error(
                "SceneTransition",
                "Scene name is empty."
            );
            return;
        }

        StartCoroutine(LoadRoutine(sceneName));
    }

    private IEnumerator LoadRoutine(string sceneName)
    {
        isLoading = true;

        // 1. บินเข้ามาบังจอ
        Play(inState);
        yield return WaitFinished(inState);

        // 2. โหลด Scene ใหม่จนเสร็จ ขณะที่เครื่องบินบังจอ
        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);

        if (op == null)
        {
            GameLogger.Error(
                "SceneTransition",
                "ไม่สามารถเริ่มโหลด Scene: " + sceneName
            );

            Play(idleState);
            isLoading = false;
            yield break;
        }


        while (!op.isDone)
        {
            yield return null;
        }

        // รอให้ Scene ใหม่ผ่านช่วงเริ่มต้นหลายเฟรม
        // เครื่องบินยังบังจออยู่ระหว่างรอ
        yield return new WaitForEndOfFrame();
        yield return null;
        yield return new WaitForEndOfFrame();
        yield return null;

        // เริ่ม flyout เพียงครั้งเดียว
        Play(outState);
        yield return WaitFinished(outState);

        // กลับไป idle
        Play(idleState);
        isLoading = false;

    }

    private void Play(string stateName)
    {
        if (animator == null)
        {
            GameLogger.Error(
                "SceneTransition",
                "Animator is not assigned."
            );
            return;
        }

        int hash = Animator.StringToHash(stateName);

        if (!animator.HasState(0, hash))
        {
            GameLogger.Error(
                "SceneTransition",
                "ไม่พบ State: " + stateName
            );
            return;
        }

        // เริ่ม Animation จากเฟรมแรกโดยไม่ใช้ CrossFade
        animator.Play(hash, 0, 0f);
    }

    private void PlayPlaneMove()
    {
        if (animator == null)
            return;

        int hash = Animator.StringToHash(planeMoveState);

        if (!animator.HasState(planeMoveLayer, hash))
        {
            GameLogger.Error(
                "SceneTransition",
                "ไม่พบ State: " + planeMoveState +
                " ที่ Layer " + planeMoveLayer
            );
            return;
        }

        animator.Play(hash, planeMoveLayer, 0f);
    }

    private IEnumerator WaitFinished(string stateName)
    {
        if (animator == null)
            yield break;

        int hash = Animator.StringToHash(stateName);
        float elapsed = 0f;

        // รอให้ Animator อัปเดต State หลังเรียก Play
        yield return null;

        while (elapsed < timeout)
        {
            if (animator == null)
                yield break;

            AnimatorStateInfo state =
                animator.GetCurrentAnimatorStateInfo(0);

            // รอจน Animation เล่นจบจริง
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
            "รอ Animation เกินเวลา: " + stateName
        );
    }
}
