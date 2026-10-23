using System.Collections;
using UnityEngine;

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
    [SerializeField, Min(0f)] private float minSpawnTime = 2f;
    [SerializeField, Min(0f)] private float maxSpawnTime = 5f;

    //Customer Spawn During tutorial Session
    [Header("Tutorial Settings")]
    [SerializeField, Min(1)] private int tutorialCustomerCount = 3;

    private int spawnedCount;

    private void Start()
    {
        if (customerPrefab == null ||
            spawnPoint == null ||
            customerQueue == null)
        {
            Debug.LogError(
                "[CustomerSpawner] Assign Customer Prefab, " +
                "Spawn Point, and Customer Queue in the Inspector."
            );
            enabled = false;
            return;
        }

        spawnedCount = 0;
        StartCoroutine(SpawnCustomers());
    }

    private IEnumerator SpawnCustomers()
    {
        int targetCount = customerCount;

        if (TutorialSession.IsInitialized &&
            TutorialSession.IsFirstTimePlayer)
        {
            targetCount = tutorialCustomerCount;
        }

        Debug.Log(
            $"[CustomerSpawner] Target customer count: {targetCount}"
        );

        while (spawnedCount < targetCount)
        {
            if (customerQueue.IsFull())
            {
                yield return null;
                continue;
            }

            SpawnCustomer();
            spawnedCount++;

            if (spawnedCount >= targetCount)
                break;

            float randomSpawnTime = Random.Range(
                minSpawnTime,
                Mathf.Max(minSpawnTime, maxSpawnTime)
            );

            Debug.Log(
                $"[CustomerSpawner] Next customer in " +
                $"{randomSpawnTime:F1} seconds."
            );

            yield return new WaitForSeconds(randomSpawnTime);
        }
    }

    private void SpawnCustomer()
    {
        CustomerMovement customer = Instantiate(
            customerPrefab,
            spawnPoint.position,
            Quaternion.identity
        );

        bool joinedQueue = customerQueue.TryAddCustomer(customer);

        if (joinedQueue)
        {
            Debug.Log(
                customer.name + " spawned and joined queue."
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