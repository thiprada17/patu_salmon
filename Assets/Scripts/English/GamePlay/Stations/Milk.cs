using UnityEngine;

public class Milk : MonoBehaviour
{
    public void AddMilk(Cup cup)
    {
        cup.AddIngredient(IngredientType.Milk);

        Debug.Log("Milk added");
    }
}