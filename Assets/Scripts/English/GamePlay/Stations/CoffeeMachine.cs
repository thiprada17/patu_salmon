using UnityEngine;
using System.Collections;

public class CoffeeMachine : MonoBehaviour
{
    private Portfilter currentPortfilter;
    private Cup currentCup;

    private Vector3 originalPosition;

    // เช็กว่าเครื่องกำลังทำงานอยู่หรือไม่
    private bool isMakingCoffee = false;

    private void Start()
    {
        originalPosition = transform.localPosition;
    }

    public void AttachPortfilter(Portfilter portfilter)
    {
        currentPortfilter = portfilter;

        Debug.Log("Portfilter attached to Coffee Machine");
    }

    public void RemovePortfilter(Portfilter portfilter)
    {
        if (currentPortfilter == portfilter)
        {
            currentPortfilter = null;

            Debug.Log("Portfilter removed from Coffee Machine");
        }
    }

    public void AttachCup(Cup cup)
    {
        currentCup = cup;

        Debug.Log("Cup attached to Coffee Machine");
    }

    public void RemoveCup(Cup cup)
    {
        if (currentCup == cup)
        {
            currentCup = null;

            Debug.Log("Cup removed from Coffee Machine");
        }
    }
    private void OnMouseDown()
    {
        // ถ้ากำลังทำกาแฟอยู่
        if (isMakingCoffee)
        {
            Debug.Log("Coffee Machine is working...");
            return;
        }

        // เช็ก Portfilter
        if (currentPortfilter == null)
        {
            Debug.Log("ยังไม่ได้ใส่ Portfilter");
            return;
        }

        // เช็ก Coffee Ground
        if (!currentPortfilter.hasCoffeeGround)
        {
            Debug.Log("Need coffee ground");
            return;
        }

        // เช็ก Cup
        if (currentCup == null)
        {
            Debug.Log("ยังไม่ได้ใส่ Cup");
            return;
        }

        // เริ่มทำกาแฟ
        StartCoroutine(MakeCoffeeProcess());
    }

    private IEnumerator MakeCoffeeProcess()
    {
        isMakingCoffee = true;

        Debug.Log("Making coffee...");

        float duration = 3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-0.02f, 0.02f);
            float y = Random.Range(-0.02f, 0.02f);

            transform.localPosition =
                originalPosition + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;

            yield return null;
        }

        // กลับตำแหน่งเดิม
        transform.localPosition = originalPosition;



        Debug.Log("Coffee finished!");

        // เพิ่ม Coffee เข้า Cup
        bool success =
            currentCup.AddIngredient(
                IngredientType.Coffee
            );

        if (success)
        {
            Debug.Log("Coffee added to Cup!");

            // ลบ Coffee Ground ออกจาก Portfilter
            currentPortfilter.RemoveCoffeeGround();
        }
        else
        {
            Debug.Log("ไม่สามารถใส่ Coffee ลง Cup ได้");
        }

        isMakingCoffee = false;
    }
}