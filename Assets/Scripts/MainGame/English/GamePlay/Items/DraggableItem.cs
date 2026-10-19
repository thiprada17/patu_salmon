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

    // เก็บตำแหน่งล่าสุดก่อนเริ่มลากรอบนี้ ไว้เด้งกลับถ้าโดน reject
    private Vector3 lastValidPosition;

    [Header("Reject Feedback")]
    [SerializeField]
    private float shakeDuration = 0.25f;

    [SerializeField]
    private float shakeMagnitude = 0.08f;

    [SerializeField]
    private float returnDuration = 0.2f;

    private Coroutine rejectRoutine;

    [SerializeField] private AudioSource audioSource;
    private void Awake()
    {
        mainCamera = Camera.main;
        lastValidPosition = transform.position;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;

        // เก็บตำแหน่งก่อนเริ่มลากรอบนี้ (เผื่อโดน reject)
        lastValidPosition = transform.position;

        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(eventData.position);
        mouseWorldPosition.z = transform.position.z;
        dragOffset = transform.position - mouseWorldPosition;

        if (isAttached)
        {
            isAttached = false;
            GameLogger.DebugLog("Dragable",gameObject.name + " detached!");
        }

        if (gameObject.name == "Cup(Clone)")
        {
            if (audioSource != null)
            {
                audioSource.PlayOneShot(audioSource.clip);
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || isAttached)
            return;

        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(eventData.position);
        mouseWorldPosition.z = transform.position.z;
        transform.position = mouseWorldPosition + dragOffset;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
    }

    public void SetAttached(bool value)
    {
        isAttached = value;

        //ถ้า attach สำเร็จ ให้ตำแหน่งปัจจุบัน (attach point) กลายเป็นตำแหน่งที่ปลอดภัยรอบถัดไป
        if (value)
        {
            lastValidPosition = transform.position;
        }
    }

    // เรียกจาก slot ตอนวางไม่ได้ (slot เต็ม/ไม่ตรงเงื่อนไข)
    public void RejectDrop()
    {
        if (rejectRoutine != null)
        {
            StopCoroutine(rejectRoutine);
        }

        rejectRoutine = StartCoroutine(ShakeAndReturn());
    }

    private IEnumerator ShakeAndReturn()
    {
        isDragging = false; // กันไม่ให้ OnDrag แย่งคุมตำแหน่งระหว่าง shake

        Vector3 shakeCenter = transform.position;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float offsetX = Random.Range(-1f, 1f) * shakeMagnitude;
            transform.position = shakeCenter + new Vector3(offsetX, 0f, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        Vector3 startPos = transform.position;
        float t = 0f;

        while (t < returnDuration)
        {
            transform.position = Vector3.Lerp(startPos, lastValidPosition, t / returnDuration);
            t += Time.deltaTime;
            yield return null;
        }

        transform.position = lastValidPosition;
        rejectRoutine = null;
    }
}