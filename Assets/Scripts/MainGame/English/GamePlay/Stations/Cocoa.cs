
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

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;

    [Header("Pump Settings")]
    [SerializeField] private float pumpingDuration = 1.3f;

    private void Start()
    {
        ResetPumpVisuals();
    }

    private void Update()
    {
        // ยกเลิกการจ่าย Cocoa เมื่อ Tutorial ล็อกการเล่น
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

        GameLogger.DebugLog(
            "Cocoa",
            "Cup attached to Cocoa."
        );
    }

    public void RemoveCup(Cup cup)
    {
        if (currentCup == cup)
        {
            if (isPumping)
                CancelPumping();

            currentCup = null;

            GameLogger.DebugLog(
                "Cocoa",
                "Cup removed from Cocoa."
            );
        }
    }

    private void OnMouseDown()
    {
        if (GameplayInputLock.IsLocked)
            return;

        if (isPumping)
        {
            GameLogger.DebugLog(
                "Cocoa",
                "Cocoa is pumping, please wait."
            );
            return;
        }

        if (currentCup == null)
        {
            GameLogger.Warn(
                "Cocoa",
                "No Cup attached."
            );
            return;
        }

        if (currentCup.IsFull)
        {
            GameLogger.Warn(
                "Cocoa",
                "Cup is full."
            );

            MachineShakeManager.Shake(gameObject);
            return;
        }

        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.PlayOneShot(audioSource.clip);
        }

        pumpingCoroutine = StartCoroutine(PumpCocoa());
    }

    private IEnumerator PumpCocoa()
    {
        if (GameplayInputLock.IsLocked || currentCup == null)
        {
            FinishPumping();
            yield break;
        }

        isPumping = true;

        // จดจำ Cup ที่ใช้ในรอบนี้
        Cup targetCup = currentCup;

        // เปลี่ยนภาพปุ่มเป็นสถานะกด
        if (pumpNormal != null)
            pumpNormal.enabled = false;

        if (pumpPressed != null)
            pumpPressed.enabled = true;

        // แสดงแอนิเมชัน Cocoa
        if (cocoaAnimation != null)
            cocoaAnimation.enabled = true;

        if (cocoaAnimator != null)
        {
            cocoaAnimator.enabled = true;
            cocoaAnimator.Play("cocoaanimation", 0, 0f);
            cocoaAnimator.Update(0f);
        }

        float elapsed = 0f;

        while (elapsed < pumpingDuration)
        {
            // ยกเลิกหากเกมถูกล็อก หรือ Cup ถูกถอด/เปลี่ยนใบ
            if (GameplayInputLock.IsLocked ||
                targetCup == null ||
                currentCup != targetCup)
            {
                CancelPumping();
                yield break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // เติม Cocoa เฉพาะเมื่อ Cup ใบเดิมยังติดอยู่
        if (!GameplayInputLock.IsLocked &&
            targetCup != null &&
            currentCup == targetCup)
        {
            bool success = targetCup.AddIngredient(
                IngredientType.Cocoa
            );

            if (success)
            {
                GameLogger.Info(
                    "Cocoa",
                    "Cocoa added to Cup."
                );
            }
            else
            {
                GameLogger.Warn(
                    "Cocoa",
                    "Could not add Cocoa to Cup."
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
            "Cocoa",
            "Cocoa pumping cancelled."
        );
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
