
using UnityEngine;

[CreateAssetMenu(
    fileName = "Recipe",
    menuName = "Coffee Game/Recipe"
)]
public class RecipeData : ScriptableObject
{
    public RecipeType recipeType;

    [Header("Price")]
    [Min(0)] public int price = 50;

    [Header("Finished Cup")]
    public Sprite finishedCupSprite;

    [Header("Ingredients Count")]
    public int coffee;
    public int milk;
    public int water;
    public int cocoa;

    [Header("Required Ingredient Order")]
    public IngredientType[] ingredientOrder = new IngredientType[4];
}
