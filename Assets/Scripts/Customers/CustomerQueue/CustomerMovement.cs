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

    private float targetX;
    private bool isMoving;

    private float startY;
    private float bobTimer;

    private Transform currentQueueSlot;


    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        startY = transform.position.y;
    }


    public void MoveToQueueSlot(Transform queueSlot)
    {
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

        bobTimer = 0f;

        if (animator != null)
        {
            animator.SetBool("IsWalking", true);
        }

        Debug.Log(
            name +
            " moving to " +
            queueSlot.name
        );
    }


    private void Update()
    {
        if (!isMoving)
            return;


        // Move X
        float newX = Mathf.MoveTowards(
            transform.position.x,
            targetX,
            moveSpeed * Time.deltaTime
        );


        // Walking bob
        bobTimer += Time.deltaTime * bobSpeed;

        float bobY =
            Mathf.Sin(bobTimer) * bobHeight;


        transform.position = new Vector3(
            newX,
            startY + bobY,
            transform.position.z
        );


        // Arrived
        if (Mathf.Abs(newX - targetX) < 0.01f)
        {
            transform.position = new Vector3(
                targetX,
                startY,
                transform.position.z
            );

            StopMoving();
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
            name +
            " arrived at " +
            currentQueueSlot.name
        );
    }


    public bool IsMoving()
    {
        return isMoving;
    }


    public Transform GetCurrentQueueSlot()
    {
        return currentQueueSlot;
    }
}