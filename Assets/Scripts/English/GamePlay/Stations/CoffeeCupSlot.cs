using UnityEngine;

public class CoffeeCupSlot : MonoBehaviour
{
    [SerializeField]
    private Transform attachPoint;

    [SerializeField]
    private CoffeeMachine coffeeMachine;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Cup cup = other.GetComponent<Cup>();

        if (cup == null)
            return;

        // ย้าย Portfilter ไปตำแหน่ง Attach Point
        cup.transform.position = attachPoint.position;

        // บอก DraggableItem ว่าตอนนี้ติด Slot แล้ว
        DraggableItem draggable =
            cup.GetComponent<DraggableItem>();

        if (draggable != null)
        {
            draggable.SetAttached(true);
        }

        // บอก CoffeeMachine ว่ามี Portfilter อยู่
        coffeeMachine.AttachCup(cup);

        Debug.Log("cup attached to Coffee machine");
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

        // บอก CoffeeMachine ว่าเอา Portfilter ออกแล้ว
        coffeeMachine.RemoveCup(cup);

        Debug.Log("cup removed from Coffee machine");
    }
}