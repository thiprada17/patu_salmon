

using UnityEngine;


public class CocoaCupSlot : MonoBehaviour
{
    [SerializeField]
    private Transform attachPoint;

    private void OnTriggerEnter2D(Collider2D other)
    {

        Cup cup = other.GetComponentInParent<Cup>();

        if (cup == null)
        {
            Debug.Log("Object is not a Cup!");
            return;
        }

        Debug.Log("Cup detected!");

        cup.transform.position = attachPoint.position;

        DraggableItem draggable =
            cup.GetComponent<DraggableItem>();

        if (draggable != null)
        {
            draggable.SetAttached(true);
        }

        Debug.Log("Cup attached to Cocoa");
    }
}

