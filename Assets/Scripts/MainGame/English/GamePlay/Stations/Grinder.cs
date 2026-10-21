
using UnityEngine;
using System.Collections;

public class Grinder : MonoBehaviour
{
    private Portfilter currentPortfilter;

    private Vector3 originalPosition;
    private Vector3 portfilterOriginalPosition;

    [Header("Coffee Animation")]
    [SerializeField] private Animator coffeeAnimator;
    [SerializeField] private SpriteRenderer coffeeRenderer;

    [Header("Grinding Settings")]
    [SerializeField] private float grindDuration = 3f;
    [SerializeField] private float shakeMagnitude = 0.02f;

    [Header("Portfilter Setup")]
    [SerializeField] private Portfilter portfilterToDetect;

    private bool isGrinding;
    private Coroutine grindCoroutine;

    private DraggableItem currentDraggable;
    private bool wasDraggableEnabled;

    private bool wasGameplayLocked;

    private void Start()
    {
        originalPosition = transform.localPosition;

        if (coffeeRenderer != null)
            coffeeRenderer.enabled = false;

        if (portfilterToDetect != null)
        {
            currentPortfilter = portfilterToDetect;
            portfilterOriginalPosition =
                portfilterToDetect.transform.localPosition;

            Debug.Log(
                $"[Grinder] Portfilter detected: {currentPortfilter.name}"
            );
        }
        else
        {
            Debug.LogError(
                "[Grinder] Portfilter To Detect is not assigned in the Inspector."
            );
        }
    }

    private void Update()
    {
        bool isLocked = GameplayInputLock.IsLocked;

        // Cancel grinding if gameplay becomes locked.
        if (isGrinding && isLocked)
        {
            CancelGrinding();
        }

        // Do not clear currentPortfilter when gameplay resumes.
        wasGameplayLocked = isLocked;
    }

    private void PlayCoffeeAnimation()
    {
        if (coffeeRenderer != null)
            coffeeRenderer.enabled = true;

        if (coffeeAnimator != null)
        {
            coffeeAnimator.enabled = true;
            coffeeAnimator.Play("grinderanimation", 0, 0f);
        }
    }

    private void ResetCoffeeAnimation()
    {
        if (coffeeRenderer != null)
            coffeeRenderer.enabled = false;

        if (coffeeAnimator != null)
            coffeeAnimator.enabled = false;
    }

    public void AttachPortfilter(Portfilter portfilter)
    {
        if (GameplayInputLock.IsLocked || isGrinding)
            return;

        if (portfilter == null)
            return;

        currentPortfilter = portfilter;
        portfilterOriginalPosition = portfilter.transform.localPosition;

        GameLogger.DebugLog(
            "Grinder",
            "Portfilter attached to Grinder."
        );
    }

    // Optional: call this from your portfilter's detach/drop logic
    // if it is removed from the grinder.
    public void DetachPortfilter(Portfilter portfilter)
    {
        if (currentPortfilter == portfilter)
        {
            currentPortfilter = null;

            GameLogger.DebugLog(
                "Grinder",
                "Portfilter detached from Grinder."
            );
        }
    }

    private void OnMouseDown()
    {
        if (GameplayInputLock.IsLocked)
            return;

        if (isGrinding)
            return;

        // Remove a destroyed or invalid reference.
        if (currentPortfilter == null)
        {
            GameLogger.Warn(
                "Grinder",
                "No Portfilter attached. Attach the Portfilter again."
            );
            return;
        }

        if (currentPortfilter.hasCoffeeGround)
        {
            GameLogger.Warn(
                "Grinder",
                "Portfilter already contains coffee grounds!"
            );
            return;
        }

        grindCoroutine = StartCoroutine(Grind(currentPortfilter));
    }

    private IEnumerator Grind(Portfilter portfilter)
    {
        if (GameplayInputLock.IsLocked || portfilter == null)
        {
            FinishGrinding();
            yield break;
        }

        isGrinding = true;

        currentDraggable = portfilter.GetComponent<DraggableItem>();

        if (currentDraggable != null)
        {
            wasDraggableEnabled = currentDraggable.enabled;
            currentDraggable.enabled = false;
        }

        originalPosition = transform.localPosition;
        portfilterOriginalPosition = portfilter.transform.localPosition;

        GameLogger.DebugLog("Grinder", "Grinding coffee...");

        PlayCoffeeAnimation();

        float elapsed = 0f;

        while (elapsed < grindDuration)
        {
            if (GameplayInputLock.IsLocked || portfilter == null)
            {
                CancelGrinding();
                yield break;
            }

            float x = Random.Range(-shakeMagnitude, shakeMagnitude);
            float y = Random.Range(-shakeMagnitude, shakeMagnitude);

            Vector3 shakeOffset = new Vector3(x, y, 0f);

            transform.localPosition = originalPosition + shakeOffset;
            portfilter.transform.localPosition =
                portfilterOriginalPosition + shakeOffset;

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (!GameplayInputLock.IsLocked && portfilter != null)
        {
            RestorePositions(portfilter);
            ResetCoffeeAnimation();

            portfilter.AddCoffeeGround();

            GameLogger.Info("Grinder", "Coffee grounds added!");
        }

        RestoreDraggable();
        FinishGrinding();
    }

    private void CancelGrinding()
    {
        if (grindCoroutine != null)
        {
            StopCoroutine(grindCoroutine);
            grindCoroutine = null;
        }

        RestorePositions(currentPortfilter);
        ResetCoffeeAnimation();
        RestoreDraggable();

        isGrinding = false;

        GameLogger.DebugLog(
            "Grinder",
            "Grinding cancelled because gameplay is locked."
        );
    }

    private void RestorePositions(Portfilter portfilter)
    {
        transform.localPosition = originalPosition;

        if (portfilter != null)
        {
            portfilter.transform.localPosition =
                portfilterOriginalPosition;
        }
    }

    private void RestoreDraggable()
    {
        if (currentDraggable != null)
        {
            currentDraggable.enabled = wasDraggableEnabled;
            currentDraggable = null;
        }
    }

    private void FinishGrinding()
    {
        isGrinding = false;
        grindCoroutine = null;
    }

    private void OnDisable()
    {
        if (grindCoroutine != null)
        {
            StopCoroutine(grindCoroutine);
            grindCoroutine = null;
        }

        RestorePositions(currentPortfilter);
        ResetCoffeeAnimation();
        RestoreDraggable();

        isGrinding = false;
    }
}
