using UnityEngine;

public class TestCustomerMovement : MonoBehaviour
{
    [SerializeField] private CustomerQueue customerQueue;
    [SerializeField] private CustomerMovement customer;

    private void Start()
    {
        customerQueue.TryAddCustomer(customer);
    }
}