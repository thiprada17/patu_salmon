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

        if (orderBubble != null)
        {
            orderBubble.HideDialogue();
        }
        else
        {
            Debug.LogError(
                name + " cannot find CustomerOrderBubble!"
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
                Random.Range(
                    0,
                    dialogueData.dialogues.Length
                )
            ];

        string itemName = GetItemName(order);

        string highlightedItem =
            "<b><color=#F87400>" +
            itemName +
            "</color></b>";

        dialogue = dialogue.Replace(
            "{ITEM_NAME}",
            highlightedItem
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

    public void ReceiveCup(ServingCup cup)
    {
        if (cup == null)
            return;

        Cup cupComponent =
            cup.GetComponent<Cup>();

        if (cupComponent == null)
        {
            Debug.LogWarning(
                name + " received object without Cup component!"
            );
            return;
        }

        // ห้ามรับแก้วเปล่า
        if (!cupComponent.IsFinished())
        {
            Debug.Log(
                name + " cannot receive an unfinished cup!"
            );
            return;
        }

        Debug.Log(
            name +
            " received " +
            cup.RecipeType +
            " | Expected = " +
            order
        );

        if (cup.RecipeType == order)
        {
            CorrectOrder(cup);
        }
        else
        {
            WrongOrder(cup);
        }
    }

    private void ServeCorrectOrder(ServingCup cup)
    {
        Debug.Log(
            name + " received CORRECT order!"
        );

        HideDialogue();

        CustomerMovement movement =
            GetComponent<CustomerMovement>();

        if (movement != null)
        {
            movement.LeaveHappy();
        }

        Destroy(cup.gameObject);
    }

    private void ServeWrongOrder(ServingCup cup)
    {
        Debug.Log(
            name +
            " received WRONG order! " +
            "Expected = " +
            order +
            ", Received = " +
            cup.RecipeType
        );

        // ตอนนี้ยังไม่ทำอะไรกับ Customer
        // ภายหลังสามารถทำให้ Angry ได้
    }

    public void HideDialogue()
    {
        if (orderBubble != null)
        {
            orderBubble.HideDialogue();
        }
    }

    private void CorrectOrder(ServingCup cup)
    {
        Debug.Log(name + " received CORRECT order!");

        // ซ่อนกล่องข้อความ
        HideDialogue();

        // ให้ลูกค้าเดินออกแบบ Happy
        CustomerMovement movement =
            GetComponent<CustomerMovement>();

        if (movement != null)
        {
            movement.LeaveHappy();
        }

        // ทำลายแก้วที่เสิร์ฟแล้ว
        Destroy(cup.gameObject);
    }

    private void WrongOrder(ServingCup cup)
    {
        Debug.Log(
            name +
            " received WRONG order! Expected = " +
            order +
            " | Received = " +
            cup.RecipeType
        );

        // ตอนนี้ยังไม่ทำอะไรกับลูกค้า
        // แก้วยังอยู่ เพื่อให้เอาไปเสิร์ฟใหม่ได้
    }

    public void ShowTimeBar()
    {
        if (orderBubble != null)
        {
            orderBubble.ShowTimeBar();
        }
    }

    public void UpdateTimeBar(
        float currentTime,
        float maxTime)
    {
        if (orderBubble != null)
        {
            orderBubble.UpdateTimeBar(
                currentTime,
                maxTime
            );
        }
    }

    public void SetTimeBarAngry()
    {
        if (orderBubble != null)
        {
            orderBubble.SetTimeBarAngry();
        }
    }

    public void HideTimeBar()
    {
        if (orderBubble != null)
        {
            orderBubble.HideTimeBar();
        }
    }
}