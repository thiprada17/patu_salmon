using UnityEngine;

public class Cup : MonoBehaviour
{

    private const int MaxIngredients = 4;

    [SerializeField]
    private IngredientType[] ingredients =
        new IngredientType[MaxIngredients];

    // เช็กว่าแต่ละช่องมีของหรือยัง
    private bool[] hasIngredient =
        new bool[MaxIngredients];

    [SerializeField]
    private Transform ingredientStack;

    [SerializeField]
    private Sprite coffeeSprite;

    [SerializeField]
    private Sprite milkSprite;

    [SerializeField]
    private Sprite waterSprite;

    [SerializeField]
    private Sprite cocoaSprite;


    public bool AddIngredient(IngredientType ingredient)
    {
        // ป้องกัน Array ผิดขนาด
        if (ingredients == null ||
            ingredients.Length != MaxIngredients)
        {
            ingredients =
                new IngredientType[MaxIngredients];
        }

        if (hasIngredient == null ||
            hasIngredient.Length != MaxIngredients)
        {
            hasIngredient =
                new bool[MaxIngredients];
        }

        // หา Index แรกที่ว่าง
        for (int i = 0; i < MaxIngredients; i++)
        {
            if (!hasIngredient[i])
            {
                // เก็บข้อมูล
                ingredients[i] = ingredient;
                hasIngredient[i] = true;

                // สร้าง Sprite
                CreateIngredientSprite(
                    ingredient,
                    i
                );

                Debug.Log(
                    "Added " +
                    ingredient +
                    " to Cup Index " +
                    i
                );

                return true;
            }
        }

        Debug.Log("Cup is full!");

        return false;
    }


    private void CreateIngredientSprite(
        IngredientType ingredient,
        int index)
    {
        if (index < 0 ||
            index >= MaxIngredients)
        {
            Debug.LogError(
                "Invalid Cup Index: " + index
            );

            return;
        }

        if (ingredientStack == null)
        {
            Debug.LogError(
                "Cup: Ingredient Stack ยังไม่ได้ใส่!"
            );

            return;
        }

        Sprite sprite =
            GetIngredientSprite(ingredient);

        if (sprite == null)
        {
            Debug.LogError(
                "ไม่มี Sprite สำหรับ " +
                ingredient
            );

            return;
        }

        GameObject ingredientObject =
            new GameObject(
                ingredient +
                "_Index_" +
                index
            );

        ingredientObject.transform.SetParent(
            ingredientStack
        );

        SpriteRenderer spriteRenderer =
            ingredientObject.AddComponent<
                SpriteRenderer
            >();

        spriteRenderer.sprite = sprite;

        ingredientObject.transform.localPosition =
            new Vector3(
                0f,
                index * 0.05f,
                0f
            );


        spriteRenderer.sortingOrder = index;
    }


    private Sprite GetIngredientSprite(
        IngredientType ingredient)
    {
        switch (ingredient)
        {
            case IngredientType.Coffee:
                return coffeeSprite;

            case IngredientType.Milk:
                return milkSprite;

            case IngredientType.Water:
                return waterSprite;

            case IngredientType.Cocoa:
                return cocoaSprite;

            default:
                return null;
        }
    }


    public IngredientType[] GetIngredients()
    {
        return ingredients;
    }
}