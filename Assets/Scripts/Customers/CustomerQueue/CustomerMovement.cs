using UnityEngine;

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
        {
            return;
        }

        if (queueSlot == null)
        {
            Debug.LogError(name + " received NULL queue slot!");
            return;
        }

        currentQueueSlot = queueSlot;

        targetX = queueSlot.position.x;

        isMoving = true;
        isWaiting = false;

        waitTimer = 0f;
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

        float bobY =
            Mathf.Sin(bobTimer) * bobHeight;

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

        Debug.Log(
            name + " arrived at " + currentQueueSlot.name
        );

        // Generate Order แค่ครั้งแรกเท่านั้น
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
        waitTimer = 0f;
        isAngry = false;

        Debug.Log(
            name +
            " started waiting for " +
            maxWaitTime +
            " seconds."
        );
    }


    private void UpdateWaiting()
    {
        waitTimer += Time.deltaTime;

        // เหลือ 10 วินาที
        if (!isAngry && waitTimer >= angryTime)
        {
            BecomeAngry();
        }

        // ครบ 30 วินาที
        if (waitTimer >= maxWaitTime)
        {
            LeaveAngry();
        }
    }


    private void BecomeAngry()
    {
        isAngry = true;

        if (animator != null)
        {
            animator.Play("Angry");
        }

        Debug.Log(
            name +
            " is ANGRY! 10 seconds remaining."
        );
    }


    // WALK OUT ANGRY
    private void LeaveAngry()
    {
        isWaiting = false;
        isLeaving = true;

        // ซ่อน Bubble ตอน Customer เดินออก
        if (customerOrder != null)
        {
            customerOrder.HideDialogue();
        }

        if (animator != null)
        {
            animator.Play("Walk_Out_Angry");
        }

        // เดินไปทางซ้าย
        targetX = transform.position.x - 5f;

        if (animator != null)
        {
            animator.SetBool("IsWalking", true);
        }

        Debug.Log(
            name +
            " wait time expired. Walking out angry to LEFT."
        );
    }

    public void LeaveHappy()
    {
        if (isLeaving)
            return;

        isLeaving = true;
        isWaiting = false;
        isAngry = false;

        if (animator != null)
        {
            animator.Play("Walk_out_Happy");
        }

        targetX = transform.position.x - 5f;

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

        float bobY =
            Mathf.Sin(bobTimer) * bobHeight;

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

        Debug.Log(
            name +
            " finished leaving queue."
        );

        if (customerQueue != null)
        {
            customerQueue.RemoveCustomer(this);
        }

        Destroy(gameObject);
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
}