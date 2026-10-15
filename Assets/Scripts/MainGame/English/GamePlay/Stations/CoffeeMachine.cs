using UnityEngine;
using System.Collections;

public class CoffeeMachine : MonoBehaviour
{
    private Portfilter currentPortfilter;
    private Cup currentCup;

    private Vector3 originalPosition;
    private Vector3 portfilterOriginalPosition;
    private Vector3 cupOriginalPosition;

    // เช็กว่าเครื่องกำลังทำงานอยู่หรือไม่
    private bool isMakingCoffee = false;

    private void Start()
    {
        originalPosition = transform.localPosition;
    }

    public void AttachPortfilter(Portfilter portfilter)
    {
        currentPortfilter = portfilter;
        portfilterOriginalPosition = portfilter.transform.localPosition;
        GameLogger.DebugLog("CoffeeMach", "Portfilter attached to Coffee Machine");
    }

    public void RemovePortfilter(Portfilter portfilter)
    {
        if (currentPortfilter == portfilter)
        {
            currentPortfilter = null;
            GameLogger.DebugLog("CoffeeMach", "Portfilter removed from Coffee Machine");
        }
    }

    public void AttachCup(Cup cup)
    {
        currentCup = cup;
        cupOriginalPosition = cup.transform.localPosition;
        GameLogger.DebugLog("CoffeeMach", "Cup attached to Coffee Machine");
    }

    public void RemoveCup(Cup cup)
    {
        if (currentCup == cup)
        {
            currentCup = null;
            GameLogger.DebugLog("CoffeeMach", "Cup removed from Coffee Machine");
        }
    }
    private void OnMouseDown()
    {
        // ถ้ากำลังทำกาแฟอยู่
        if (isMakingCoffee)
        {
            GameLogger.DebugLog("CoffeeMach", "Coffee Machine is working...");
            return;
        }

        // เช็ก Portfilter
        if (currentPortfilter == null)
        {
            GameLogger.Warn("CoffeeMach", "ยังไม่ได้ใส่ Portfilter");
            return;
        }

        // เช็ก Coffee Ground
        if (!currentPortfilter.hasCoffeeGround)
        {
            GameLogger.DebugLog("CoffeeMach", "Need coffee ground");
            return;
        }


        // เช็ก Cup
        if (currentCup == null)
        {
            GameLogger.Warn("CoffeeMach", "ยังไม่ได้ใส่ Cup");
            return;
        }

        if (currentCup.IsFull)
        {
            return;
        }

        // เริ่มทำกาแฟ
        StartCoroutine(MakeCoffeeProcess());
    }

    private IEnumerator MakeCoffeeProcess()
    {
        isMakingCoffee = true;

        float duration = 2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-0.02f, 0.02f);
            float y = Random.Range(-0.02f, 0.02f);

            transform.localPosition =
                originalPosition + new Vector3(x, y, 0);

            currentPortfilter.transform.localPosition = portfilterOriginalPosition + new Vector3(x, y, 0);

            currentCup.transform.localPosition = cupOriginalPosition + new Vector3(x, y, 0);
            elapsed += Time.deltaTime;

            yield return null;
        }

        // กลับตำแหน่งเดิม
        transform.localPosition = originalPosition;

        if (currentPortfilter != null)
        {
            currentPortfilter.transform.localPosition =
                portfilterOriginalPosition;
        }

        // เพิ่ม Coffee เข้า Cup
        bool success = currentCup.AddIngredient( IngredientType.Coffee );

        if (success)
        {
            GameLogger.DebugLog("CocCoffeeMachoa", "Coffee added to Cup!");

            // ลบ Coffee Ground ออกจาก Portfilter
            currentPortfilter.RemoveCoffeeGround();
        }
        else
        {
            GameLogger.DebugLog("CoffeeMach", "ไม่สามารถใส่ Coffee ลง Cup ได้");
        }

        isMakingCoffee = false;
    }
}