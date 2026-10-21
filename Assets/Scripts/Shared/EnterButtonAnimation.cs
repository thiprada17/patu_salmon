
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class EnterButtonAnimation : MonoBehaviour,
    IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [Header("Button Animation")]
    [SerializeField] private RectTransform buttonRoot;
    [SerializeField] private float pressedScale = 0.92f;
    [SerializeField] private float duration = 0.08f;

    private Vector3 originalScale;
    private Coroutine animationCoroutine;

    private void Awake()
    {
        if (buttonRoot == null)
            buttonRoot = GetComponent<RectTransform>();

        if (buttonRoot != null)
            originalScale = buttonRoot.localScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        AnimateTo(originalScale * pressedScale);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        AnimateTo(originalScale);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        AnimateTo(originalScale);
    }

    private void AnimateTo(Vector3 targetScale)
    {
        if (buttonRoot == null)
            return;

        if (animationCoroutine != null)
            StopCoroutine(animationCoroutine);

        animationCoroutine = StartCoroutine(ScaleAnimation(targetScale));
    }

    private IEnumerator ScaleAnimation(Vector3 targetScale)
    {
        Vector3 startScale = buttonRoot.localScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / duration);
            // Ease Out ให้การเคลื่อนไหวนุ่มนวล
            t = 1f - Mathf.Pow(1f - t, 3f);

            buttonRoot.localScale = Vector3.Lerp(
                startScale, targetScale, t
            );

            yield return null;
        }

        buttonRoot.localScale = targetScale;
        animationCoroutine = null;
    }

    private void OnDisable()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }

        if (buttonRoot != null)
            buttonRoot.localScale = originalScale;
    }
}
