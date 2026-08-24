using UnityEngine;

public class PortfilterSlot : MonoBehaviour
{
    [SerializeField]
    private Transform attachPoint;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Portfilter portfilter = other.GetComponent<Portfilter>();

        if (portfilter == null)
            return;

        // ย้าย Portfilter ไปที่ AttachPoint
        portfilter.transform.position = attachPoint.position;

        // บอกว่า Portfilter ติดอยู่กับ Slot
        DraggableItem draggable =
            portfilter.GetComponent<DraggableItem>();

        if (draggable != null)
        {
            draggable.SetAttached(true);
        }

        // ส่ง Portfilter ให้ Grinder
        Grinder grinder = GetComponentInParent<Grinder>();

        if (grinder != null)
        {
            grinder.AttachPortfilter(portfilter);
        }

        Debug.Log("Portfilter attached to Grinder!");
    }
}