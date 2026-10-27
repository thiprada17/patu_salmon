using UnityEngine;

public class PortfilterSlot : MonoBehaviour, IStationSnapTarget
{
    [SerializeField] private Transform attachPoint;

    public Transform AttachPoint => attachPoint;

    public bool CanAccept(DraggableItem item)
    {
        return item != null && item.GetComponent<Portfilter>() != null &&
               attachPoint != null && GetComponentInParent<Grinder>() != null;
    }

    public bool TryAttach(DraggableItem item)
    {
        if (!CanAccept(item)) return false;
        Portfilter portfilter = item.GetComponent<Portfilter>();
        Grinder grinder = GetComponentInParent<Grinder>();
        portfilter.transform.position = attachPoint.position;
        item.SetAttached(true, this);
        grinder.AttachPortfilter(portfilter);
        return true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Portfilter portfilter = other.GetComponentInParent<Portfilter>();
        if (portfilter != null)
            TryAttach(portfilter.GetComponent<DraggableItem>());
    }
}
