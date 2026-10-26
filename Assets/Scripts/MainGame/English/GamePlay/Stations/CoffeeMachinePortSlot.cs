
using UnityEngine;

public class CoffeeMachinePortSlot : MonoBehaviour
{
    [Header("Slot Setup")]
    [SerializeField] private Transform attachPoint;
    [SerializeField] private CoffeeMachine coffeeMachine;
    [SerializeField] private Grinder grinder;

    private Portfilter attachedPortfilter;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Portfilter portfilter = other.GetComponent<Portfilter>();

        if (portfilter == null)
            return;

        // ป้องกันการ Attach ซ้ำ
        if (attachedPortfilter == portfilter)
            return;

        // แจ้ง Grinder ให้ปล่อย Portfilter ก่อน
        if (grinder != null)
        {
            grinder.RemovePortfilter(portfilter);
        }

        // ถ้ามี Portfilter ตัวเดิม ให้เคลียร์สถานะก่อน
        if (attachedPortfilter != null)
        {
            coffeeMachine.RemovePortfilter(attachedPortfilter);
        }

        attachedPortfilter = portfilter;

        // ย้าย Portfilter ไปยังตำแหน่ง Slot
        if (attachPoint != null)
        {
            portfilter.transform.position = attachPoint.position;
        }

        DraggableItem draggable =
            portfilter.GetComponent<DraggableItem>();

        if (draggable != null)
        {
            draggable.SetAttached(true);
        }

        if (coffeeMachine != null)
        {
            coffeeMachine.AttachPortfilter(portfilter);
        }

        GameLogger.DebugLog(
            "CoffeeMachinePortSlot",
            "Portfilter attached to Coffee Machine slot."
        );
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Portfilter portfilter = other.GetComponent<Portfilter>();

        if (portfilter == null || attachedPortfilter != portfilter)
            return;

        attachedPortfilter = null;

        DraggableItem draggable =
            portfilter.GetComponent<DraggableItem>();

        if (draggable != null)
        {
            draggable.SetAttached(false);
        }

        if (coffeeMachine != null)
        {
            coffeeMachine.RemovePortfilter(portfilter);
        }

        GameLogger.DebugLog(
            "CoffeeMachinePortSlot",
            "Portfilter removed from Coffee Machine slot."
        );
    }
}