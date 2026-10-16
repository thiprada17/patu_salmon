
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
        // ไม่เปิดหรือปิด Active ของ Panel และ Icon
        icon2.anchoredPosition = icon2OpenPosition;
        arrowButton.anchoredPosition = arrowOpenPosition;
        backgroundPanel.offsetMin = new Vector2(backgroundPanel.offsetMin.x,openBottom);
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
        Vector2 icon2Start = icon2.anchoredPosition;
        Vector2 icon3Start = icon3.anchoredPosition;
        //Vector2 icon4Start = icon4.anchoredPosition;
        Vector2 arrowStart = arrowButton.anchoredPosition;

        Vector2 icon2Target = isOpen
            ? icon2OpenPosition
            : icon2ClosedPosition;

        Vector2 icon3Target = isOpen
            ? icon3OpenPosition
            : icon3ClosedPosition;

        //Vector2 icon4Target = isOpen
        //    ? icon4OpenPosition
        //    : icon4ClosedPosition;

        Vector2 arrowTarget = isOpen
            ? arrowOpenPosition
            : arrowClosedPosition;

        float bottomStart = backgroundPanel.offsetMin.y;
        float bottomTarget = isOpen ? openBottom : closedBottom;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / duration);
            t = t * t * (3f - 2f * t);

            Vector2 offsetMin = backgroundPanel.offsetMin;
            offsetMin.y = Mathf.Lerp(bottomStart, bottomTarget, t);
            backgroundPanel.offsetMin = offsetMin;

            icon2.anchoredPosition = Vector2.Lerp(icon2Start, icon2Target, t);
            icon3.anchoredPosition = Vector2.Lerp(icon3Start, icon3Target, t);
            //icon4.anchoredPosition = Vector2.Lerp(icon4Start, icon4Target, t);
            arrowButton.anchoredPosition = Vector2.Lerp(arrowStart, arrowTarget, t);

            yield return null;
        }
        Vector2 finalOffset = backgroundPanel.offsetMin;
        finalOffset.y = bottomTarget;
        backgroundPanel.offsetMin = finalOffset;
        icon2.anchoredPosition = icon2Target;
        icon3.anchoredPosition = icon3Target;
        //icon4.anchoredPosition = icon4Target;
        arrowButton.anchoredPosition = arrowTarget;

        animationCoroutine = null;
    }
}
