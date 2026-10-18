
using UnityEngine;

public class TutorialVisualController : MonoBehaviour
{
    [Header("Finger Pointer")]
    [SerializeField] private HandPointerController handPointer;

    [Header("Overlay Canvas")]
    [Tooltip("The Canvas containing the tutorial overlay.")]
    [SerializeField] private Canvas overlayCanvas;

    [Header("Highlight Settings")]
    [Tooltip("Minimum sorting order for a highlighted SpriteRenderer.")]
    [SerializeField] private int highlightedSpriteOrder = 15;

    [Tooltip("Minimum sorting order for a highlighted UI Canvas.")]
    [SerializeField] private int highlightedCanvasOrder = 15;

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
        // Restore the previous target before highlighting another one.
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

            // The pointer tracks the target independently of sorting.
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
                "TutorialVisualController: Overlay Canvas is not assigned."
            );

            return;
        }

        currentHighlightedTarget = target;

        // CASE 1: The target uses a SpriteRenderer.
        targetSpriteRenderer = target.GetComponent<SpriteRenderer>();

        if (targetSpriteRenderer != null)
        {
            currentRenderType = TargetRenderType.SpriteRenderer;

            // Save the original sorting settings.
            originalSpriteSortingLayerID =
                targetSpriteRenderer.sortingLayerID;

            originalSpriteSortingOrder =
                targetSpriteRenderer.sortingOrder;

            // Move the sprite to the overlay's Sorting Layer.
            targetSpriteRenderer.sortingLayerID =
                overlayCanvas.sortingLayerID;

            // Place it above the overlay and normal CoffeeBar UI order.
            targetSpriteRenderer.sortingOrder =
                Mathf.Max(
                    overlayCanvas.sortingOrder + 1,
                    highlightedSpriteOrder
                );

            Debug.Log(
                $"Tutorial highlight: {target.name} | " +
                $"Sorting Layer: {targetSpriteRenderer.sortingLayerName} | " +
                $"Order: {targetSpriteRenderer.sortingOrder}"
            );

            return;
        }

        // CASE 2: The target is a UI element.
        // Reuse its Canvas, or temporarily add one if needed.
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
        originalCanvasSortingOrder = targetCanvas.sortingOrder;
        originalCanvasSortingLayerID = targetCanvas.sortingLayerID;

        // Temporarily place the target Canvas above the overlay.
        targetCanvas.overrideSorting = true;
        targetCanvas.sortingLayerID = overlayCanvas.sortingLayerID;

        targetCanvas.sortingOrder =
            Mathf.Max(
                overlayCanvas.sortingOrder + 1,
                highlightedCanvasOrder
            );

        targetCanvas.enabled = true;

        Debug.Log(
            $"Tutorial highlight: {target.name} | " +
            $"Canvas Sorting Layer: {targetCanvas.sortingLayerName} | " +
            $"Order: {targetCanvas.sortingOrder}"
        );
    }

    private void RestoreTargetOrder()
    {
        // Restore a SpriteRenderer target.
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
        // Restore a UI Canvas target.
        else if (currentRenderType == TargetRenderType.Canvas)
        {
            if (targetCanvas != null)
            {
                if (addedCanvas)
                {
                    // Remove only the Canvas added by this script.
                    Destroy(targetCanvas);
                }
                else
                {
                    targetCanvas.overrideSorting =
                        originalOverrideSorting;

                    targetCanvas.sortingOrder =
                        originalCanvasSortingOrder;

                    targetCanvas.sortingLayerID =
                        originalCanvasSortingLayerID;

                    targetCanvas.enabled =
                        originalCanvasEnabled;
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
