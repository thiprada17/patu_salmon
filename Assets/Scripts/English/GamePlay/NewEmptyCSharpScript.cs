
}using UnityEngine;
using UnityEngine.EventSystems;

public class NewEmptyCSharpScript : MonoBehaviour,
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


        Debug.Log(gameObject.name + " Start Drag");
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isAttached)
            return;

        Vector3 worldPosition =
            mainCamera.ScreenToWorldPoint(eventData.position);

        worldPosition.z = transform.position.z;

        transform.position = worldPosition;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log(gameObject.name + " Drop");
    }

    public void SetAttached(bool value)
    {
        isAttached = value;
    }
}