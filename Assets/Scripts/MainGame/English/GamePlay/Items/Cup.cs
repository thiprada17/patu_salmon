using UnityEngine;
using System.Collections;
using System;

public class Cup : MonoBehaviour
{
    private const int MaxIngredients = 4;
    private ServingCup servingCup;

    [Header("Colliders")]
    [SerializeField]
    private BoxCollider2D dragCollider;

    [SerializeField]
    private BoxCollider2D attachCollider;

    public BoxCollider2D AttachCollider => attachCollider;

    [SerializeField]
    private IngredientType[] ingredients = new IngredientType[MaxIngredients];

    private bool[] hasIngredient = new bool[MaxIngredients];

    [SerializeField] private GameObject finishVFX;
    [SerializeField] private Transform vfxPoint;

    [SerializeField] private GameObject prefinishVFX;
    [SerializeField] private Transform prevfxPoint;

    private bool isFull = false;
    public bool IsFull => isFull;

    [Header("Empty Cup")] 
    [SerializeField]
    private SpriteRenderer emptyCupRenderer;


    [Header("Finished Cup")] 
    [SerializeField]
    private SpriteRenderer finishedCupRenderer;

    [Header("Cap")] 
    [SerializeField]
    private GameObject capObject;


    [Header("Coffee")] 
    [SerializeField]
    private GameObject[] coffeeObjects = new GameObject[MaxIngredients];

    
    [Header("Milk")] 
    [SerializeField]
    private GameObject[] milkObjects = new GameObject[MaxIngredients];


    [Header("Water")] 
    [SerializeField]
    private GameObject[] waterObjects = new GameObject[MaxIngredients];


    [Header("Cocoa")] 
    [SerializeField]
    private GameObject[] cocoaObjects = new GameObject[MaxIngredients];


    private void Awake()
    {
        DisableAllIngredientObjects();
        servingCup = GetComponent<ServingCup>();

        if (servingCup == null)
        {
            GameLogger.Error("CUP", name + " cannot find ServingCup");
        }
        if (finishedCupRenderer != null)
        {
            finishedCupRenderer.gameObject.SetActive(false);
        }
    }


    private void DisableAllIngredientObjects()
    {
        DisableObjects(coffeeObjects);
        DisableObjects(milkObjects);
        DisableObjects(waterObjects);
        DisableObjects(cocoaObjects);
    }


    private void DisableObjects(GameObject[] objects)
    {
        if (objects == null) return;

        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i] != null)
            {
                objects[i].SetActive(false);
            }
        }
    }


    public bool AddIngredient(IngredientType ingredient)
    {
        if (ingredients == null || ingredients.Length != MaxIngredients)
            ingredients = new IngredientType[MaxIngredients];

        if (hasIngredient == null || hasIngredient.Length != MaxIngredients)
            hasIngredient = new bool[MaxIngredients];

        for (int i = 0; i < MaxIngredients; i++)
        {
            if (!hasIngredient[i])
            {
                ingredients[i] = ingredient;
                hasIngredient[i] = true;

                ShowIngredient(ingredient, i);

                // เช็กว่าใส่ส่วนผสมครบทุกช่องหรือยัง
                isFull = true;

                for (int j = 0; j < MaxIngredients; j++)
                {
                    if (!hasIngredient[j])
                    {
                        isFull = false;
                        break;
                    }
                }

                return true;
            }
        }

        isFull = true;
        return false;
    }


    private void ShowIngredient(
        IngredientType ingredient,
        int index)
    {
        if (index < 0 ||
            index >= MaxIngredients)
        {
            return;
        }


        GameObject[] objects = GetIngredientObjects(ingredient);


        if (objects == null)
        {
            GameLogger.Warn("CUP", "ไม่มี Object สำหรับ " + ingredient );

            return;
        }


        if (index >= objects.Length ||
            objects[index] == null)
        {
            GameLogger.Error("CUP", "ไม่มี " + ingredient + "Index: " + index);
            return;
        }

        objects[index].SetActive(true);
    }


    private GameObject[] GetIngredientObjects(IngredientType ingredient)
    {
        switch (ingredient)
        {
            case IngredientType.Coffee:
                return coffeeObjects;

            case IngredientType.Milk:
                return milkObjects;

            case IngredientType.Water:
                return waterObjects;

            case IngredientType.Cocoa:
                return cocoaObjects;

            default:
                return null;
        }
    }


    public IngredientType[] GetIngredients()
    {
        int count = 0;

        for (int i = 0; i < MaxIngredients; i++)
        {
            if (hasIngredient[i])
            {
                count++;
            }
        }

        IngredientType[] result = new IngredientType[count];

        int index = 0;

        for (int i = 0; i < MaxIngredients; i++)
        {
            if (hasIngredient[i])
            {
                result[index] = ingredients[i];
                index++;
            }
        }

        return result;
    }

    public void DestroyCup()
    {
        Destroy(gameObject);
    }

    public void SetFinishedRecipe(Sprite finishedSprite, RecipeType recipeType)
    {
        if (finishedSprite == null)
        {
            GameLogger.Error("CUP", "Finished Sprite is null!");
            return;
        }

        isFinished = true;

        // อัปเดตข้อมูลเมนูของ Cup
        if (servingCup != null)
        {
            servingCup.SetRecipe(recipeType);
        }

        PlayPreFinishVFX();

        // ปิดส่วนผสมทั้งหมด
        DisableAllIngredientObjects();

        // ปิดแก้วเปล่า
        if (emptyCupRenderer != null)
        {
            emptyCupRenderer.gameObject.SetActive(false);
        }

        // ปิด Cap
        if (capObject != null)
        {
            capObject.SetActive(false);
        }

        // เปลี่ยนเป็นรูปเครื่องดื่มสำเร็จ
        if (finishedCupRenderer != null)
        {
            finishedCupRenderer.sprite = finishedSprite;
            finishedCupRenderer.gameObject.SetActive(true);
        }

        PlayFinishVFX();

        GameLogger.Info("CUP", name + " finished recipe: " + recipeType);
    }


    private void PlayPreFinishVFX()
    {
        if (finishVFX == null || vfxPoint == null)
        {
            GameLogger.Warn("CUP","ยังไม่ได้ใส่ Finish VFX หรือ VFX Point");
            return;
        }

        GameObject vfx = Instantiate(
            prefinishVFX,
            prevfxPoint.position,
            prevfxPoint.rotation
        );

        Destroy(vfx, 20f);
    }
    private void PlayFinishVFX()
    {
        if (finishVFX == null || vfxPoint == null)
        {
            GameLogger.Info("CUP","ยังไม่ได้ใส่ Finish VFX หรือ VFX Point");
            return;
        }

        GameObject vfx = Instantiate(
            finishVFX,
            vfxPoint.position,
            vfxPoint.rotation
        );

        Destroy(vfx, 3f);
    }

    private bool isFinished = false;

    public bool IsFinished()
    {
        return isFinished;
    }
}