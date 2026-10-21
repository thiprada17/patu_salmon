
using UnityEngine;
using System.Collections;

public class Cocoa : MonoBehaviour
{
    private Cup currentCup;
    private bool isPumping = false;
    private Coroutine pumpingCoroutine;

    [Header("Pump")]
    [SerializeField] private SpriteRenderer pumpNormal;
    [SerializeField] private SpriteRenderer pumpPressed;

    [Header("Cocoa Animation")]
    [SerializeField] private Animator cocoaAnimator;
    [SerializeField] private SpriteRenderer cocoaAnimation;

    private void Start()
    {
        ResetPumpVisuals();
    }

    public void AttachCup(Cup cup)
    {
        if (GameplayInputLock.IsLocked)
            return;

        currentCup = cup;
        GameLogger.DebugLog("Cocoa", "Cup attached to Cocoa");
    }

    public void RemoveCup(Cup cup)
    {
        if (currentCup == cup)
        {
            currentCup = null;
            GameLogger.DebugLog("Cocoa", "Cup removed from Cocoa");
        }
    }

    private void OnMouseDown()
    {
        if (GameplayInputLock.IsLocked)
            return;

        if (currentCup == null)
        {
            GameLogger.Warn("Cocoa", "ยังไม่ได้ใส่ Cup");
            return;
        }

        if (isPumping)
        {
            GameLogger.DebugLog("Cocoa", "Cocoa is pumping, please wait");
            return;
        }

        pumpingCoroutine = StartCoroutine(PumpCocoa());
    }

    private IEnumerator PumpCocoa()
    {
        isPumping = true;

        Cup targetCup = currentCup;

        if (pumpNormal != null)
            pumpNormal.enabled = false;

        if (pumpPressed != null)
            pumpPressed.enabled = true;

        if (cocoaAnimation != null)
            cocoaAnimation.enabled = true;

        if (cocoaAnimator != null)
        {
            cocoaAnimator.enabled = true;
            cocoaAnimator.Play("cocoaanimation", 0, 0f);
            cocoaAnimator.Update(0f);
        }

        float elapsed = 0f;
        const float pumpDuration = 1.3f;

        while (elapsed < pumpDuration)
        {
            // Cancel dispensing if the tutorial begins.
            if (GameplayInputLock.IsLocked)
            {
                ResetPumpVisuals();
                isPumping = false;
                pumpingCoroutine = null;
                yield break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Only add cocoa if the same cup remains attached
        // and gameplay is still unlocked.
        if (!GameplayInputLock.IsLocked &&
            targetCup != null &&
            currentCup == targetCup)
        {
            bool success = targetCup.AddIngredient(IngredientType.Cocoa);

            if (success)
            {
                GameLogger.DebugLog("Cocoa", "Cocoa added to Cup");
            }
            else
            {
                GameLogger.DebugLog(
                    "Cocoa",
                    "ไม่สามารถใส่ cocoa ลง Cup ได้"
                );
            }
        }

        ResetPumpVisuals();

        isPumping = false;
        pumpingCoroutine = null;
    }

    private void ResetPumpVisuals()
    {
        if (pumpNormal != null)
            pumpNormal.enabled = true;

        if (pumpPressed != null)
            pumpPressed.enabled = false;

        if (cocoaAnimation != null)
            cocoaAnimation.enabled = false;

        if (cocoaAnimator != null)
            cocoaAnimator.enabled = false;
    }

    private void OnDisable()
    {
        if (pumpingCoroutine != null)
        {
            StopCoroutine(pumpingCoroutine);
            pumpingCoroutine = null;
        }

        isPumping = false;
        ResetPumpVisuals();
    }
}
