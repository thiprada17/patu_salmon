using UnityEngine;

public class Cocoa : MonoBehaviour
{
    private Cup currentCup;

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

        // เพิ่ม Cocoa เข้า Cup
        bool success = currentCup.AddIngredient( IngredientType.Cocoa );

        if (success)
        {
            Debug.Log("Cocoa added to Cup!");
        }
        else
        {
            Debug.Log("ไม่สามารถใส่ Cocoa ลง Cup ได้");
        }
    }
}