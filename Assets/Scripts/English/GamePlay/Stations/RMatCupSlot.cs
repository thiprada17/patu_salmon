using UnityEngine;

public class RMatCupSlot : MonoBehaviour
{
    [SerializeField]
    private Transform attachPoint;

    [SerializeField]
    private Mat mat;

    [SerializeField]
    private CupDispenser cupDispenser;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Cup cup = other.GetComponentInParent<Cup>();

        if (cup == null) return;

        // ต้องเป็น AttachCollider เท่านั้น
        if (other != cup.AttachCollider) return;

        if (cup.IsFinished()) return;

        // เช็คก่อนว่า slot นี้มีแก้วอยู่แล้วรึยัง
        if (cupDispenser != null && cupDispenser.IsSlotOccupied("R"))
        {
            DraggableItem rejectedDraggable = cup.GetComponent<DraggableItem>();
            if (rejectedDraggable != null)
            {
                rejectedDraggable.RejectDrop();
            }

            return;
        }

        // เช็ก Attach Point
        if (attachPoint == null)
        {
            GameLogger.Warn("RMat", "ยังไม่ได้ใส่ Attach Point");
            return;
        }

        // ย้าย Cup ไปตำแหน่ง Attach Point
        cup.transform.position = attachPoint.position;

        // บอก DraggableItem ว่าตอนนี้ติด Slot แล้ว
        DraggableItem draggable = cup.GetComponent<DraggableItem>();
        if (draggable != null)
        {
            draggable.SetAttached(true);
        }

        // บอก Mat
        if (mat != null)
        {
            mat.AttachRightCup(cup);
        }

        // บอก CupDispenser ว่า Left มี Cup
        if (cupDispenser != null)
        {
            cupDispenser.OccupySlot("R", cup);
        }

        GameLogger.DebugLog("RMat", "Cup attached to RIGHT slot");
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Cup cup = other.GetComponentInParent<Cup>();

        if (cup == null)    return;

        // ต้องเป็น AttachCollider เท่านั้น
        if (other != cup.AttachCollider)    return;

        // ลบเช็ค IsFinished() ออกน้า ไม่งั้น slot จะไม่ถูกปล่อยเวลาแก้วที่ finished แล้วถูกลากออก

        // บอก DraggableItem ว่าหลุดจาก Slot แล้ว
        DraggableItem draggable = cup.GetComponent<DraggableItem>();

        if (draggable != null)
        {
            draggable.SetAttached(false);
        }

        // บอก Mat
        if (mat != null)
        {
            mat.RemoveRightCup(cup);
        }

        // บอก Dispenser ว่า RIGGHT ว่างแล้ว
        if (cupDispenser != null)
        {
            cupDispenser.ReleaseSlot("R", cup);
        }

        GameLogger.DebugLog("RMat", "Cup removed from RIGHT slot");
    }
}