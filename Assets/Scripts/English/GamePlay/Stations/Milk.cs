using UnityEngine;
using System.Collections;

public class Milk : MonoBehaviour
{
    private Cup currentCup;

    private bool isPumping = false;

    [Header("Pump")]
    [SerializeField] private SpriteRenderer pumpNormal;
    [SerializeField] private SpriteRenderer pumpPressed;

    [Header("Milk Animation")]
    [SerializeField] private Animator milkAnimator;
    [SerializeField] private SpriteRenderer milkAnimation;

    private void Start()
    {
        if (pumpNormal != null)
            pumpNormal.enabled = true;

        if (pumpPressed != null)
            pumpPressed.enabled = false;

        if (milkAnimation != null)
            milkAnimation.enabled = false;
    }

    public void AttachCup(Cup cup)
    {
        currentCup = cup;

    }

    public void RemoveCup(Cup cup)
    {
        if (currentCup == cup)
        {
            currentCup = null;

            Debug.Log("Cup removed from milk");
        }
    }

    private void OnMouseDown()
    {
        // ยังไม่มี Cup
        if (currentCup == null)
        {
            Debug.Log("ยังไม่ได้ใส่ Cup");
            return;
        }

  
        if (isPumping)
        {
            Debug.Log("pls wait milk");
            return;
        }

        StartCoroutine(PumpMilk());
    }

    private IEnumerator PumpMilk()
    {
        isPumping = true;

        // เปลี่ยน Pump เป็นรูปกด
        if (pumpNormal != null)
            pumpNormal.enabled = false;

        if (pumpPressed != null)
            pumpPressed.enabled = true;

        // แสดง Milk Animation
        if (milkAnimation != null)
            milkAnimation.enabled = true;

        // เปิด Animator
        if (milkAnimator != null)
        {
            milkAnimator.enabled = true;

            // เริ่ม Animation ที่ Key แรก
            milkAnimator.Play("milkanimation", 0, 0f);

            // บังคับให้ Animator อัปเดตทันที
            milkAnimator.Update(0f);
        }

        // รอ Animation
        yield return new WaitForSeconds(1.3f);

        // เพิ่ม Milk เข้า Cup
        if (currentCup != null)
        {
            bool success = currentCup.AddIngredient(IngredientType.Milk);

            if (success)
                Debug.Log("Milk added to Cup");
            else
                Debug.Log("ไม่สามารถใส่ milk ลง Cup ได้");
        }

        // ซ่อน Animation
        if (milkAnimation != null)
            milkAnimation.enabled = false;

        // ปิด Animator เพื่อไม่ให้ Animation เล่นต่อ
        if (milkAnimator != null)
            milkAnimator.enabled = false;

        // กลับ Pump ปกติ
        if (pumpPressed != null)
            pumpPressed.enabled = false;

        if (pumpNormal != null)
            pumpNormal.enabled = true;

        isPumping = false;
    }
}