using UnityEngine;
using UnityEngine.U2D.Animation;

public class RandomCustomerVariantsSpawner : MonoBehaviour
{
    [Header("Sprite Resolvers")]
    [SerializeField] private SpriteResolver body;
    [SerializeField] private SpriteResolver head;
    [SerializeField] private SpriteResolver eyesOpen;
    [SerializeField] private SpriteResolver eyesClose;
    [SerializeField] private SpriteResolver eyesAngry;
    [SerializeField] private SpriteResolver mouth;
    [SerializeField] private SpriteResolver armL;
    [SerializeField] private SpriteResolver armR;

    [SerializeField] private SpriteResolver tail;

    [Header("Variant Settings")]
    [SerializeField] private int variantCount = 4;

    private int currentVariant;

    private void Awake()
    {
        RandomizeVariant();
    }

    private void RandomizeVariant()
    {
        currentVariant = Random.Range(1, variantCount + 1);

        SetVariant(body, "Body");
        SetVariant(head, "Head");
        SetVariant(eyesOpen, "Eyes Open");
        SetVariant(eyesClose, "Eyes Close");
        SetVariant(eyesAngry, "Eyes Angry");
        SetVariant(mouth, "Mouth");
        SetVariant(armL, "Arm L");
        SetVariant(armR, "Arm R");

        //Optional Variant
        SetVariant(tail, "Tail");

        Debug.Log(
            gameObject.name +
            " uses Variant " +
            currentVariant
        );
    }


    private void SetVariant(
        SpriteResolver resolver,
        string category)
    {
        if (resolver == null)
        {
            // Tail is optional, so no warning is needed.
            if (category != "Tail")
            {
                Debug.LogWarning(
                    category + " Sprite Resolver is missing!"
                );
            }

            return;
        }

        string label = category + " - " + currentVariant;

        resolver.SetCategoryAndLabel(
            category,
            label
        );
    }

}