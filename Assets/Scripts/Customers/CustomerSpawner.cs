
using System.Collections;
using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    [Header("Customer Prefabs")]
    [SerializeField] private CustomerMovement customerPrefab;
    [SerializeField] private CustomerMovement[] dogsPrefabs;

    [Header("Spawn")]
    [SerializeField] private Transform spawnPoint;

    [Header("Queue")]
    [SerializeField] private CustomerQueue customerQueue;

    [Header("Difficulty Progression")]
    [SerializeField, Min(1)] private int startingCustomerCount = 5;
    [SerializeField, Min(0)] private int extraCustomersPerShift = 1;

    [Tooltip("Minimum delay between customer spawns.")]
    [SerializeField, Min(0.1f)] private float startingMinSpawnTime = 2f;

    [Tooltip("Maximum delay between customer spawns.")]
    [SerializeField, Min(0.1f)] private float startingMaxSpawnTime = 5f;

    [Tooltip("Spawn delay reduction per completed shift.")]
    [SerializeField, Min(0f)] private float spawnTimeReductionPerShift = 0.1f;

    [Tooltip("Smallest allowed spawn delay.")]
    [SerializeField, Min(0.1f)] private float minimumSpawnDelay = 0.75f;

    [Header("Tutorial Settings")]
    [SerializeField, Min(1)] private int tutorialCustomerCount = 3;

    [Header("Dog Spawn Settings")]
    [SerializeField, Range(0f, 1f)]
    private float dogSpawnChance = 0.25f;

    private int spawnedCount;
    private int targetCount;
    private bool tutorialMode;

    private Coroutine spawnCoroutine;
    private bool isSpawning;

    private void Start()
    {
        if (customerPrefab == null ||
            spawnPoint == null ||
            customerQueue == null)
        {
            Debug.LogError(
                "[CustomerSpawner] Assign Customer Prefab, " +
                "Spawn Point, and Customer Queue."
            );

            enabled = false;
            return;
        }

        // DayManager starts each shift.
    }

    private IEnumerator SpawnCustomers()
    {
        while (isSpawning)
        {
            if (tutorialMode && !TutorialSession.IsTutorialActive)
            {
                yield return null;
                continue;
            }

            if (customerQueue.IsFull())
            {
                yield return null;
                continue;
            }

            SpawnCustomer();

            if (tutorialMode && spawnedCount >= tutorialCustomerCount)
                yield break;

            yield return new WaitForSeconds(GetSpawnDelay());
        }

        spawnCoroutine = null;
    }

    private float GetSpawnDelay()
    {
        if (tutorialMode)
        {
            return Random.Range(
                startingMinSpawnTime,
                Mathf.Max(startingMinSpawnTime, startingMaxSpawnTime)
            );
        }

        int completedShifts = CoinManager.Instance != null
            ? Mathf.Max(0, CoinManager.Instance.WorkDays)
            : 0;

        float reduction =
            completedShifts * spawnTimeReductionPerShift;

        float minDelay = Mathf.Max(
            minimumSpawnDelay,
            startingMinSpawnTime - reduction
        );

        float maxDelay = Mathf.Max(
            minDelay,
            startingMaxSpawnTime - reduction
        );

        return Random.Range(minDelay, maxDelay);
    }

    private void SpawnCustomer()
    {
        CustomerMovement prefabToSpawn = GetRandomCustomerPrefab();

        if (prefabToSpawn == null)
        {
            Debug.LogWarning(
                "[CustomerSpawner] No valid customer prefab available."
            );
            return;
        }

        CustomerMovement customer = Instantiate(
            prefabToSpawn,
            spawnPoint.position,
            Quaternion.identity
        );

        bool joinedQueue = customerQueue.TryAddCustomer(customer);

        if (joinedQueue)
        {
            spawnedCount++;

            Debug.Log(
                customer.name + " spawned and joined queue."
            );
        }
        else
        {
            Debug.LogWarning(
                customer.name + " could not join the queue."
            );

            Destroy(customer.gameObject);
        }
    }


    private CustomerMovement GetRandomCustomerPrefab()
    {
        bool hasDogs = dogsPrefabs != null && dogsPrefabs.Length > 0;

        if (hasDogs && Random.value < 0.5f)
        {
            // 50% chance to spawn a dog.
            for (int i = 0; i < dogsPrefabs.Length; i++)
            {
                CustomerMovement dog = dogsPrefabs[
                    Random.Range(0, dogsPrefabs.Length)
                ];

                if (dog != null)
                    return dog;
            }
        }

        // 50% chance to spawn a cat.
        return customerPrefab;
    }


    public void BeginShift()
    {
        if (!enabled)
            return;

        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }

        spawnedCount = 0;
        isSpawning = true;

        tutorialMode =
            TutorialSession.IsInitialized &&
            TutorialSession.IsFirstTimePlayer;

        if (tutorialMode)
        {
            targetCount = tutorialCustomerCount;
        }
        else
        {
            int completedShifts = CoinManager.Instance != null
                ? Mathf.Max(0, CoinManager.Instance.WorkDays)
                : 0;

            targetCount = startingCustomerCount +
                completedShifts * extraCustomersPerShift;
        }

        spawnCoroutine = StartCoroutine(SpawnCustomers());
    }

    public void StopSpawning()
    {
        isSpawning = false;

        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }

        Debug.Log(
            "[CustomerSpawner] Spawning stopped for this shift."
        );
    }
}
