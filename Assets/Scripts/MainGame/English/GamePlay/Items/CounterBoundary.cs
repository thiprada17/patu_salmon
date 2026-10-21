using UnityEngine;

public class CounterBoundary : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        DraggableItem item = other.GetComponentInParent<DraggableItem>();

        if (item == null)
            return;

        Cup cup = item.GetComponent<Cup>();

        // FinishedCup ออกจาก Counter ได้
        if (cup != null && cup.IsFinished())
            return;

        item.RejectDrop();
    }
}