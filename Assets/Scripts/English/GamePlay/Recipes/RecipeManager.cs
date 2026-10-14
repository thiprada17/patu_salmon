using UnityEngine;

public class RecipeManager : MonoBehaviour
{
    [SerializeField]
    private RecipeData[] recipes;


    public RecipeData GetRecipe(Cup cup)
    {
        if (cup == null)
        {
            GameLogger.Error("Recipe", "Cup is null!");
            return null;
        }

        foreach (RecipeData recipe in recipes)
        {
            if (IsCorrectRecipe(cup, recipe))
            {
                GameLogger.DebugLog("Recipe","Recipe Found: " + recipe.recipeType);
                return recipe;
            }
        }

        GameLogger.DebugLog("Recipe", "No matching recipe");

        return null;
    }


    private bool IsCorrectRecipe( Cup cup, RecipeData recipe)
    {
        if (recipe == null)
            return false;

        int coffeeCount = Count(cup, IngredientType.Coffee);
        int milkCount = Count(cup, IngredientType.Milk);
        int waterCount = Count(cup, IngredientType.Water);
        int cocoaCount = Count(cup, IngredientType.Cocoa);


        //GameLogger.DebugLog("Recipe", "Checking Recipe: " + recipe.recipeType +
        //    "\nCup = " +
        //    "Coffee:" + coffeeCount +
        //    " Milk:" + milkCount +
        //    " Water:" + waterCount +
        //    " Cocoa:" + cocoaCount +
        //    "\nRecipe = " +
        //    "Coffee:" + recipe.coffee +
        //    " Milk:" + recipe.milk +
        //    " Water:" + recipe.water +
        //    " Cocoa:" + recipe.cocoa
        //);

        return coffeeCount == recipe.coffee
            && milkCount == recipe.milk
            && waterCount == recipe.water
            && cocoaCount == recipe.cocoa;
    }


    private int Count( Cup cup, IngredientType type)
    {
        int count = 0;
        foreach (IngredientType ingredient in cup.GetIngredients())
        {
            if (ingredient == type)
            {
                count++;
            }
        }
        return count;
    }
}