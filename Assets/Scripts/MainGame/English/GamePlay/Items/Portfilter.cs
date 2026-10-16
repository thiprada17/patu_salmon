using UnityEngine;

public class Portfilter : MonoBehaviour
{
    public bool hasCoffeeGround = false;

    public void AddCoffeeGround()
    {
        if (!hasCoffeeGround)
        {
            hasCoffeeGround = true;
            GameLogger.DebugLog("Portfilter", "Added coffee ground");
        }
    }

    public void RemoveCoffeeGround()
    {
        if (hasCoffeeGround)
        {
            hasCoffeeGround = false;
            GameLogger.DebugLog("Portfilter", "Coffee ground removed from Portfilter");
        }
    }
}