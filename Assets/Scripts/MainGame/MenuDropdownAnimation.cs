using System.Collections;
using UnityEngine;

public class MenuDropdownAnimation : MonoBehaviour
{
    [Header("Objects")]
    [SerializeField] private RectTransform icon2;
    [SerializeField] private RectTransform icon3;
    [SerializeField] private RectTransform icon4;
    [SerializeField] private RectTransform arrowButton;

    [Header("Panel Height")]
    [SerializeField] private RectTransform backgroundPanel;
    [SerializeField] private float openBottom = -8f;
    [SerializeField] private float closedBottom = 250f;

    [Header("Positions")]
    [SerializeField] private Vector2 icon2ClosedPosition;
    [SerializeField] private Vector2 icon2OpenPosition;
    [SerializeField] private Vector2 arrowClosedPosition;
    [SerializeField] private Vector2 arrowOpenPosition;
    [SerializeField] private Vector2 icon3ClosedPosition;
    [SerializeField] private Vector2 icon3OpenPosition;
    [SerializeField] private Vector2 icon4ClosedPosition;
    [SerializeField] private Vector2 icon4OpenPosition;

    [Header("Animation")]
    [SerializeField] private float duration = 0.25f;

    private Coroutine animationCoroutine;
    private bool isOpen = true;

    private void Awake()
    {
        if (icon2 != null) icon2.anchoredPosition = icon2OpenPosition;
        if (icon3 != null) icon3.anchoredPosition = icon3OpenPosition;
        if (icon4 != null) icon4.anchoredPosition = icon4OpenPosition;
        if (arrowButton != null) arrowButton.anchoredPosition = arrowOpenPosition;

        if (backgroundPanel != null)
        {
            Vector2 offsetMin = backgroundPanel.offsetMin;
            offsetMin.y = openBottom;
            backgroundPanel.offsetMin = offsetMin;
        }
    }

    public void ToggleDropdown()
    {
        isOpen = !isOpen;

        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);

        animationCoroutine = StartCoroutine(AnimateDropdown());
    }

    private IEnumerator AnimateDropdown()
    {
        Vector2 icon2Start = icon2 != null ? icon2.anchoredPosition : Vector2.zero;
        Vector2 icon3Start = icon3 != null ? icon3.anchoredPosition : Vector2.zero;
        Vector2 icon4Start = icon4 != null ? icon4.anchoredPosition : Vector2.zero;
        Vector2 arrowStart = arrowButton != null ? arrowButton.anchoredPosition : Vector2.zero;

        Vector2 icon2Target = isOpen ? icon2OpenPosition : icon2ClosedPosition;
        Vector2 icon3Target = isOpen ? icon3OpenPosition : icon3ClosedPosition;
        Vector2 icon4Target = isOpen ? icon4OpenPosition : icon4ClosedPosition;
        Vector2 arrowTarget = isOpen ? arrowOpenPosition : arrowClosedPosition;
        float bottomStart = backgroundPanel != null ? backgroundPanel.offsetMin.y : 0f;
        float bottomTarget = isOpen ? openBottom : closedBottom;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = duration > 0f ? Mathf.Clamp01(elapsed / duration) : 1f;
            t = t * t * (3f - 2f * t);

            if (backgroundPanel != null)
            {
                Vector2 offsetMin = backgroundPanel.offsetMin;
                offsetMin.y = Mathf.Lerp(bottomStart, bottomTarget, t);
                backgroundPanel.offsetMin = offsetMin;
            }
            if (icon2 != null) icon2.anchoredPosition = Vector2.Lerp(icon2Start, icon2Target, t);
            if (icon3 != null) icon3.anchoredPosition = Vector2.Lerp(icon3Start, icon3Target, t);
            if (icon4 != null) icon4.anchoredPosition = Vector2.Lerp(icon4Start, icon4Target, t);
            if (arrowButton != null) arrowButton.anchoredPosition = Vector2.Lerp(arrowStart, arrowTarget, t);

            yield return null;
        }

        if (backgroundPanel != null)
        {
            Vector2 finalOffset = backgroundPanel.offsetMin;
            finalOffset.y = bottomTarget;
            backgroundPanel.offsetMin = finalOffset;
        }
        if (icon2 != null) icon2.anchoredPosition = icon2Target;
        if (icon3 != null) icon3.anchoredPosition = icon3Target;
        if (icon4 != null) icon4.anchoredPosition = icon4Target;
        if (arrowButton != null) arrowButton.anchoredPosition = arrowTarget;

        animationCoroutine = null;
    }
}
