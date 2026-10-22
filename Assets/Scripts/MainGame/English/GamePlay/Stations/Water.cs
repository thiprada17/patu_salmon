using UnityEngine;
using System.Collections;

public class Water : MonoBehaviour
{
    private Cup currentCup;
    private bool isAddingWater = false;
    private Coroutine waterCoroutine;

    [Header("Water Animation")]
    [SerializeField] private Animator waterAnimator;
    [SerializeField] private SpriteRenderer waterRenderer;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;

    [Header("Water Settings")]
    [SerializeField] private float addingWaterDuration = 1f;

    private void Start()
    {
        ResetWaterVisuals();
    }

    private void Update()
    {
        // ยกเลิกการจ่ายน้ำเมื่อ Tutorial ล็อกการเล่น
        if (isAddingWater && GameplayInputLock.IsLocked)
        {
            CancelAddingWater();
        }
    }

    public void AttachCup(Cup cup)
    {
        if (GameplayInputLock.IsLocked || isAddingWater)
            return;

        if (cup == null)
            return;

        currentCup = cup;

        GameLogger.DebugLog(
            "Water",
            "Cup attached to Water."
        );
    }

    public void RemoveCup(Cup cup)
    {
        if (currentCup == cup)
        {
            if (isAddingWater)
                CancelAddingWater();

            currentCup = null;

            GameLogger.DebugLog(
                "Water",
                "Cup removed from Water."
            );
        }
    }

    private void OnMouseDown()
    {
        if (GameplayInputLock.IsLocked)
            return;

        if (isAddingWater)
        {
            GameLogger.DebugLog(
                "Water",
                "Water is being added, please wait."
            );
            return;
        }

        if (currentCup == null)
        {
            GameLogger.Warn(
                "Water",
                "No Cup attached."
            );
            return;
        }

        if (currentCup.IsFull)
        {
            GameLogger.Warn(
                "Water",
                "Cup is full."
            );

            MachineShakeManager.Shake(gameObject);
            return;
        }

        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.PlayOneShot(audioSource.clip);
        }

        waterCoroutine = StartCoroutine(AddWater());
    }

    private IEnumerator AddWater()
    {
        if (GameplayInputLock.IsLocked || currentCup == null)
        {
            FinishAddingWater();
            yield break;
        }

        isAddingWater = true;

        // จดจำ Cup ที่ใช้ในรอบนี้
        Cup targetCup = currentCup;

        // แสดงแอนิเมชันน้ำ
        if (waterRenderer != null)
            waterRenderer.enabled = true;

        if (waterAnimator != null)
        {
            waterAnimator.enabled = true;
            waterAnimator.Play("wateranimation", 0, 0f);
            waterAnimator.Update(0f);
        }

        float elapsed = 0f;

        while (elapsed < addingWaterDuration)
        {
            // ยกเลิกหากเกมถูกล็อก หรือ Cup ถูกถอด/เปลี่ยนใบ
            if (GameplayInputLock.IsLocked ||
                targetCup == null ||
                currentCup != targetCup)
            {
                CancelAddingWater();
                yield break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // เติมน้ำเฉพาะเมื่อ Cup ใบเดิมยังติดอยู่
        if (!GameplayInputLock.IsLocked &&
            targetCup != null &&
            currentCup == targetCup)
        {
            bool success = targetCup.AddIngredient(
                IngredientType.Water
            );

            if (success)
            {
                GameLogger.Info(
                    "Water",
                    "Water added to Cup."
                );
            }
            else
            {
                GameLogger.Warn(
                    "Water",
                    "Could not add Water to Cup."
                );
            }
        }

        ResetWaterVisuals();
        FinishAddingWater();
    }

    private void CancelAddingWater()
    {
        if (waterCoroutine != null)
        {
            StopCoroutine(waterCoroutine);
            waterCoroutine = null;
        }

        ResetWaterVisuals();
        isAddingWater = false;

        GameLogger.DebugLog(
            "Water",
            "Water adding cancelled."
        );
    }

    private void ResetWaterVisuals()
    {
        if (waterRenderer != null)
            waterRenderer.enabled = false;

        if (waterAnimator != null)
            waterAnimator.enabled = false;
    }

    private void FinishAddingWater()
    {
        isAddingWater = false;
        waterCoroutine = null;
    }

    private void OnDisable()
    {
        if (waterCoroutine != null)
        {
            StopCoroutine(waterCoroutine);
            waterCoroutine = null;
        }

        isAddingWater = false;
        ResetWaterVisuals();
    }
}