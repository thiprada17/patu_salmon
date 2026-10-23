using UnityEngine;
using System.Collections;

public class CustomerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    [Header("Walking Bob")]
    [SerializeField] private float bobHeight = 0.06f;
    [SerializeField] private float bobSpeed = 8f;

    [Header("Waiting")]
    [SerializeField] private float maxWaitTime = 30f;
    [SerializeField] private float angryTime = 20f;

    [Header("Exit")]
    [SerializeField] private Transform exitPoint;

    private float targetX;
    private bool isMoving;

    private float startY;
    private float bobTimer;

    private Transform currentQueueSlot;

    private float waitTimer;
    private bool isWaiting;
    private bool isAngry;
    private bool isLeaving;

    private CustomerQueue customerQueue;
    private CustomerOrder customerOrder;

    private bool hasGeneratedOrder = false;
    private bool hasStartedWaiting = false;

    // True only for the third tutorial customer.
    private bool isFinalTutorialCustomer;


    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        startY = transform.position.y;

        customerQueue = FindFirstObjectByType<CustomerQueue>();
        customerOrder = GetComponent<CustomerOrder>();
    }


    public void MoveToQueueSlot(Transform queueSlot)
    {
        if (isLeaving)
            return;

        if (queueSlot == null)
        {
            Debug.LogError(
                name + " received NULL queue slot!"
            );
            return;
        }

        currentQueueSlot = queueSlot;
        targetX = queueSlot.position.x;

        isMoving = true;
        isWaiting = false;

        // Do not reset waitTimer here.
        bobTimer = 0f;

        if (animator != null)
        {
            animator.SetBool("IsWalking", true);
        }

        Debug.Log(name + " moving to " + queueSlot.name);
    }


    private void Update()
    {
        if (isLeaving)
        {
            UpdateExitMovement();
            return;
        }

        if (isMoving)
        {
            UpdateQueueMovement();
            return;
        }

        if (isWaiting)
        {
            UpdateWaiting();
        }
    }


    // WALK TO QUEUE
    private void UpdateQueueMovement()
    {
        float newX = Mathf.MoveTowards(
            transform.position.x,
            targetX,
            moveSpeed * Time.deltaTime
        );

        bobTimer += Time.deltaTime * bobSpeed;

        float bobY = Mathf.Sin(bobTimer) * bobHeight;

        transform.position = new Vector3(
            newX,
            startY + bobY,
            transform.position.z
        );

        if (Mathf.Abs(newX - targetX) < 0.01f)
        {
            transform.position = new Vector3(
                targetX,
                startY,
                transform.position.z
            );

            StopMoving();
            StartWaiting();
        }
    }


    private void StopMoving()
    {
        isMoving = false;
        bobTimer = 0f;

        if (animator != null)
        {
            animator.SetBool("IsWalking", false);
        }

        transform.position = new Vector3(
            transform.position.x,
            startY,
            transform.position.z
        );

        Debug.Log(name + " arrived at " + currentQueueSlot.name);

        if (!hasGeneratedOrder)
        {
            hasGeneratedOrder = true;

            if (customerOrder != null)
            {
                customerOrder.GenerateOrder();
            }
        }
    }


    // WAIT TIMER
    private void StartWaiting()
    {
        isWaiting = true;

        if (!hasStartedWaiting)
        {
            hasStartedWaiting = true;

            waitTimer = 0f;
            isAngry = false;

            if (customerOrder != null)
            {
                customerOrder.ShowTimeBar();
            }

            Debug.Log(
                name + " started waiting for " +
                maxWaitTime + " seconds."
            );
        }
    }


    private void UpdateWaiting()
    {
        waitTimer += Time.deltaTime;

        if (customerOrder != null)
        {
            customerOrder.UpdateTimeBar(waitTimer, maxWaitTime);
        }

        if (!isAngry && waitTimer >= angryTime)
        {
            BecomeAngry();
        }

        if (waitTimer >= maxWaitTime)
        {
            LeaveAngry();
        }
    }


    private void BecomeAngry()
    {
        isAngry = true;

        if (CoinManager.Instance != null)
        {
            CoinManager.Instance.ApplyAngryPenalty();
        }

        if (customerOrder != null)
        {
            customerOrder.SetTimeBarAngry();
        }

        if (animator != null)
        {
            animator.Play("Angry");
        }

        Debug.Log(name + " is ANGRY! 10 seconds remaining.");
    }


    // WALK OUT ANGRY
    private void LeaveAngry()
    {
        isWaiting = false;
        isLeaving = true;

        if (customerOrder != null)
        {
            customerOrder.HideDialogue();
            customerOrder.HideTimeBar();
        }

        if (animator != null)
        {
            animator.Play("Walk_out_Angry");
        }

        targetX = transform.position.x - 5f;

        if (animator != null)
        {
            animator.SetBool("IsWalking", true);
        }

        Debug.Log(
            name + " wait time expired. Walking out angry to LEFT."
        );
    }


    // WALK OUT HAPPY
    public void LeaveHappy(bool finalTutorialCustomer = false)
    {
        if (isLeaving) return;

        isWaiting = false;
        isAngry = false;
        isMoving = false;

        isFinalTutorialCustomer = finalTutorialCustomer;

        if (customerOrder != null)
        {
            customerOrder.HideDialogue();
            customerOrder.HideTimeBar();
        }

        StartCoroutine(HappyThenWalkOut());
    }


    private IEnumerator HappyThenWalkOut()
    {
        Debug.Log(name + " >>> PLAY HAPPY");

        // HAPPY
        if (animator != null)
        {
            animator.SetBool("IsWalking", false);
            animator.Play("Happy", 0, 0f);
        }

        // Let the happy animation play for two seconds.
        yield return new WaitForSeconds(2f);

        // WALK OUT HAPPY
        Debug.Log(name + " >>> PLAY Walk_out_Happy");

        isLeaving = true;

        targetX = transform.position.x - 5f;

        if (animator != null)
        {
            animator.Play("Walk_out_Happy", 0, 0f);
            animator.SetBool("IsWalking", true);
        }

        Debug.Log(name + " leaving happy.");
    }


    private void UpdateExitMovement()
    {
        float newX = Mathf.MoveTowards(
            transform.position.x,
            targetX,
            moveSpeed * Time.deltaTime
        );

        bobTimer += Time.deltaTime * bobSpeed;

        float bobY = Mathf.Sin(bobTimer) * bobHeight;

        transform.position = new Vector3(
            newX,
            startY + bobY,
            transform.position.z
        );

        if (Mathf.Abs(newX - targetX) < 0.01f)
        {
            FinishLeaving();
        }
    }


    private void FinishLeaving()
    {
        isLeaving = false;

        if (animator != null)
        {
            animator.SetBool("IsWalking", false);
        }

        Debug.Log(name + " finished leaving queue.");

        if (customerQueue != null)
        {
            customerQueue.RemoveCustomer(this);
        }

        bool shouldFinishTutorial = isFinalTutorialCustomer;
        isFinalTutorialCustomer = false;

        // The NPC has completed its exit movement.
        Destroy(gameObject);

        if (shouldFinishTutorial)
        {
            Debug.Log("[CustomerMovement] Final tutorial customer has left. Returning to map.");

            TutorialSession.FinishTutorialAfterLastCustomerLeaves();
        }
    }


    private IEnumerator FinishTutorialAfterDestruction()
    {
        // Wait until Unity processes Destroy(gameObject).
        yield return new WaitForEndOfFrame();

        Debug.Log(
            "[CustomerMovement] Final tutorial NPC destroyed. " +
            "Finishing tutorial."
        );

        TutorialSession.FinishTutorialAfterLastCustomerLeaves();
    }


    // GETTERS
    public bool IsMoving()
    {
        return isMoving;
    }

    public Transform GetCurrentQueueSlot()
    {
        return currentQueueSlot;
    }

    public bool IsLeaving()
    {
        return isLeaving;
    }

    public float GetWaitTimer()
    {
        return waitTimer;
    }

    public bool IsAngryState()
    {
        return isAngry;
    }

    public float GetMaxWaitTime()
    {
        return maxWaitTime;
    }
}