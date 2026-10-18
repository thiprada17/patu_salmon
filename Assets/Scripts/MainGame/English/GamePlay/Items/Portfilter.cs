using UnityEngine;

public class Portfilter : MonoBehaviour
{

    [Header("Has Coffee Ground")]
    public bool hasCoffeeGround = false;
    [SerializeField] private GameObject coffeeGroundStatus;

    public void Awake()
    {
        HideGroundedCoffeeStatus();
    }

    public void HideGroundedCoffeeStatus()
    {
        if (coffeeGroundStatus != null)
            coffeeGroundStatus.SetActive(false);
    }

    public void showGroundedCoffeeStatus()
    {
        if (coffeeGroundStatus != null)
        {
            coffeeGroundStatus.SetActive(true);
            Debug.Log("Status activated: " + coffeeGroundStatus);
        }
        else
        {
            Debug.LogWarning("Status is not Assigned!");
        }
    
    }

    public void AddCoffeeGround()
    {
        if (!hasCoffeeGround)
        {
            hasCoffeeGround = true;
            showGroundedCoffeeStatus();
            GameLogger.DebugLog("Portfilter", "Added coffee ground");
        }
    }

    public void RemoveCoffeeGround()
    {
        if (hasCoffeeGround)
        {
            hasCoffeeGround = false;
            HideGroundedCoffeeStatus();
            GameLogger.DebugLog("Portfilter", "Coffee ground removed from Portfilter");
        }
    }
}