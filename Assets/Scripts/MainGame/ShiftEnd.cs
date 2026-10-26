using System.Collections;
using TMPro;
using UnityEngine;

public class ShiftEnd : MonoBehaviour
{
    [Header("Summary Text")]
    [SerializeField] private TMP_Text daysText;
    [SerializeField] private TMP_Text totalCoinsText;
    [SerializeField] private TMP_Text tipsText;
    [SerializeField] private TMP_Text outticketText;
    [SerializeField] private TMP_Text outrentText;
    [SerializeField] private TMP_Text TotalEarnText;
    [SerializeField] private GameObject firerplatu;
    [SerializeField] private GameObject firersalmon;
    [SerializeField] private GameObject shiftSummaryPopup;

    [Header("Summary Animation")]
    [SerializeField] private RectTransform approvalStamp;
    [SerializeField, Min(0f)] private float panelOpenDuration = 0.32f;
    [SerializeField, Min(0f)] private float stampDelay = 0.12f;
    [SerializeField, Min(0f)] private float stampZoomDuration = 0.16f;
    [SerializeField, Min(0f)] private float stampImpactDuration = 0.11f;
    [SerializeField, Min(0f)] private float stampSettleDuration = 0.14f;
    [SerializeField, Min(0f)] private float stampStartOffset = 180f;

    private CanvasGroup popupCanvasGroup;
    private CanvasGroup stampCanvasGroup;
    private RectTransform popupRect;
    private Vector3 popupBaseScale;
    private Vector3 stampBaseScale;
    private Vector2 stampBasePosition;
    private Quaternion stampBaseRotation;
    private Coroutine animationRoutine;

    public bool IsShowingSummary { get; private set; }

    private void Awake()
    {
        if (shiftSummaryPopup != null)
        {
            popupRect = shiftSummaryPopup.GetComponent<RectTransform>();
            popupCanvasGroup = GetOrAddCanvasGroup(shiftSummaryPopup);
            if (popupRect != null)
                popupBaseScale = popupRect.localScale;
        }

        if (approvalStamp != null)
        {
            stampCanvasGroup = GetOrAddCanvasGroup(approvalStamp.gameObject);
            stampBaseScale = approvalStamp.localScale;
            stampBasePosition = approvalStamp.anchoredPosition;
            stampBaseRotation = approvalStamp.localRotation;
            SetStampVisible(false);
        }
    }

    private static CanvasGroup GetOrAddCanvasGroup(GameObject target)
    {
        CanvasGroup group = target.GetComponent<CanvasGroup>();
        return group != null ? group : target.AddComponent<CanvasGroup>();
    }

    public void ShowSummary()
    {
        IsShowingSummary = true;

        if (CoinManager.Instance == null)
        {
            GameLogger.Error("ShiftEnd", "CoinManager is missing.");
            IsShowingSummary = false;
            return;
        }

        CoinManager coin = CoinManager.Instance;
        string character = PlayerPrefs.GetString("SelectedCharacter", "salmon");
        if (firersalmon != null) firersalmon.SetActive(character == "salmon");
        if (firerplatu != null) firerplatu.SetActive(character != "salmon");

        SetText(daysText, coin.WorkDays.ToString());
        SetText(totalCoinsText, coin.TodayOrderCoins.ToString());
        SetText(tipsText, coin.TodayTipCoins.ToString());
        SetText(outticketText, coin.TicketExpense.ToString());
        SetText(outrentText, coin.RentExpense.ToString());
        SetText(TotalEarnText, (coin.TodayNetEarning
            - coin.TicketExpense - coin.RentExpense).ToString());

        Time.timeScale = 0f;
        gameObject.SetActive(true);

        if (shiftSummaryPopup != null)
        {
            shiftSummaryPopup.SetActive(true);
            if (popupCanvasGroup != null) popupCanvasGroup.alpha = 0f;
            if (popupRect != null) popupRect.localScale = popupBaseScale * 0.88f;
        }

        ResetStamp();
        if (animationRoutine != null) StopCoroutine(animationRoutine);
        animationRoutine = StartCoroutine(PlaySummaryAnimation());
    }

    private static void SetText(TMP_Text target, string value)
    {
        if (target != null) target.text = value;
    }

