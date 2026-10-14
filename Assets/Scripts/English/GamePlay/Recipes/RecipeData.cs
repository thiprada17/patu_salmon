
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

    [Header("Ingredients")]
    public int coffee;
    public int milk;
    public int water;
    public int cocoa;
}
