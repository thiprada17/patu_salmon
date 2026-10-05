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

        Debug.Log("Cup attached to water");
    }

    public void RemoveCup(Cup cup)
    {
        if (currentCup == cup)
        {
            currentCup = null;

            Debug.Log("Cup removed from water");
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
        if (currentCup == null)
        {
            Debug.Log("ยังไม่ได้ใส่ Cup");
            return;
        }

        if (isAddingWater)
        {
            Debug.Log("Water กำลังทำงานอยู่");
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
                Debug.Log("Water added to Cup");
            }
            else
            {
                Debug.Log("ไม่สามารถใส่ Water ลง Cup ได้");
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