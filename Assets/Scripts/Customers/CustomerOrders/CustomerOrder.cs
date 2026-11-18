using UnityEngine;

public class CustomerOrder : MonoBehaviour
{
    private static readonly RecipeType[] EarlyOrders =
    {
        RecipeType.PlainWater,
        RecipeType.CocoaLatte
    };

    private static readonly RecipeType[] IntermediateOrders =
    {
        RecipeType.PlainWater,
        RecipeType.CocoaLatte,
        RecipeType.Latte,
        RecipeType.Americano
    };

    private static readonly RecipeType[] AllOrders =
    {
        RecipeType.PlainWater,
        RecipeType.CocoaLatte,
        RecipeType.Latte,
        RecipeType.Americano,
        RecipeType.Mocha
    };

    [Header("Order")]
    [SerializeField] private RecipeType order = RecipeType.None;

    [Header("Bubble")]
    [SerializeField] private CustomerOrderBubble orderBubble;

    public RecipeType Order => order;

    private bool orderAlreadyServed;
    private CustomerMovement movement;

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

        movement = GetComponent<CustomerMovement>();

        if (movement == null)
        {
            Debug.LogError(
                name + " cannot find CustomerMovement!"
            );
        }
    }

    public void GenerateOrder()
    {
        int completedDays = CoinManager.Instance != null
            ? CoinManager.Instance.WorkDays
            : 0;
        int currentDay = completedDays + 1;

        RecipeType[] availableOrders = currentDay <= 2
            ? EarlyOrders
            : currentDay <= 5
                ? IntermediateOrders
                : AllOrders;

        order = availableOrders[Random.Range(0, availableOrders.Length)];
        orderAlreadyServed = false;

        ShowDialogue();

        Debug.Log(name + " ordered " + order);
    }

    private void ShowDialogue()
    {
        Sprite orderSprite = CoinManager.Instance != null
            ? CoinManager.Instance.GetFinishedCupSprite(order)
            : null;
        string itemName = GetItemName(order);

        if (orderBubble != null)
        {
            orderBubble.ShowOrder(orderSprite, itemName);
        }

        if (orderSprite == null)
        {
            Debug.LogWarning(
                name + " cannot find the finished cup sprite for " + order
            );
        }
    }

    private string GetItemName(RecipeType recipe)
    {
        switch (recipe)
        {
            case RecipeType.Latte: return "Latte";
            case RecipeType.Americano: return "Americano";
            case RecipeType.Mocha: return "Mocha";
            case RecipeType.CocoaLatte: return "Cocoa Latte";
            case RecipeType.PlainWater: return "Plain Water";
            default: return "";
        }
    }

    public void ReceiveCup(ServingCup cup)
    {
        if (cup == null || orderAlreadyServed)
            return;

        Cup cupComponent = cup.GetComponent<Cup>();

        if (cupComponent == null)
        {
            GameLogger.Warn(
                "ReceiveCup",
                name + " received object without Cup component!"
            );
            return;
        }

        if (!cupComponent.IsFinished())
        {
            GameLogger.Info(
                "ReceiveCup",
                name + " cannot receive an unfinished cup!"
            );
            return;
        }

        GameLogger.Info(
            "ReceiveCup",
            name + " received " + cup.RecipeType +
            " | Expected = " + order
        );

        if (cup.RecipeType == order)
        {
            CorrectOrder(cup);
        }
        else
        {
            WrongOrder(cup);

            DraggableItem draggable =
                cup.GetComponent<DraggableItem>();

            if (draggable != null)
            {
                draggable.RejectDrop();
            }
        }
    }

    private void ServeCorrectOrder(ServingCup cup)
    {
        CorrectOrder(cup);
    }

    private void ServeWrongOrder(ServingCup cup)
    {
        WrongOrder(cup);
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
        if (orderAlreadyServed)
            return;

        if (movement == null)
        {
            Debug.LogError(
                name + " cannot complete order: CustomerMovement is missing!"
            );
            return;
        }

        orderAlreadyServed = true;
        HideDialogue();

        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.EarnCoins(
                order,
                movement.GetWaitTimer(),
                movement.GetMaxWaitTime(),
                movement.IsAngryState()
            );
        }

        TutorialSession.RegisterTutorialCustomerServed();

        bool isFinalTutorialCustomer =
            TutorialSession.IsFinalTutorialCustomer;

        if (cup != null)
        {
            Destroy(cup.gameObject);
        }

        if (isFinalTutorialCustomer)
        {
            movement.LeaveHappy(true);
            return;
        }

        movement.LeaveHappy();
    }

    private void WrongOrder(ServingCup cup)
    {
        if (cup == null)
            return;

        Debug.Log(
            name +
            " received WRONG order! Expected = " +
            order +
            " | Received = " +
            cup.RecipeType
        );
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
