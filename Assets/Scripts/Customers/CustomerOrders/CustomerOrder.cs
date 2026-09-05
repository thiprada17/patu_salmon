using UnityEngine;

public class CustomerOrder : MonoBehaviour
{
    [Header("Order")]
    [SerializeField] private RecipeType order = RecipeType.None;

    [Header("Dialogue")]
    [SerializeField] private CustomerDialogueData dialogueData;

    [Header("Bubble")]
    [SerializeField] private CustomerOrderBubble orderBubble;

    public RecipeType Order => order;

    private void Awake()
    {
        if (orderBubble == null)
        {
            orderBubble =
                GetComponentInChildren<CustomerOrderBubble>(true);
        }

        if (orderBubble == null)
        {
            Debug.LogError(
                name + " cannot find CustomerOrderBubble!"
            );
        }
        else
        {
            Debug.Log(
                name + " found CustomerOrderBubble!"
            );
        }
    }

    public void GenerateOrder()
    {
        int randomIndex = Random.Range(1, 6);

        order = (RecipeType)randomIndex;

        ShowDialogue();

        Debug.Log(name + " ordered " + order);
    }

    private void ShowDialogue()
    {
        if (dialogueData == null)
        {
            Debug.LogWarning(
                name + " has no CustomerDialogueData!"
            );
            return;
        }

        if (dialogueData.dialogues == null ||
            dialogueData.dialogues.Length == 0)
        {
            Debug.LogWarning(
                name + " has no dialogue!"
            );
            return;
        }

        string dialogue =
            dialogueData.dialogues[
                Random.Range(0, dialogueData.dialogues.Length)
            ];

        string itemName = GetItemName(order);

        dialogue = dialogue.Replace(
            "{ITEM_NAME}",
            itemName
        );

        if (orderBubble != null)
        {
            orderBubble.ShowDialogue(dialogue);
        }
    }

    private string GetItemName(RecipeType recipe)
    {
        switch (recipe)
        {
            case RecipeType.Latte:
                return "Latte";

            case RecipeType.Americano:
                return "Americano";

            case RecipeType.Mocha:
                return "Mocha";

            case RecipeType.CocoaLatte:
                return "Cocoa Latte";

            case RecipeType.PlainWater:
                return "Plain Water";

            default:
                return "";
        }
    }
}