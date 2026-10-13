
using UnityEngine;

public class TutorialVisualController : MonoBehaviour
{
    [Header("Finger Pointer")]
    [SerializeField] private HandPointerController handPointer;

    [Header("Overlay Canvas")]
    [Tooltip("The Canvas that contains TutorialOverlay.")]
    [SerializeField] private Canvas overlayCanvas;

    private Transform currentHighlightedTarget;
    private Canvas targetCanvas;

    private bool addedCanvas;
    private bool originalOverrideSorting;
    private int originalSortingOrder;
    private int originalSortingLayerID;

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
                "Overlay Canvas is not assigned in TutorialVisualController."
            );
            return;
        }

        currentHighlightedTarget = target;

        // Use the existing Canvas if one is present.
        targetCanvas = target.GetComponent<Canvas>();
        addedCanvas = targetCanvas == null;

        if (addedCanvas)
            targetCanvas = target.gameObject.AddComponent<Canvas>();

        // Remember the original settings.
        originalOverrideSorting = targetCanvas.overrideSorting;
        originalSortingOrder = targetCanvas.sortingOrder;
        originalSortingLayerID = targetCanvas.sortingLayerID;

        // Render this target above the tutorial overlay.
        targetCanvas.overrideSorting = true;
        targetCanvas.sortingLayerID = overlayCanvas.sortingLayerID;
        targetCanvas.sortingOrder = overlayCanvas.sortingOrder + 1;
    }

    private void RestoreTargetOrder()
    {
        if (targetCanvas == null)
        {
            currentHighlightedTarget = null;
            return;
        }

        if (addedCanvas)
        {
            // Remove only the Canvas this script added.
            Destroy(targetCanvas);
        }
        else
        {
            // Restore the Canvas that was already there.
            targetCanvas.overrideSorting = originalOverrideSorting;
            targetCanvas.sortingOrder = originalSortingOrder;
            targetCanvas.sortingLayerID = originalSortingLayerID;
        }

        targetCanvas = null;
        currentHighlightedTarget = null;
        addedCanvas = false;
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
