using UnityEngine;

public class Cup : MonoBehaviour
{
    private const int MaxIngredients = 4;


    [SerializeField]
    private IngredientType[] ingredients =
        new IngredientType[MaxIngredients];

    private bool[] hasIngredient =
        new bool[MaxIngredients];

    [SerializeField] private GameObject finishVFX;
    [SerializeField] private Transform vfxPoint;

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
        if (objects == null)
            return;

        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i] != null)
            {
                objects[i].SetActive(false);
            }
        }
    }


    public bool AddIngredient(
        IngredientType ingredient)
    {
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


        for (int i = 0; i < MaxIngredients; i++)
        {
            if (!hasIngredient[i])
            {
                ingredients[i] = ingredient;
                hasIngredient[i] = true;

                ShowIngredient(
                    ingredient,
                    i
                );

                Debug.Log(
                    "Added " + ingredient + " to Cup Index " + i
                );

                return true;
            }
        }


        Debug.Log("Cup is full!");

        return false;
    }


    private void ShowIngredient(
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


        GameObject[] objects =
            GetIngredientObjects(ingredient);


        if (objects == null)
        {
            Debug.LogError(
                "ไม่มี Object สำหรับ " + ingredient
            );

            return;
        }


        if (index >= objects.Length ||
            objects[index] == null)
        {
            Debug.LogError(
                "ไม่มี " + ingredient + "_Index_" + index
            );

            return;
        }


        objects[index].SetActive(true);


        Debug.Log(
            "เปิด " + ingredient + "_Index_" + index
        );
    }


    private GameObject[] GetIngredientObjects(
        IngredientType ingredient)
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


        IngredientType[] result =
            new IngredientType[count];


        int index = 0;

        for (int i = 0; i < MaxIngredients; i++)
        {
            if (hasIngredient[i])
            {
                result[index] =
                    ingredients[i];

                index++;
            }
        }


        return result;
    }

    public void DestroyCup()
    {
        Destroy(gameObject);
    }

    public void SetFinishedRecipe(Sprite finishedSprite)
    {
        if (finishedSprite == null)
        {
            Debug.LogError("Finished Sprite is null!");
            return;
        }

        isFinished = true;

        // ปิดส่วนผสมทั้งหมด 
        DisableAllIngredientObjects();

        // ปิดรูปแก้วเปล่า ไม่ให้ซ้อนกับแก้วสำเร็จ
        if (emptyCupRenderer != null)
        {
            emptyCupRenderer.gameObject.SetActive(false);
        }

        if (capObject != null)
        {
            capObject.SetActive(false);
        }

        // เปลี่ยนเป็นรูปเมนูสำเร็จ 
        if (finishedCupRenderer != null)
        {
            finishedCupRenderer.sprite = finishedSprite;
            finishedCupRenderer.gameObject.SetActive(true);
        }

        PlayFinishVFX();
    }

    private void PlayFinishVFX()
    {
        if (finishVFX == null || vfxPoint == null)
        {
            Debug.LogWarning("ยังไม่ได้ใส่ Finish VFX หรือ VFX Point");
            return;
        }

        GameObject vfx = Instantiate(
            finishVFX,
            vfxPoint.position,
            vfxPoint.rotation
        );

        Destroy(vfx, 0.4f);
    }

    private bool isFinished = false;

    public bool IsFinished()
    {
        return isFinished;
    }
}