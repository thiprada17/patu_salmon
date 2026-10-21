
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableItem : MonoBehaviour,
    IPointerDownHandler,
    IDragHandler,
    IPointerUpHandler
{
    private Camera mainCamera;

    private bool isAttached = false;
    private bool isDragging = false;

    private Vector3 dragOffset;
    private Vector3 lastValidPosition;
    private Vector3 positionBeforeLock;

    private bool wasLockedLastFrame = false;

    [Header("Reject Feedback")]
    [SerializeField] private float shakeDuration = 0.25f;
    [SerializeField] private float shakeMagnitude = 0.08f;
    [SerializeField] private float returnDuration = 0.2f;

    private Coroutine rejectRoutine;

    private void Awake()
    {
        mainCamera = Camera.main;
        lastValidPosition = transform.position;
        positionBeforeLock = transform.position;
    }

    private void Update()
    {
        if (GameplayInputLock.IsLocked)
        {
            // Remember the object's position when the lock starts.
            if (!wasLockedLastFrame)
            {
                positionBeforeLock = transform.position;
                wasLockedLastFrame = true;
            }

            // Cancel any active drag and rejection animation.
            isDragging = false;

            if (rejectRoutine != null)
            {
                StopCoroutine(rejectRoutine);
                rejectRoutine = null;
            }

            // Restore the position in case the lock started mid-drag.
            transform.position = positionBeforeLock;
        }
        else
        {
            wasLockedLastFrame = false;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (GameplayInputLock.IsLocked)
            return;

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera == null)
            return;

        if (rejectRoutine != null)
        {
            StopCoroutine(rejectRoutine);
            rejectRoutine = null;
        }

        isDragging = true;
        lastValidPosition = transform.position;

        Vector3 mouseWorldPosition =
            mainCamera.ScreenToWorldPoint(eventData.position);

        mouseWorldPosition.z = transform.position.z;

        dragOffset = transform.position - mouseWorldPosition;

        if (isAttached)
        {
            isAttached = false;

            GameLogger.DebugLog(
                "Dragable",
                gameObject.name + " detached!"
            );
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

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera == null)
            return;

        Vector3 mouseWorldPosition =
            mainCamera.ScreenToWorldPoint(eventData.position);

        mouseWorldPosition.z = transform.position.z;

        transform.position = mouseWorldPosition + dragOffset;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (GameplayInputLock.IsLocked)
        {
            isDragging = false;
            return;
        }

        isDragging = false;
        lastValidPosition = transform.position;
    }

    public void SetAttached(bool value)
    {
        isAttached = value;

        if (value)
        {
            lastValidPosition = transform.position;
            isDragging = false;
        }
    }

    public void RejectDrop()
    {
        if (GameplayInputLock.IsLocked)
            return;

        if (rejectRoutine != null)
            StopCoroutine(rejectRoutine);

        rejectRoutine = StartCoroutine(ShakeAndReturn());
    }

    private IEnumerator ShakeAndReturn()
    {
        isDragging = false;

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

            float offsetX =
                Random.Range(-1f, 1f) * shakeMagnitude;

            transform.position =
                shakeCenter + new Vector3(offsetX, 0f, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        Vector3 startPos = transform.position;
        float t = 0f;

        while (t < returnDuration)
        {
            if (GameplayInputLock.IsLocked)
            {
                transform.position = positionBeforeLock;
                rejectRoutine = null;
                yield break;
            }

            transform.position = Vector3.Lerp(
                startPos,
                lastValidPosition,
                t / returnDuration
            );

            t += Time.deltaTime;
            yield return null;
        }

        transform.position = lastValidPosition;
        rejectRoutine = null;
    }
}
