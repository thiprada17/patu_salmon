
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class MobileSpriteClick : MonoBehaviour
{
    private Camera mainCamera;
    private Collider2D targetCollider;

    private void Awake()
    {
        mainCamera = Camera.main;
        targetCollider = GetComponent<Collider2D>();
    }

    private void Update()
    {
        Vector2 screenPosition;
        bool pressed = false;

        if (Touchscreen.current != null &&
            Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            screenPosition =
                Touchscreen.current.primaryTouch.position.ReadValue();
            pressed = true;
        }
        else if (Mouse.current != null &&
                 Mouse.current.leftButton.wasPressedThisFrame)
        {
            screenPosition = Mouse.current.position.ReadValue();
            pressed = true;
        }
        else
        {
            return;
        }

        if (mainCamera == null || targetCollider == null)
            return;

        Vector2 worldPosition =
            mainCamera.ScreenToWorldPoint(screenPosition);

        if (targetCollider.OverlapPoint(worldPosition))
        {
            SendMessage(
                "OnMouseDown",
                SendMessageOptions.DontRequireReceiver
            );
        }
    }
}
