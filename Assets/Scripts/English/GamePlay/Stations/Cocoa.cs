using UnityEngine;
using System.Collections;

public class Cocoa : MonoBehaviour
{
    private Cup currentCup;

    private bool isPumping = false;

    [Header("Pump")]
    [SerializeField] private SpriteRenderer pumpNormal;
    [SerializeField] private SpriteRenderer pumpPressed;

    [Header("Cocoa Animation")]
    [SerializeField] private Animator cocoaAnimator;
    [SerializeField] private SpriteRenderer cocoaAnimation;

    private void Start()
    {
        if (pumpNormal != null)
            pumpNormal.enabled = true;

        if (pumpPressed != null)
            pumpPressed.enabled = false;

        if (cocoaAnimation != null)
            cocoaAnimation.enabled = false;
    }
    public void AttachCup(Cup cup)
    {
        currentCup = cup;

        Debug.Log("Cup attached to Cocoa");
    }


    public void RemoveCup(Cup cup)
    {
        if (currentCup == cup)
        {
            currentCup = null;

            Debug.Log("Cup removed from Cocoa");
        }
    }


    private void OnMouseDown()
    {
        // เช็กว่ามี Cup
        if (currentCup == null)
        {
            Debug.Log("ยังไม่ได้ใส่ Cup");
            return;
        }

        if (isPumping)
        {
            Debug.Log("cocoa waiting");
            return;
        }

        StartCoroutine(PumpCocoa());
    }

    private IEnumerator PumpCocoa()
    {
        isPumping = true;

        // เปลี่ยน Pump เป็นรูปกด
        if (pumpNormal != null)
            pumpNormal.enabled = false;

        if (pumpPressed != null)
            pumpPressed.enabled = true;

        // แสดง Milk Animation
        if (cocoaAnimation != null)
            cocoaAnimation.enabled = true;

        // เปิด Animator
        if (cocoaAnimator != null)
        {
            cocoaAnimator.enabled = true;

            // เริ่ม Animation ที่ Key แรก
            cocoaAnimator.Play("cocoaanimation", 0, 0f);

            // บังคับให้ Animator อัปเดตทันที
            cocoaAnimator.Update(0f);
        }

        // รอ Animation
        yield return new WaitForSeconds(1.3f);

        // เพิ่ม Milk เข้า Cup
        if (currentCup != null)
        {
            bool success = currentCup.AddIngredient(IngredientType.Cocoa);

            if (success)
                Debug.Log("Cocoa added to Cup");
            else
                Debug.Log("ไม่สามารถใส่ cocoa ลง Cup ได้");
        }

        // ซ่อน Animation
        if (cocoaAnimation != null)
            cocoaAnimation.enabled = false;

        // ปิด Animator เพื่อไม่ให้ Animation เล่นต่อ
        if (cocoaAnimator != null)
            cocoaAnimator.enabled = false;

        // กลับ Pump ปกติ
        if (pumpPressed != null)
            pumpPressed.enabled = false;

        if (pumpNormal != null)
            pumpNormal.enabled = true;

        isPumping = false;
    }
}