    private IEnumerator PlaySummaryAnimation()
    {
        float elapsed = 0f;
        while (elapsed < panelOpenDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / panelOpenDuration);
            float eased = 1f - Mathf.Pow(1f - t, 3f);
            if (popupCanvasGroup != null) popupCanvasGroup.alpha = eased;
            if (popupRect != null)
                popupRect.localScale = Vector3.Lerp(popupBaseScale * 0.88f, popupBaseScale, eased);
            yield return null;
        }
        if (popupCanvasGroup != null) popupCanvasGroup.alpha = 1f;
        if (popupRect != null) popupRect.localScale = popupBaseScale;

        if (approvalStamp == null) yield break;
        if (stampDelay > 0f) yield return new WaitForSecondsRealtime(stampDelay);
        yield return AnimateStamp();
        animationRoutine = null;
    }

    private IEnumerator AnimateStamp()
    {
        Vector2 above = stampBasePosition + Vector2.up * stampStartOffset;
        Vector3 zoomScale = stampBaseScale * 1.65f;
        approvalStamp.anchoredPosition = above;
        approvalStamp.localScale = stampBaseScale * 0.15f;
        approvalStamp.localRotation = stampBaseRotation * Quaternion.Euler(0f, 0f, -12f);
        SetStampVisible(true);

        yield return TweenStamp(stampZoomDuration, t =>
        {
            float eased = t * t * (3f - 2f * t);
            approvalStamp.anchoredPosition = Vector2.Lerp(above, stampBasePosition + Vector2.up * 35f, eased);
            approvalStamp.localScale = Vector3.Lerp(stampBaseScale * 0.15f, zoomScale, eased);
            stampCanvasGroup.alpha = eased;
        });

        Vector2 impactPosition = stampBasePosition;
        yield return TweenStamp(stampImpactDuration, t =>
        {
            approvalStamp.anchoredPosition = Vector2.Lerp(stampBasePosition + Vector2.up * 35f, impactPosition, t);
            approvalStamp.localScale = Vector3.Lerp(zoomScale, stampBaseScale * 0.86f, t);
            approvalStamp.localRotation = Quaternion.Slerp(
                stampBaseRotation * Quaternion.Euler(0f, 0f, -12f), stampBaseRotation, t);
        });

        Vector3 compressedScale = stampBaseScale * 0.86f;
        yield return TweenStamp(stampSettleDuration, t =>
        {
            float bounce = 1f - Mathf.Pow(1f - t, 3f);
            approvalStamp.localScale = Vector3.Lerp(compressedScale, stampBaseScale, bounce);
            approvalStamp.anchoredPosition = Vector2.Lerp(impactPosition, stampBasePosition, bounce);
        });

        approvalStamp.anchoredPosition = stampBasePosition;
        approvalStamp.localScale = stampBaseScale;
        approvalStamp.localRotation = stampBaseRotation;
        stampCanvasGroup.alpha = 1f;
    }

    private IEnumerator TweenStamp(float duration, System.Action<float> update)
    {
        if (duration <= 0f)
        {
            update(1f);
            yield break;
        }
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            update(Mathf.Clamp01(elapsed / duration));
            yield return null;
        }
        update(1f);
    }

    private void ResetStamp()
    {
        if (approvalStamp == null) return;
        approvalStamp.anchoredPosition = stampBasePosition;
        approvalStamp.localScale = stampBaseScale;
        approvalStamp.localRotation = stampBaseRotation;
        SetStampVisible(false);
    }

    private void SetStampVisible(bool visible)
    {
        if (stampCanvasGroup == null) return;
        stampCanvasGroup.alpha = visible ? 1f : 0f;
        stampCanvasGroup.interactable = visible;
        stampCanvasGroup.blocksRaycasts = visible;
    }

    public void NextShift()
    {
        IsShowingSummary = false;
        if (animationRoutine != null)
        {
            StopCoroutine(animationRoutine);
            animationRoutine = null;
        }
        Time.timeScale = 1f;
        ResetStamp();
        if (popupCanvasGroup != null) popupCanvasGroup.alpha = 1f;
        if (popupRect != null) popupRect.localScale = popupBaseScale;
        if (shiftSummaryPopup != null) shiftSummaryPopup.SetActive(false);
        gameObject.SetActive(false);
    }
}
