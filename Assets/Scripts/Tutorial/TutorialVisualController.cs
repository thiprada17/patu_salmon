
using UnityEngine;

public class TutorialVisualController : MonoBehaviour
{
    [Header("Finger Pointer")]
    [SerializeField] private HandPointerController handPointer;

    [Header("Overlay Canvas")]
    [Tooltip("The Canvas containing the tutorial overlay.")]
    [SerializeField] private Canvas overlayCanvas;

    private Transform currentHighlightedTarget;

    // SpriteRenderer target settings
    private SpriteRenderer targetSpriteRenderer;
    private int originalSpriteSortingLayerID;
    private int originalSpriteSortingOrder;

    // Canvas target settings
    private Canvas targetCanvas;
    private bool addedCanvas;
    private bool originalOverrideSorting;
    private bool originalCanvasEnabled;
    private int originalCanvasSortingOrder;
    private int originalCanvasSortingLayerID;

    private enum TargetRenderType
    {
        None,
        SpriteRenderer,
        Canvas
    }

    private TargetRenderType currentRenderType =
        TargetRenderType.None;

    public void ShowStep(TutorialStepData step)
    {
        RestoreTargetOrder();

        if (step == null)
        {
            HideAll();
            return;
        }

        if (TutorialTargetRegistry.Instance == null)
        {
            Debug.LogWarning(
                "TutorialTargetRegistry is missing from the scene."
            );

            HideAll();
            return;
        }

        Transform target =
            TutorialTargetRegistry.Instance.FindTarget(step.TargetID);

        if (target == null)
        {
            Debug.LogWarning(
                "Tutorial target not found: " + step.TargetID
            );

            HideAll();
            return;
        }

        BringTargetToFront(target);

        if (handPointer != null)
        {
            handPointer.Configure(
                step.HandPointerSprite,
                step.HandOffset,
                step.HandAnimation
            );

            handPointer.SetTarget(target);
        }
        else
        {
            Debug.LogWarning(
                "HandPointerController is not assigned."
            );
        }
    }

    private void BringTargetToFront(Transform target)
    {
        if (target == null)
            return;

        if (overlayCanvas == null)
        {
            Debug.LogWarning(
                "Overlay Canvas is not assigned."
            );

            return;
        }

        currentHighlightedTarget = target;

        // CASE 1: The target is a SpriteRenderer.
        targetSpriteRenderer = target.GetComponent<SpriteRenderer>();

        if (targetSpriteRenderer != null)
        {
            currentRenderType = TargetRenderType.SpriteRenderer;

            // Save the original sorting settings.
            originalSpriteSortingLayerID =
                targetSpriteRenderer.sortingLayerID;

            originalSpriteSortingOrder =
                targetSpriteRenderer.sortingOrder;

            // Temporarily render it above the overlay.
            targetSpriteRenderer.sortingLayerID =
                overlayCanvas.sortingLayerID;

            targetSpriteRenderer.sortingOrder =
                overlayCanvas.sortingOrder + 1;

            return;
        }

        // CASE 2: The target is a UI element with its own Canvas.
        targetCanvas = target.GetComponent<Canvas>();
        addedCanvas = targetCanvas == null;

        if (addedCanvas)
        {
            targetCanvas = target.gameObject.AddComponent<Canvas>();
        }

        currentRenderType = TargetRenderType.Canvas;

        // Save the original Canvas settings.
        originalOverrideSorting = targetCanvas.overrideSorting;
        originalCanvasEnabled = targetCanvas.enabled;

        originalCanvasSortingOrder =
            targetCanvas.sortingOrder;

        originalCanvasSortingLayerID =
            targetCanvas.sortingLayerID;

        // Temporarily render it above the overlay Canvas.
        targetCanvas.overrideSorting = true;
        targetCanvas.sortingLayerID =
            overlayCanvas.sortingLayerID;

        targetCanvas.sortingOrder =
            Mathf.Max(overlayCanvas.sortingOrder, targetCanvas.sortingOrder) + 1;

        targetCanvas.enabled = true;
    }

    private void RestoreTargetOrder()
    {
        if (currentRenderType == TargetRenderType.SpriteRenderer)
        {
            if (targetSpriteRenderer != null)
            {
                targetSpriteRenderer.sortingLayerID =
                    originalSpriteSortingLayerID;

                targetSpriteRenderer.sortingOrder =
                    originalSpriteSortingOrder;
            }
        }
        else if (currentRenderType == TargetRenderType.Canvas)
        {
            if (targetCanvas != null)
            {
                if (addedCanvas)
                {
                    Destroy(targetCanvas);
                }
                else
                {
                    targetCanvas.overrideSorting =
                        originalOverrideSorting;

                    targetCanvas.enabled =
                        originalCanvasEnabled;

                    targetCanvas.sortingOrder =
                        originalCanvasSortingOrder;

                    targetCanvas.sortingLayerID =
                        originalCanvasSortingLayerID;
                }
            }
        }

        currentHighlightedTarget = null;
        targetSpriteRenderer = null;
        targetCanvas = null;
        addedCanvas = false;
        currentRenderType = TargetRenderType.None;
    }

    public void HideAll()
    {
        RestoreTargetOrder();

        if (handPointer != null)
            handPointer.Hide();
    }

    private void OnDisable()
    {
        RestoreTargetOrder();

        if (handPointer != null)
            handPointer.Hide();
    }
}
