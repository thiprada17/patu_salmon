using UnityEngine;

public class MilkCupSlot : MonoBehaviour
{
    [SerializeField]
    private Transform attachPoint;

    [SerializeField]
    private Milk milk;


    private void OnTriggerEnter2D(Collider2D other)
    {
        Cup cup = other.GetComponentInParent<Cup>();

        if (cup == null)
            return;

        if (other != cup.AttachCollider)
            return;

        if (cup.IsFinished())
            return;

        // เช็ก Attach Point
        if (attachPoint == null)
        {
            Debug.LogError("ยังไม่ได้ใส่ Attach Point");
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

        // บอก Cocoa ว่ามี Cup อยู่
        milk.AttachCup(cup);

        Debug.Log("Cup attached to milk");
    }


    private void OnTriggerExit2D(Collider2D other)
    {
        Cup cup = other.GetComponentInParent<Cup>();

        if (cup == null)
            return;

        // ต้องเป็น AttachCollider เท่านั้น
        if (other != cup.AttachCollider)
            return;

        if (cup.IsFinished())
            return;

        // บอก DraggableItem ว่าหลุดจาก Slot แล้ว
        DraggableItem draggable = cup.GetComponent<DraggableItem>();

        if (draggable != null)
        {
            draggable.SetAttached(false);
        }

        // บอก Cocoa ว่าเอา Cup ออกแล้ว
        if (milk != null)
        {
            milk.RemoveCup(cup);
        }

        Debug.Log("Cup removed from milk");
    }
}