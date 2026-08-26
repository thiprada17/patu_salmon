using UnityEngine;

public class Portfilter : MonoBehaviour
{
    public bool hasCoffeeGround = false;

    public void AddCoffeeGround()
    {
        if (!hasCoffeeGround)
        {
            hasCoffeeGround = true;

            Debug.Log("Added coffee ground");
        }
    }

    public void RemoveCoffeeGround()
    {
        if (hasCoffeeGround)
        {
            hasCoffeeGround = false;

            Debug.Log("Coffee ground removed from Portfilter");
        }
    }
}