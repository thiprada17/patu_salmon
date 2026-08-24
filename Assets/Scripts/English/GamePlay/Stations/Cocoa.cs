using UnityEngine;

public class Cocoa : MonoBehaviour
{
    public void AddCocoa(Cup cup)
    {
        cup.AddIngredient(IngredientType.Cocoa);

        Debug.Log("Cocoa added");
    }
}