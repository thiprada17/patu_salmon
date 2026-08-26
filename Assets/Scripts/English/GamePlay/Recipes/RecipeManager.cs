using UnityEngine;

public class RecipeManager : MonoBehaviour
{
    public bool IsLatte(Cup cup)
    {
        if (cup == null)
        {
            Debug.LogError("Cup is null!");
            return false;
        }

        int coffee = 0;
        int milk = 0;

        foreach (IngredientType ingredient in cup.GetIngredients())
        {
            if (ingredient == IngredientType.Coffee)
            {
                coffee++;
            }
            else if (ingredient == IngredientType.Milk)
            {
                milk++;
            }
        }

        return coffee == 1 && milk == 3;
    }
}