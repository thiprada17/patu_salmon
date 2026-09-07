using UnityEngine;

public class CustomerServeZone : MonoBehaviour
{
    private CustomerOrder customerOrder;

    private void Awake()
    {
        customerOrder =
            GetComponentInParent<CustomerOrder>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        ServingCup cup =
            other.GetComponent<ServingCup>();

        if (cup == null)
            return;

        if (customerOrder == null)
        {
            Debug.LogError(
                "CustomerOrder not found!"
            );
            return;
        }

        customerOrder.ReceiveCup(cup);
    }
}