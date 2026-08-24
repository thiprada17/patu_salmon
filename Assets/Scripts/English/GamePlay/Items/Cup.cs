using System.Collections.Generic;
using UnityEngine;

public class Cup : MonoBehaviour
{
    public List<IngredientType> ingredients = new List<IngredientType>();

    public void AddIngredient(IngredientType ingredient)
    {
        ingredients.Add(ingredient);

        Debug.Log("Added: " + ingredient);
    }
}