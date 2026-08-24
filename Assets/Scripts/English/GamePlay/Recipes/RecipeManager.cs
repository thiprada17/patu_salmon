using UnityEngine;

public class RecipeManager : MonoBehaviour
{
    public bool IsLatte(Cup cup)
    {
        int coffee = 0;
        int milk = 0;

        foreach (IngredientType ingredient in cup.ingredients)
        {
            if (ingredient == IngredientType.Coffee)
            {
                coffee++;
            }

            if (ingredient == IngredientType.Milk)
            {
                milk++;
            }
        }

        return coffee == 1 && milk == 3;
    }
}