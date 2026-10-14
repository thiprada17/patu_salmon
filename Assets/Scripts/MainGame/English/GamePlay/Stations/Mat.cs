using UnityEngine;

public class Mat : MonoBehaviour
{
    private Cup currentCup;

    [Header("Recipe Manager")]
    [SerializeField]
    private RecipeManager recipeManager;

    public void AttachLeftCup(Cup cup)
    {
        currentCup = cup;
        CheckRecipe();
    }


    public void RemoveLeftCup(Cup cup)
    {
        if (currentCup == cup)
        {
            currentCup = null;
        }
    }

    public void AttachRightCup(Cup cup)
    {
        currentCup = cup;
        CheckRecipe();
    }


    public void RemoveRightCup(Cup cup)
    {
        if (currentCup == cup)
        {
            currentCup = null;
        }
    }



    private void CheckRecipe()
    {
        if (currentCup == null)
            return;


        if (recipeManager == null)
        {
            GameLogger.Warn("Mat", "ยังไม่ได้ใส่ RecipeManager");
            return;
        }


        RecipeData recipe =
            recipeManager.GetRecipe(currentCup);


        if (recipe == null)
        { 
            GameLogger.Warn("Mat", "Cup นี้ไม่ตรงกับสูตร");
            return;
        }

        GameLogger.Info("Mat", "Cup is " + recipe.recipeType);

        // เปลี่ยน Cup เป็นรูปเมนูสำเร็จ
        currentCup.SetFinishedRecipe(recipe.finishedCupSprite, recipe.recipeType);
    }
}