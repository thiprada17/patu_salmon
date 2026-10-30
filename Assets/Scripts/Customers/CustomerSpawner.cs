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
    [Tooltip("Kept for compatibility with existing shift progression.")]
    [SerializeField, Min(1)] private int startingCustomerCount = 5;

    [Tooltip("Kept for compatibility with existing shift progression.")]
    [SerializeField, Min(0)] private int extraCustomersPerShift = 1;

    [Header("Spawn Timing")]
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
    private bool tutorialMode;
    private bool isSpawning;
    private bool shiftSpawningStopped;

    private Coroutine spawnCoroutine;

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

        // DayManager should call BeginShift() when the shift starts.
    }

    private IEnumerator SpawnCustomers()
    {
        while (isSpawning)
        {
            // Preserve the tutorial's limited customer count.
            if (tutorialMode && spawnedCount >= tutorialCustomerCount)
            {
                FinishSpawning();
                yield break;
            }

            // Pause tutorial spawning while the tutorial is inactive.
            if (tutorialMode && !TutorialSession.IsTutorialActive)
            {
                yield return null;
                continue;
            }

            // Wait until there is room in the queue.
            if (customerQueue.IsFull())
            {
                yield return null;
                continue;
            }

            int previousCount = spawnedCount;

            SpawnCustomer();

            // If no customer was spawned successfully, avoid
            // repeatedly retrying every frame.
            if (spawnedCount == previousCount)
            {
                yield return new WaitForSeconds(0.5f);
                continue;
            }

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
                "[CustomerSpawner] " + customer.name +
                " spawned. Total this shift: " + spawnedCount
            );
        }
        else
        {
            Debug.LogWarning(
                "[CustomerSpawner] " + customer.name +
                " could not join the queue."
            );

            Destroy(customer.gameObject);
        }
    }

    private CustomerMovement GetRandomCustomerPrefab()
    {
        bool hasDogs = dogsPrefabs != null && dogsPrefabs.Length > 0;

        if (hasDogs && Random.value < dogSpawnChance)
        {
            // Choose a valid dog prefab.
            for (int i = 0; i < dogsPrefabs.Length; i++)
            {
                CustomerMovement dog = dogsPrefabs[
                    Random.Range(0, dogsPrefabs.Length)
                ];

                if (dog != null)
                    return dog;
            }
        }

        return customerPrefab;
    }

    public void BeginShift()
    {
        if (!enabled)
            return;

        // Prevent duplicate spawning coroutines.
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }

        spawnedCount = 0;
        isSpawning = true;
        shiftSpawningStopped = false;

        tutorialMode =
            TutorialSession.IsInitialized &&
            TutorialSession.IsFirstTimePlayer;

        Debug.Log(
            "[CustomerSpawner] Shift started. " +
            "Tutorial: " + tutorialMode
        );

        spawnCoroutine = StartCoroutine(SpawnCustomers());
    }

    private void FinishSpawning()
    {
        isSpawning = false;
        spawnCoroutine = null;

        Debug.Log(
            "[CustomerSpawner] Tutorial spawning finished. " +
            "Customers spawned: " + spawnedCount
        );
    }

    // Call this when the shift timer reaches zero.
    public void StopSpawning()
    {
        if (shiftSpawningStopped)
            return;

        shiftSpawningStopped = true;
        isSpawning = false;

        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }

        Debug.Log(
            "[CustomerSpawner] Shift ended. " +
            "Total customers spawned: " + spawnedCount
        );
    }

    public int GetSpawnedCount()
    {
        return spawnedCount;
    }

    // Retained for compatibility with existing scripts.
    public int GetTargetCount()
    {
        return tutorialMode ? tutorialCustomerCount : -1;
    }

    public bool HasFinishedSpawning()
    {
        // For regular shifts, spawning finishes when the timer
        // calls StopSpawning(). For tutorials, it can finish at
        // the tutorial customer limit.
        return !isSpawning;
    }
}