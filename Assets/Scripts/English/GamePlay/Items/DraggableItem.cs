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

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;

        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint( eventData.position );

        mouseWorldPosition.z = transform.position.z;

        dragOffset = transform.position - mouseWorldPosition;

        if (isAttached)
        {
            isAttached = false;
            Debug.Log(gameObject.name + " detached!");
        }

        Debug.Log(gameObject.name + " Start Drag");
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || isAttached)
            return;

        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint( eventData.position );

        mouseWorldPosition.z = transform.position.z;

        transform.position = mouseWorldPosition + dragOffset;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;

        Debug.Log(gameObject.name + " Drop");
    }

    public void SetAttached(bool value)
    {
        isAttached = value;
    }
}