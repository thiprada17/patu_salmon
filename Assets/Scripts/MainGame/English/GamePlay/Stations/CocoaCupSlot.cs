using UnityEngine;

public class CocoaCupSlot : MonoBehaviour, IStationSnapTarget
{
    [SerializeField] private Transform attachPoint;
    [SerializeField] private Cocoa cocoa;

    public Transform AttachPoint => attachPoint;

    public bool CanAccept(DraggableItem item)
    {
        Cup cup = item != null ? item.GetComponent<Cup>() : null;
        return cup != null && !cup.IsFinished() && attachPoint != null && cocoa != null;
    }

    public bool TryAttach(DraggableItem item)
    {
        if (!CanAccept(item)) return false;
        Cup cup = item.GetComponent<Cup>();
        cup.transform.position = attachPoint.position;
        item.SetAttached(true, this);
        cocoa.AttachCup(cup);
        return true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Cup cup = other.GetComponentInParent<Cup>();
        if (cup != null && other == cup.AttachCollider)
            TryAttach(cup.GetComponent<DraggableItem>());
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Cup cup = other.GetComponentInParent<Cup>();
        if (cup == null || other != cup.AttachCollider || cup.IsFinished()) return;
        DraggableItem draggable = cup.GetComponent<DraggableItem>();
        if (draggable != null) draggable.SetAttached(false, this);
        cocoa.RemoveCup(cup);
    }
}
