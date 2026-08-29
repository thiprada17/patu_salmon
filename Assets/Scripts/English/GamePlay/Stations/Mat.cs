using UnityEngine;

public class Mat : MonoBehaviour
{
    private Cup currentCup;


    [Header("Recipe Manager")]
    [SerializeField]
    private RecipeManager recipeManager;


    public void AttachCup(Cup cup)
    {
        currentCup = cup;

        Debug.Log("Cup attached to mat");

        CheckRecipe();
    }


    public void RemoveCup(Cup cup)
    {
        if (currentCup == cup)
        {
            currentCup = null;

            Debug.Log("Cup removed from mat");
        }
    }


    private void CheckRecipe()
    {
        if (currentCup == null)
            return;


        if (recipeManager == null)
        {
            Debug.LogError(
                "ยังไม่ได้ใส่ RecipeManager"
            );

            return;
        }


        RecipeData recipe =
            recipeManager.GetRecipe(currentCup);


        if (recipe == null)
        {
            Debug.Log(
                "Cup นี้ไม่ตรงกับสูตร"
            );

            return;
        }


        Debug.Log(
            "Cup is " +
            recipe.recipeType
        );


        // เปลี่ยน Cup เป็นรูปเมนูสำเร็จ
        currentCup.SetFinishedRecipe(
            recipe.finishedCupSprite
        );
    }


}