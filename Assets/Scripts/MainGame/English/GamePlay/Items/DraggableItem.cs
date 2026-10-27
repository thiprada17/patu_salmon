
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public interface IStationSnapTarget
{
    Transform AttachPoint { get; }
    bool CanAccept(DraggableItem item);
    bool TryAttach(DraggableItem item);
}

public static class StationSnapResolver
{
    public static bool TryAttachNearest(
        DraggableItem item,
        Vector2 dropPosition,
        float snapRadius)
    {
        if (item == null || snapRadius <= 0f)
            return false;

        MonoBehaviour[] components =
            Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);

        IStationSnapTarget nearest = null;
        float nearestDistance = snapRadius * snapRadius;

        foreach (MonoBehaviour component in components)
        {
            if (component == null || !(component is IStationSnapTarget target))
                continue;

            Transform point = target.AttachPoint;
            if (point == null || !target.CanAccept(item))
                continue;

            float distance =
                ((Vector2)point.position - dropPosition).sqrMagnitude;
            if (distance > nearestDistance)
                continue;

            nearest = target;
            nearestDistance = distance;
        }

        return nearest != null && nearest.TryAttach(item);
    }
}

public class DraggableItem : MonoBehaviour,
    IPointerDownHandler,
    IDragHandler,
    IPointerUpHandler
{
    [Header("Station Snap")]
    [SerializeField, Min(0.1f)] private float stationSnapRadius = 1.25f;

    [Header("Reject Feedback")]
    [SerializeField] private float shakeDuration = 0.25f;
    [SerializeField] private float shakeMagnitude = 0.08f;
    [SerializeField] private float returnDuration = 0.2f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;

    private Camera mainCamera;
    private Rigidbody2D body;
    private Collider2D[] itemColliders;
    private bool[] originalTriggerStates;
    private bool isAttached;
    private bool isDragging;
    private bool wasLockedLastFrame;
    private Vector3 dragOffset;
    private Vector3 dragTargetPosition;
    private Vector3 lastValidPosition;
    private Vector3 positionBeforeLock;
    private Component attachedSlot;
    private Coroutine rejectRoutine;

    private void Awake()
    {
        mainCamera = Camera.main;
        body = GetComponent<Rigidbody2D>();
        itemColliders = GetComponents<Collider2D>();
        originalTriggerStates = new bool[itemColliders.Length];
        for (int i = 0; i < itemColliders.Length; i++)
            originalTriggerStates[i] = itemColliders[i].isTrigger;

        lastValidPosition = transform.position;
        positionBeforeLock = transform.position;
        dragTargetPosition = transform.position;

        if (body != null)
        {
            body.gravityScale = 0f;
            body.constraints |= RigidbodyConstraints2D.FreezeRotation;
            body.bodyType = RigidbodyType2D.Kinematic;
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
        }
    }

    private void Update()
    {
        if (!GameplayInputLock.IsLocked)
        {
            wasLockedLastFrame = false;
            return;
        }

        if (!wasLockedLastFrame)
        {
            positionBeforeLock = transform.position;
            wasLockedLastFrame = true;
        }

        isDragging = false;
        if (rejectRoutine != null)
        {
            StopCoroutine(rejectRoutine);
            rejectRoutine = null;
        }

        SetPhysicsKinematic();
        transform.position = positionBeforeLock;
        dragTargetPosition = positionBeforeLock;
    }

    private void FixedUpdate()
    {
        if (isDragging && !isAttached && body != null &&
            !GameplayInputLock.IsLocked)
        {
            body.MovePosition(dragTargetPosition);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (GameplayInputLock.IsLocked)
            return;

        if (mainCamera == null)
            mainCamera = Camera.main;
        if (mainCamera == null)
        {
            GameLogger.Warn("Draggable", "Main Camera not found.");
            return;
        }

        if (rejectRoutine != null)
        {
            StopCoroutine(rejectRoutine);
            rejectRoutine = null;
        }

        lastValidPosition = transform.position;
        Vector3 pointerPosition = GetPointerWorldPosition(eventData.position);
        dragOffset = transform.position - pointerPosition;
        dragTargetPosition = transform.position;

        if (isAttached)
        {
            isAttached = false;
            attachedSlot = null;
            GameLogger.DebugLog("Draggable", gameObject.name + " detached.");
        }

        isDragging = true;
        SetPhysicsForDrag();

        if (gameObject.name == "Cup(Clone)" &&
            audioSource != null && audioSource.clip != null)
        {
            audioSource.PlayOneShot(audioSource.clip);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (GameplayInputLock.IsLocked)
        {
            isDragging = false;
            return;
        }

        if (!isDragging || isAttached)
            return;

        Vector3 pointerPosition = GetPointerWorldPosition(eventData.position);
        dragTargetPosition = pointerPosition + dragOffset;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (GameplayInputLock.IsLocked)
        {
            isDragging = false;
            SetPhysicsKinematic();
            return;
        }

        if (!isDragging)
            return;

        Vector3 pointerPosition = GetPointerWorldPosition(eventData.position);
        dragTargetPosition = pointerPosition + dragOffset;
        isDragging = false;

        if (body != null)
        {
            body.position = dragTargetPosition;
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
        }
        else
        {
            transform.position = dragTargetPosition;
        }

        bool mustBeInStationSlot =
            GetComponent<Cup>() != null || GetComponent<Portfilter>() != null;

        if (mustBeInStationSlot && !isAttached)
        {
            isAttached = StationSnapResolver.TryAttachNearest(
                this,
                transform.position,
                stationSnapRadius
            );

            if (!isAttached)
            {
                RejectDrop();
                return;
            }
        }

        lastValidPosition = transform.position;
        SetPhysicsKinematic();
    }

    private Vector3 GetPointerWorldPosition(Vector2 screenPosition)
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(screenPosition);
        worldPosition.z = transform.position.z;
        return worldPosition;
    }

    public void SetAttached(bool value)
    {
        SetAttached(value, null);
    }

    public void SetAttached(bool value, Component slot)
    {
        if (!value && slot != null && attachedSlot != slot)
            return;

        isAttached = value;
        if (value)
        {
            attachedSlot = slot;
            isDragging = false;
            lastValidPosition = transform.position;
            dragTargetPosition = transform.position;
            SetPhysicsKinematic();
        }
        else
        {
            attachedSlot = null;
        }
    }

    public void RejectDrop()
    {
        if (GameplayInputLock.IsLocked)
            return;

        if (rejectRoutine != null)
            StopCoroutine(rejectRoutine);

        SetPhysicsKinematic(restoreCollisions: false);
        rejectRoutine = StartCoroutine(ShakeAndReturn());
    }

    private void SetPhysicsForDrag()
    {
        SetCollisionTriggers(true);

        if (body != null)
        {
            body.gravityScale = 0f;
            body.constraints |= RigidbodyConstraints2D.FreezeRotation;
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
            body.bodyType = RigidbodyType2D.Kinematic;
        }
    }

    private void SetPhysicsKinematic(bool restoreCollisions = true)
    {
        if (restoreCollisions)
            SetCollisionTriggers(false);

        if (body != null)
        {
            body.linearVelocity = Vector2.zero;
            body.angularVelocity = 0f;
            body.bodyType = RigidbodyType2D.Kinematic;
        }
    }

    private void SetCollisionTriggers(bool dragging)
    {
        if (itemColliders == null)
            return;

        for (int i = 0; i < itemColliders.Length; i++)
        {
            if (itemColliders[i] != null)
                itemColliders[i].isTrigger = dragging || originalTriggerStates[i];
        }
    }

    private IEnumerator ShakeAndReturn()
    {
        isDragging = false;
        isAttached = false;
        attachedSlot = null;

        Vector3 shakeCenter = transform.position;
        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            if (GameplayInputLock.IsLocked)
            {
                transform.position = positionBeforeLock;
                rejectRoutine = null;
                yield break;
            }

            float offsetX = Random.Range(-1f, 1f) * shakeMagnitude;
            transform.position = shakeCenter + new Vector3(offsetX, 0f, 0f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        Vector3 startPosition = transform.position;
        float time = 0f;
        while (time < returnDuration)
        {
            if (GameplayInputLock.IsLocked)
            {
                transform.position = positionBeforeLock;
                rejectRoutine = null;
                yield break;
            }

            float progress = returnDuration > 0f
                ? Mathf.Clamp01(time / returnDuration)
                : 1f;
            transform.position = Vector3.Lerp(
                startPosition,
                lastValidPosition,
                progress
            );
            time += Time.deltaTime;
            yield return null;
        }

        transform.position = lastValidPosition;
        if (GetComponent<Cup>() != null || GetComponent<Portfilter>() != null)
        {
            bool attached = StationSnapResolver.TryAttachNearest(
                this,
                lastValidPosition,
                stationSnapRadius
            );

            if (!attached)
                SetPhysicsKinematic();
        }
        else
            SetPhysicsKinematic();
        rejectRoutine = null;
    }
}
