
using UnityEngine;

public class Mat : MonoBehaviour
{
    private Cup leftCup;
    private Cup rightCup;

    [Header("Recipe Manager")]
    [SerializeField] private RecipeManager recipeManager;

    public void AttachLeftCup(Cup cup)
    {
        if (cup == null) return;

        leftCup = cup;
        CheckRecipe(leftCup);
    }

    public void RemoveLeftCup(Cup cup)
    {
        if (leftCup == cup)
        {
            leftCup = null;
        }
    }

    public void AttachRightCup(Cup cup)
    {
        if (cup == null) return;

        rightCup = cup;
        CheckRecipe(rightCup);
    }

    public void RemoveRightCup(Cup cup)
    {
        if (rightCup == cup)
        {
            rightCup = null;
        }
    }

    private void CheckRecipe(Cup cup)
    {
        if (cup == null) return;

        if (recipeManager == null)
        {
            GameLogger.Warn("Mat", "RecipeManager is not assigned.");
            return;
        }

        RecipeData recipe = recipeManager.GetRecipe(cup);

        if (recipe == null)
        {
            GameLogger.Warn("Mat", "Cup does not match any recipe.");
            return;
        }

        GameLogger.Info("Mat", "Cup recipe: " + recipe.recipeType);
        cup.SetFinishedRecipe(
            recipe.finishedCupSprite,
            recipe.recipeType
        );
    }
}
