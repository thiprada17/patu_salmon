
using UnityEngine;
using UnityEngine.UI;

public class HandPointerController : MonoBehaviour
{
    [Header("Pointer References")]
    [SerializeField] private RectTransform handPointer;
    [SerializeField] private RectTransform pointerParent;
    [SerializeField] private Canvas pointerCanvas;
    [SerializeField] private Image pointerImage;

    [Header("Hand Appearance")]
    [SerializeField] private float tiltAngle = -25f;

    [Header("Bounce Animation")]
    [SerializeField] private float bounceHeight = 12f;
    [SerializeField] private float bounceSpeed = 4f;

    private Transform currentTarget;
    private Vector2 pointerOffset;

    private TutorialHandAnimation handAnimation =
        TutorialHandAnimation.Bounce;

    private void Awake()
    {
        if (handPointer == null)
            handPointer = GetComponent<RectTransform>();

        if (handPointer != null && pointerImage == null)
            pointerImage = handPointer.GetComponent<Image>();

        if (handPointer != null && pointerParent == null)
            pointerParent = handPointer.parent as RectTransform;

        if (handPointer != null && pointerCanvas == null)
            pointerCanvas = handPointer.GetComponentInParent<Canvas>();

        Hide();
    }

    private void LateUpdate()
    {
        if (currentTarget == null ||
            handPointer == null ||
            pointerParent == null)
            return;

        Vector3 targetWorldPosition;

        // UI target: use its RectTransform position.
        RectTransform targetRect =
            currentTarget as RectTransform;

        if (targetRect != null)
        {
            targetWorldPosition = targetRect.position;
        }
        else
        {
            // World-space target: use its Transform position.
            targetWorldPosition = currentTarget.position;
        }

        // Convert the target's position to screen coordinates.
        Camera targetCamera = null;

        if (targetRect != null)
        {
            Canvas targetCanvas =
                targetRect.GetComponentInParent<Canvas>();

            if (targetCanvas != null &&
                targetCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                targetCamera = targetCanvas.worldCamera;
            }
        }
        else
        {
            // World objects must use the camera that renders the scene.
            targetCamera = Camera.main;
        }

        Vector2 screenPosition =
            RectTransformUtility.WorldToScreenPoint(
                targetCamera,
                targetWorldPosition
            );

        // Convert screen coordinates into the pointer parent's UI space.
        Camera pointerCamera = null;

        if (pointerCanvas != null &&
            pointerCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            pointerCamera = pointerCanvas.worldCamera;
        }

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            pointerParent,
            screenPosition,
            pointerCamera,
            out Vector2 localPosition))
        {
            return;
        }

        // Preserve the existing bounce animation.
        float bounce = 0f;

        if (handAnimation == TutorialHandAnimation.Bounce)
        {
            bounce = Mathf.Abs(
                Mathf.Sin(Time.unscaledTime * bounceSpeed)
            ) * bounceHeight;
        }

        handPointer.anchoredPosition =
            localPosition + pointerOffset +
            new Vector2(0f, bounce);

        // Preserve the hand's tilt.
        handPointer.localRotation =
            Quaternion.Euler(0f, 0f, tiltAngle);

        if (!handPointer.gameObject.activeSelf)
            handPointer.gameObject.SetActive(true);
    }


    public void SetTarget(Transform target)
    {
        currentTarget = target;

        if (handPointer != null)
            handPointer.gameObject.SetActive(target != null);
    }

    public void Configure(
        Sprite sprite,
        Vector2 offset,
        TutorialHandAnimation animation)
    {
        pointerOffset = offset;
        handAnimation = animation;

        if (pointerImage != null && sprite != null)
            pointerImage.sprite = sprite;
    }

    public void Hide()
    {
        currentTarget = null;

        if (handPointer != null)
            handPointer.gameObject.SetActive(false);
    }
}
