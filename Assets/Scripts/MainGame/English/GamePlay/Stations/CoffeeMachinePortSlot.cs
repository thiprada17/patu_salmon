using UnityEngine;

public class CoffeeMachinePortSlot : MonoBehaviour, IStationSnapTarget
{
    [Header("Slot Setup")]
    [SerializeField] private Transform attachPoint;
    [SerializeField] private CoffeeMachine coffeeMachine;
    [SerializeField] private Grinder grinder;

    private Portfilter attachedPortfilter;

    public Transform AttachPoint => attachPoint;

    public bool CanAccept(DraggableItem item)
    {
        return item != null && item.GetComponent<Portfilter>() != null &&
               attachPoint != null && coffeeMachine != null &&
               (attachedPortfilter == null || attachedPortfilter == item.GetComponent<Portfilter>());
    }

    public bool TryAttach(DraggableItem item)
    {
        if (!CanAccept(item)) return false;
        Portfilter portfilter = item.GetComponent<Portfilter>();
        if (attachedPortfilter == portfilter) return true;

        if (grinder != null)
            grinder.RemovePortfilter(portfilter);

        if (attachedPortfilter != null)
        {
            DraggableItem previous = attachedPortfilter.GetComponent<DraggableItem>();
            if (previous != null) previous.SetAttached(false, this);
            coffeeMachine.RemovePortfilter(attachedPortfilter);
        }

        attachedPortfilter = portfilter;
        portfilter.transform.position = attachPoint.position;
        item.SetAttached(true, this);
        coffeeMachine.AttachPortfilter(portfilter);
        return true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Portfilter portfilter = other.GetComponentInParent<Portfilter>();
        if (portfilter != null)
            TryAttach(portfilter.GetComponent<DraggableItem>());
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Portfilter portfilter = other.GetComponentInParent<Portfilter>();
        if (portfilter == null || attachedPortfilter != portfilter)
            return;

        attachedPortfilter = null;
        DraggableItem draggable = portfilter.GetComponent<DraggableItem>();
        if (draggable != null) draggable.SetAttached(false, this);
        if (coffeeMachine != null) coffeeMachine.RemovePortfilter(portfilter);
    }
}
