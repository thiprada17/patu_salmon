using UnityEngine;

public class Portfilter : MonoBehaviour
{
    public bool  hasCoffeeGround = false;


    public void AddCoffeeGround()
    {
        if (!hasCoffeeGround) {
        hasCoffeeGround = true;

            Debug.Log("add coffee ground");
                
        }
    }
}
