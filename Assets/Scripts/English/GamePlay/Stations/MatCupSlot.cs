using UnityEngine;

public class MatCupSlot : MonoBehaviour
{
    [SerializeField]
    private Transform attachPoint;

    [SerializeField]
    private Mat mat;


    private void OnTriggerEnter2D(Collider2D other)
    {
        Cup cup = other.GetComponent<Cup>();

        if (cup == null)
            return;

        // เช็ก Attach Point
        if (attachPoint == null)
        {
            Debug.LogError("ยังไม่ได้ใส่ Attach Point");
            return;
        }

        // ย้าย Cup ไปตำแหน่ง Attach Point
        cup.transform.position =
            attachPoint.position;

        // บอก DraggableItem ว่าตอนนี้ติด Slot แล้ว
        DraggableItem draggable =
            cup.GetComponent<DraggableItem>();

        if (draggable != null)
        {
            draggable.SetAttached(true);
        }

        // บอก Cocoa ว่ามี Cup อยู่
        mat.AttachCup(cup);

        Debug.Log("Cup attached to mat");
    }


    private void OnTriggerExit2D(Collider2D other)
    {
        Cup cup = other.GetComponent<Cup>();

        if (cup == null)
            return;

        // บอก DraggableItem ว่าหลุดจาก Slot แล้ว
        DraggableItem draggable =
            cup.GetComponent<DraggableItem>();

        if (draggable != null)
        {
            draggable.SetAttached(false);
        }

        // บอก Cocoa ว่าเอา Cup ออกแล้ว
        if (mat != null)
        {
            mat.RemoveCup(cup);
        }

        Debug.Log("Cup removed from mat");
    }
}