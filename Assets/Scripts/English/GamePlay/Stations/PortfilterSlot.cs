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

        // ย้ายไปตำแหน่งที่กำหนด
        portfilter.transform.position = attachPoint.position;

        // บอก DraggableItem ว่าตอนนี้ติด Slot แล้ว
        DraggableItem draggable =
            portfilter.GetComponent<DraggableItem>();

        if (draggable != null)
        {
            draggable.SetAttached(true);
        }

        Debug.Log("Portfilter attached to Slot!");
    }
}