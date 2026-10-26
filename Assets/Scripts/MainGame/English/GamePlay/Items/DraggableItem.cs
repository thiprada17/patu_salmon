
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

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;

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
            // Save position when the gameplay lock starts.
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

            // Prevent movement while gameplay is locked.
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
        {
            GameLogger.Warn(
                "Draggable",
                "Main Camera not found."
            );
            return;
        }

        if (rejectRoutine != null)
        {
            StopCoroutine(rejectRoutine);
            rejectRoutine = null;
        }

        // Save the position before starting this drag.
        lastValidPosition = transform.position;

        Vector3 mouseWorldPosition =
            mainCamera.ScreenToWorldPoint(eventData.position);

        mouseWorldPosition.z = transform.position.z;

        dragOffset = transform.position - mouseWorldPosition;

        if (isAttached)
        {
            isAttached = false;

            GameLogger.DebugLog(
                "Draggable",
                gameObject.name + " detached."
            );
        }

        isDragging = true;

        if (gameObject.name == "Cup(Clone)" &&
            audioSource != null &&
            audioSource.clip != null)
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

        // Cups and portfilters must finish a drag in a station slot. Keep the
        // previous valid position so releasing over the counter or another
        // prop returns the item instead of leaving it wherever it was dropped.
        bool mustBeInStationSlot =
            GetComponent<Cup>() != null || GetComponent<Portfilter>() != null;

        if (mustBeInStationSlot && !isAttached)
        {
            RejectDrop();
            return;
        }

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

            float progress = returnDuration > 0f
                ? Mathf.Clamp01(t / returnDuration)
                : 1f;

            transform.position = Vector3.Lerp(
                startPos,
                lastValidPosition,
                progress
            );

            t += Time.deltaTime;
            yield return null;
        }

        transform.position = lastValidPosition;
        rejectRoutine = null;
    }
}
