using UnityEngine;

public class RMatCupSlot : MonoBehaviour, IStationSnapTarget
{
    [SerializeField] private Transform attachPoint;
    [SerializeField] private Mat mat;
    [SerializeField] private CupDispenser cupDispenser;

    public Transform AttachPoint => attachPoint;

    public bool CanAccept(DraggableItem item)
    {
        Cup cup = item != null ? item.GetComponent<Cup>() : null;
        return cup != null && !cup.IsFinished() && attachPoint != null &&
               (cupDispenser == null || !cupDispenser.IsSlotOccupiedByOther("R", cup));
    }

    public bool TryAttach(DraggableItem item)
    {
        if (!CanAccept(item)) return false;
        Cup cup = item.GetComponent<Cup>();
        cup.transform.position = attachPoint.position;
        item.SetAttached(true, this);
        if (mat != null) mat.AttachRightCup(cup);
        if (cupDispenser != null) cupDispenser.OccupySlot("R", cup);
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
        if (cup == null || other != cup.AttachCollider) return;
        DraggableItem draggable = cup.GetComponent<DraggableItem>();
        if (draggable != null) draggable.SetAttached(false, this);
        if (mat != null) mat.RemoveRightCup(cup);
        if (cupDispenser != null) cupDispenser.ReleaseSlot("R", cup);
    }
}
