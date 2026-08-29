using UnityEngine;

public class Water : MonoBehaviour
{
    private Cup currentCup;


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


    private void OnMouseDown()
    {
        // เช็กว่ามี Cup
        if (currentCup == null)
        {
            Debug.Log("ยังไม่ได้ใส่ Cup");
            return;
        }

        // เพิ่ม Cocoa เข้า Cup
        bool success =
            currentCup.AddIngredient(
                IngredientType.Water
            );

        if (success)
        {
            Debug.Log("water added to Cup");
        }
        else
        {
            Debug.Log("ไม่สามารถใส่ water ลง Cup ได้");
        }
    }
}