using UnityEngine;
using System.Collections;

public class CustomerSpawner : MonoBehaviour
{
    [Header("Customer")]
    [SerializeField] private CustomerMovement customerPrefab;

    [Header("Spawn")]
    [SerializeField] private Transform spawnPoint;

    [Header("Queue")]
    [SerializeField] private CustomerQueue customerQueue;

    [Header("Spawn Settings")]
    [SerializeField] private int customerCount = 5;
    [SerializeField] private float spawnInterval = 2f;

    private int spawnedCount = 0;


    private void Start()
    {
        StartCoroutine(SpawnCustomers());
    }


    private IEnumerator SpawnCustomers()
    {
        while (spawnedCount < customerCount)
        {
            if (customerQueue.IsFull())
            {
                yield return null;
                continue;
            }

            SpawnCustomer();

            spawnedCount++;

            yield return new WaitForSeconds(spawnInterval);
        }
    }


    private void SpawnCustomer()
    {
        CustomerMovement customer =
            Instantiate(
                customerPrefab,
                spawnPoint.position,
                Quaternion.identity
            );


        bool joinedQueue =
            customerQueue.TryAddCustomer(customer);


        if (joinedQueue)
        {
            Debug.Log(
                customer.name +
                " spawned and joined queue."
            );
        }
        else
        {
            Debug.LogWarning(
                customer.name +
                " spawned but could not join queue."
            );
        }
    }
}