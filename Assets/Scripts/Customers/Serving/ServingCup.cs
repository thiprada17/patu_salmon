using UnityEngine;

public class ServingCup : MonoBehaviour
{
    [Header("Recipe")]
    [SerializeField]
    private RecipeType recipeType = RecipeType.None;

    public RecipeType RecipeType => recipeType;

    public void SetRecipe(RecipeType recipe)
    {
        recipeType = recipe;

        Debug.Log(
            gameObject.name +
            " ServingCup recipe updated to: " +
            recipeType
        );
    }
}