using UnityEngine;

public class Milk : MonoBehaviour
{
    private Cup currentCup;


    public void AttachCup(Cup cup)
    {
        currentCup = cup;

        Debug.Log("Cup attached to milk");
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
        // เช็กว่ามี Cup
        if (currentCup == null)
        {
            Debug.Log("ยังไม่ได้ใส่ Cup");
            return;
        }

        // เพิ่ม Cocoa เข้า Cup
        bool success = currentCup.AddIngredient( IngredientType.Milk );

        if (success)
        {
            Debug.Log("milk added to Cup");
        }
        else
        {
            Debug.Log("ไม่สามารถใส่ milk ลง Cup ได้");
        }
    }
}