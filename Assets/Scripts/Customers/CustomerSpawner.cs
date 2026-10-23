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

        // DayManager will start each shift's spawn cycle.
    }

    private IEnumerator SpawnCustomers()
    {
        while (true)
        {
            // Stop normal spawning while the tutorial has not started.
            if (tutorialMode && !TutorialSession.IsTutorialActive)
            {
                yield return null;
                continue;
            }

            // Stop spawning once the shift timer reaches zero.
            // DayManager should call StopSpawning() at shift end.
            if (!isSpawning)
                yield break;

            // Wait if both queue slots are occupied.
            if (customerQueue.IsFull())
            {
                yield return null;
                continue;
            }

            SpawnCustomer();
            spawnedCount++;

            // Tutorial only needs its configured number of customers.
            if (tutorialMode && spawnedCount >= tutorialCustomerCount)
                yield break;

            float delay = GetSpawnDelay();

            yield return new WaitForSeconds(delay);
        }
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

        float reduction = completedShifts * spawnTimeReductionPerShift;

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

            Destroy(customer.gameObject);
            spawnedCount--;
        }
    }

    public void BeginShift()
    {
        if (!enabled)
            return;

        if (spawnCoroutine != null)
            StopCoroutine(spawnCoroutine);

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

        Debug.Log("[CustomerSpawner] Spawning stopped for this shift.");
    }
}