using UnityEngine;

public class CoffeeMachinePortSlot : MonoBehaviour
{
    [SerializeField]
    private Transform attachPoint;

    [SerializeField]
    private CoffeeMachine coffeeMachine;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Portfilter portfilter = other.GetComponent<Portfilter>();

        if (portfilter == null)
            return;

        // ย้าย Portfilter ไปตำแหน่ง Attach Point
        portfilter.transform.position = attachPoint.position;

        // บอก DraggableItem ว่าตอนนี้ติด Slot แล้ว
        DraggableItem draggable =
            portfilter.GetComponent<DraggableItem>();

        if (draggable != null)
        {
            draggable.SetAttached(true);
        }

        // บอก CoffeeMachine ว่ามี Portfilter อยู่
        coffeeMachine.AttachPortfilter(portfilter);
        GameLogger.DebugLog("CoffeeMech", "Portfilter attached to Coffee machine");
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Portfilter portfilter = other.GetComponent<Portfilter>();

        if (portfilter == null)
            return;

        // บอก DraggableItem ว่าหลุดจาก Slot แล้ว
        DraggableItem draggable =
            portfilter.GetComponent<DraggableItem>();

        if (draggable != null)
        {
            draggable.SetAttached(false);
        }

        // บอก CoffeeMachine ว่าเอา Portfilter ออกแล้ว
        coffeeMachine.RemovePortfilter(portfilter);
        GameLogger.DebugLog("CoffeeMech", "Portfilter removed from Coffee machine");
    }
}