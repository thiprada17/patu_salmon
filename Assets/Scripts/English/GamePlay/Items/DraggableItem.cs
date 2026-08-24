using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableItem : MonoBehaviour,
    IPointerDownHandler,
    IDragHandler,
    IPointerUpHandler
{
    private Camera mainCamera;

    private bool isAttached = false;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // ถ้าตอนนี้ติด Slot อยู่
        // การคลิกครั้งนี้ = ปลดออก
        if (isAttached)
        {
            isAttached = false;
            Debug.Log("Portfilter detached!");
        }

        Debug.Log("Start Drag");
    }

    public void OnDrag(PointerEventData eventData)
    {
        // ถ้ายังติด Slot อยู่ ห้ามขยับตามเมาส์
        if (isAttached)
            return;

        Vector3 worldPosition =
            mainCamera.ScreenToWorldPoint(eventData.position);

        worldPosition.z = transform.position.z;

        transform.position = worldPosition;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log("Drop");
    }

    public void SetAttached(bool value)
    {
        isAttached = value;
    }
}