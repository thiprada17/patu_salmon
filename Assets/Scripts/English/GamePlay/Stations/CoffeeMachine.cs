using UnityEngine;

public class CoffeeMachine : MonoBehaviour
{
    public void MakeCoffee(Portfilter portfilter)
    {
        if (!portfilter.hasCoffeeGround)
        {
            Debug.Log("Need coffee ground");
            return;
        }

        Debug.Log("Making coffee");
    }
}