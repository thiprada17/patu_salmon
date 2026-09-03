using UnityEngine;

public class Mat : MonoBehaviour
{
    private Cup currentCup;

    [Header("Recipe Manager")]
    [SerializeField]
    private RecipeManager recipeManager;

    public void AttachLeftCup(Cup cup)
    {
        Debug.Log("Cup อยู่ Slot ซ้าย");

        currentCup = cup;

        CheckRecipe();
    }


    public void RemoveLeftCup(Cup cup)
    {
        if (currentCup == cup)
        {
            currentCup = null;

            Debug.Log("Cup ออกจาก Slot ซ้าย");
        }
    }

    public void AttachRightCup(Cup cup)
    {
        Debug.Log("Cup อยู่ Slot ขวา");

        currentCup = cup;

        CheckRecipe();
    }


    public void RemoveRightCup(Cup cup)
    {
        if (currentCup == cup)
        {
            currentCup = null;

            Debug.Log("Cup ออกจาก Slot ขวา");
        }
    }



    private void CheckRecipe()
    {
        if (currentCup == null)
            return;


        if (recipeManager == null)
        {
            Debug.LogError("ยังไม่ได้ใส่ RecipeManager");
            return;
        }


        RecipeData recipe =
            recipeManager.GetRecipe(currentCup);


        if (recipe == null)
        {
            Debug.Log("Cup นี้ไม่ตรงกับสูตร");
            return;
        }


        Debug.Log("Cup is " + recipe.recipeType);


        // เปลี่ยน Cup เป็นรูปเมนูสำเร็จ
        currentCup.SetFinishedRecipe(recipe.finishedCupSprite);
    }
}