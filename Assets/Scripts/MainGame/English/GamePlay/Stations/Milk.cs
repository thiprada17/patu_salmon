
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
        // ยกเลิกการปั๊มนมเมื่อ Dialogue หรือ Tutorial ล็อกเกม
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
            "Milk",
            "Cup attached to Milk machine."
        );
    }

    public void RemoveCup(Cup cup)
    {
        if (currentCup == cup)
        {
            // ยกเลิกการปั๊มหาก Cup ถูกถอดระหว่างทำงาน
            if (isPumping)
                CancelPumping();

            currentCup = null;

            GameLogger.DebugLog(
                "Milk",
                "Cup removed from Milk machine."
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
                "Milk",
                "Please wait, milk is pumping."
            );
            return;
        }

        if (currentCup == null)
        {
            GameLogger.Warn(
                "Milk",
                "No Cup attached."
            );
            return;
        }

        if (currentCup.IsFull)
        {
            GameLogger.Warn(
                "Milk",
                "Cup is full."
            );

            MachineShakeManager.Shake(gameObject);
            return;
        }

        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.PlayOneShot(audioSource.clip);
        }

        // เริ่ม Coroutine เพียงครั้งเดียว
        pumpingCoroutine = StartCoroutine(PumpMilk());
    }

    private IEnumerator PumpMilk()
    {
        if (GameplayInputLock.IsLocked || currentCup == null)
        {
            FinishPumping();
            yield break;
        }

        isPumping = true;

        // จดจำ Cup ที่ใช้ในรอบนี้
        Cup targetCup = currentCup;

        // เปลี่ยนเป็นภาพปุ่มที่ถูกกด
        if (pumpNormal != null)
            pumpNormal.enabled = false;

        if (pumpPressed != null)
            pumpPressed.enabled = true;

        // แสดงแอนิเมชันนม
        if (milkAnimation != null)
            milkAnimation.enabled = true;

        if (milkAnimator != null)
        {
            milkAnimator.enabled = true;
            milkAnimator.Play("milkanimation", 0, 0f);
            milkAnimator.Update(0f);
        }

        float elapsed = 0f;

        while (elapsed < pumpingDuration)
        {
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

        // เติมนมเฉพาะเมื่อ Cup ใบเดิมยังอยู่และเกมไม่ถูกล็อก
        if (!GameplayInputLock.IsLocked &&
            targetCup != null &&
            currentCup == targetCup)
        {
            bool success = targetCup.AddIngredient(
                IngredientType.Milk
            );

            if (success)
            {
                GameLogger.Info(
                    "Milk",
                    "Milk added to Cup."
                );
            }
            else
            {
                GameLogger.Warn(
                    "Milk",
                    "Could not add Milk to Cup."
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
            "Pumping cancelled."
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
