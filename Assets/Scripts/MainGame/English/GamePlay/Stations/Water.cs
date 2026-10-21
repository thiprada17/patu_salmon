using UnityEngine;
using System.Collections;

public class Water : MonoBehaviour
{
    private Cup currentCup;
    private bool isAddingWater = false;

    [Header("Water Animation")]
    [SerializeField] private Animator waterAnimator;
    [SerializeField] private SpriteRenderer waterRenderer;

    public void AttachCup(Cup cup)
    {
        currentCup = cup;
        GameLogger.DebugLog("Water", "Cup attached to water");
    }

    public void RemoveCup(Cup cup)
    {
        if (currentCup == cup)
        {
            currentCup = null;

            GameLogger.DebugLog("Water", "Cup removed from water");
        }
    }

    private void Start()
    {
        if (waterRenderer != null)
        {
            waterRenderer.enabled = false;
        }

        if (waterAnimator != null)
        {
            waterAnimator.enabled = false;
        }

    }

    private void PlayWaterAnimation()
    {
        if (waterRenderer != null)
        {
            waterRenderer.enabled = true;
        }

        if (waterAnimator != null)
        {
            waterAnimator.enabled = true;

            waterAnimator.Play("wateranimation", 0, 0f);
            waterAnimator.Update(0f);
        }
    }

    private void OnMouseDown()
    {
        // Block gameplay interactions while the dialogue is active.
        if (GameplayInputLock.IsLocked)
            return;

        if (currentCup == null)
        {
            GameLogger.Warn("Water", "ยังไม่ได้ใส่ Cup");
            return;
        }

        if (isAddingWater)
        {
            GameLogger.DebugLog("Water", "Water กำลังทำงานอยู่");
            return;
        }

        StartCoroutine(AddWater());
    }


    private IEnumerator AddWater()
    {
        isAddingWater = true;

        PlayWaterAnimation();

        // รอให้ Animation เล่นก่อน
        yield return new WaitForSeconds(1f);

        if (currentCup != null)
        {
            bool success = currentCup.AddIngredient(IngredientType.Water);

            if (success)
            {
                GameLogger.Info("Water", "Water added to Cup");
            }
            else
            {
                GameLogger.Info("Water", "ไม่สามารถใส่ Water ลง Cup ได้");
            }
        }

        // ปิด Animation
        if (waterRenderer != null)
        {
            waterRenderer.enabled = false;
        }

        if (waterAnimator != null)
        {
            waterAnimator.enabled = false;
        }

        isAddingWater = false;
    }
}