
using UnityEngine;
using System.Collections;

public class Milk : MonoBehaviour
{
    private Cup currentCup;
    private bool isPumping = false;
    private Coroutine pumpingCoroutine;

    [Header("Pump")]
    [SerializeField] private SpriteRenderer pumpNormal;
    [SerializeField] private SpriteRenderer pumpPressed;

    [Header("Milk Animation")]
    [SerializeField] private Animator milkAnimator;
    [SerializeField] private SpriteRenderer milkAnimation;

    [Header("Pump Settings")]
    [SerializeField] private float pumpingDuration = 1.3f;

    private void Start()
    {
        ResetPumpVisuals();
    }

    private void Update()
    {
        // Cancel pumping if dialogue/tutorial locks gameplay.
        if (isPumping && GameplayInputLock.IsLocked)
        {
            CancelPumping();
        }
    }

    public void AttachCup(Cup cup)
    {
        if (GameplayInputLock.IsLocked || isPumping)
            return;

        if (cup == null)
            return;

        currentCup = cup;
    }

    public void RemoveCup(Cup cup)
    {
        if (currentCup == cup)
        {
            currentCup = null;
            GameLogger.DebugLog("Milk", "Cup removed from milk");
        }
    }

    private void OnMouseDown()
    {
        if (GameplayInputLock.IsLocked)
            return;

        if (isPumping)
        {
            GameLogger.DebugLog(
                "Milk",
                "Please wait, milk is pumping"
            );
            return;
        }

        if (currentCup == null)
        {
            GameLogger.Warn("Milk", "ยังไม่ได้ใส่ Cup");
            return;
        }

        pumpingCoroutine = StartCoroutine(PumpMilk());
    }

    private IEnumerator PumpMilk()
    {
        if (GameplayInputLock.IsLocked || currentCup == null)
            yield break;

        isPumping = true;

        // Remember the cup used for this pumping process.
        Cup targetCup = currentCup;

        // Change pump to pressed sprite.
        if (pumpNormal != null)
            pumpNormal.enabled = false;

        if (pumpPressed != null)
            pumpPressed.enabled = true;

        // Show milk animation.
        if (milkAnimation != null)
            milkAnimation.enabled = true;

        // Start animation from the beginning.
        if (milkAnimator != null)
        {
            milkAnimator.enabled = true;
            milkAnimator.Play("milkanimation", 0, 0f);
            milkAnimator.Update(0f);
        }

        float elapsed = 0f;

        while (elapsed < pumpingDuration)
        {
            if (GameplayInputLock.IsLocked)
            {
                CancelPumping();
                yield break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Add milk only if gameplay is unlocked and the same cup
        // is still attached.
        if (!GameplayInputLock.IsLocked &&
            targetCup != null &&
            currentCup == targetCup)
        {
            bool success = targetCup.AddIngredient(IngredientType.Milk);

            if (success)
            {
                GameLogger.Info("Milk", "Milk added to Cup");
            }
            else
            {
                GameLogger.Info(
                    "Milk",
                    "ไม่สามารถใส่ milk ลง Cup ได้"
                );
            }
        }

        ResetPumpVisuals();
        FinishPumping();
    }

    private void CancelPumping()
    {
        if (pumpingCoroutine != null)
        {
            StopCoroutine(pumpingCoroutine);
            pumpingCoroutine = null;
        }

        ResetPumpVisuals();
        isPumping = false;

        GameLogger.DebugLog(
            "Milk",
            "Pumping cancelled because gameplay is locked."
        );
    }

    private void ResetPumpVisuals()
    {
        if (milkAnimation != null)
            milkAnimation.enabled = false;

        if (milkAnimator != null)
            milkAnimator.enabled = false;

        if (pumpPressed != null)
            pumpPressed.enabled = false;

        if (pumpNormal != null)
            pumpNormal.enabled = true;
    }

    private void FinishPumping()
    {
        isPumping = false;
        pumpingCoroutine = null;
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
