using UnityEngine;
using System.Collections.Generic;

public class CustomerQueue : MonoBehaviour
{
    [Header("Queue Slots")]
    [SerializeField] private Transform q1;
    [SerializeField] private Transform q2;

    private List<CustomerMovement> customers =
        new List<CustomerMovement>();


    public bool TryAddCustomer(CustomerMovement customer)
    {
        if (customer == null)
            return false;

        if (customers.Contains(customer))
            return false;

        if (customers.Count >= 2)
        {
            Debug.Log(
                "Queue is full. " +
                customer.name +
                " must wait."
            );

            return false;
        }

        customers.Add(customer);

        UpdateQueuePositions();

        Debug.Log(
            customer.name +
            " joined queue. Position = " +
            customers.Count
        );

        return true;
    }


    public void RemoveCustomer(CustomerMovement customer)
    {
        if (customer == null)
            return;

        if (!customers.Contains(customer))
            return;

        customers.Remove(customer);

        Debug.Log(
            customer.name +
            " left queue."
        );

        UpdateQueuePositions();
    }


    private void UpdateQueuePositions()
    {
        if (customers.Count >= 1)
        {
            customers[0].MoveToQueueSlot(q1);
        }

        if (customers.Count >= 2)
        {
            customers[1].MoveToQueueSlot(q2);
        }
    }


    public int GetQueueCount()
    {
        return customers.Count;
    }


    public bool IsFull()
    {
        return customers.Count >= 2;
    }
}