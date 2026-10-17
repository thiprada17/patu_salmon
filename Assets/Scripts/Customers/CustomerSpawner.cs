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

    [SerializeField] private float minSpawnTime = 2f;
    [SerializeField] private float maxSpawnTime = 5f;

    private int spawnedCount = 0;


    private void Start()
    {
        StartCoroutine(SpawnCustomers());
    }


    private IEnumerator SpawnCustomers()
    {
        while (spawnedCount < customerCount)
        {
            // ถ้าคิวเต็ม ให้รอจนกว่าจะมีช่อง
            if (customerQueue.IsFull())
            {
                yield return null;
                continue;
            }

            SpawnCustomer();

            spawnedCount++;

            // สุ่มเวลาสำหรับ Customer คนต่อไป
            float randomSpawnTime =
                Random.Range(minSpawnTime, maxSpawnTime);

            Debug.Log(
                "Next customer will spawn in " +
                randomSpawnTime.ToString("F1") +
                " seconds."
            );

            yield return new WaitForSeconds(randomSpawnTime);
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