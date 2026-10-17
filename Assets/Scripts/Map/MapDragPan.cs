
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class MapDragPan : MonoBehaviour
{
    public bool IsDragging => dragStarted;

    [Header("Bounds")]
    [SerializeField] private SpriteRenderer mapBoundsReference;
    [SerializeField] private bool clampToBounds = true;

    [Header("Drag Feel")]
    [SerializeField] private float dragThreshold = 1f;

    [Header("Zoom")]
    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private float minZoom = 3f;
    [SerializeField] private float maxZoom = 10f;

    private Camera cam;
    private Vector3 dragOrigin;
    private Vector2 pointerDownScreenPos;
    private bool isDragging;
    private bool dragStarted;

    private void Awake()
    {
        cam = GetComponent<Camera>();

        if (cam == null)
        {
            GameLogger.Error("MapDragPan", "Camera component not found!");
        }
    }

    private void Update()
    {
        if (cam == null)
            return;

        // Touch input
        if (Touchscreen.current != null &&
            Touchscreen.current.primaryTouch.press.isPressed)
        {
            var touch0 = Touchscreen.current.primaryTouch;
            Vector2 touchPosition = touch0.position.ReadValue();

            if (IsTouchOverUI())
            {
                EndDrag();
                return;
            }

            // Two-finger pinch zoom
            if (TryGetPinch(out Vector2 midpoint, out float difference))
            {
                isDragging = false;
                dragStarted = false;

                if (Mathf.Abs(difference) > 0.01f)
                    Zoom(difference * zoomSpeed * 0.01f, midpoint);

                return;
            }

            if (touch0.press.wasPressedThisFrame)
                BeginDrag(touchPosition);
            else
                ContinueDrag(touchPosition);

            return;
        }

        // Mouse input
        if (Mouse.current != null)
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();

            if (EventSystem.current != null &&
                EventSystem.current.IsPointerOverGameObject())
            {
                if (Mouse.current.leftButton.wasReleasedThisFrame)
                    EndDrag();

                return;
            }

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                BeginDrag(mousePosition);
            }
            else if (Mouse.current.leftButton.isPressed && isDragging)
            {
                ContinueDrag(mousePosition);
            }
            else if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                EndDrag();
            }

            float scroll = Mouse.current.scroll.ReadValue().y;

            if (Mathf.Abs(scroll) > 0.01f)
                Zoom(scroll * zoomSpeed * 0.01f, mousePosition);
        }
    }

    private bool IsTouchOverUI()
    {
        if (EventSystem.current == null ||
            Touchscreen.current == null)
            return false;

        int touchId = Touchscreen.current.primaryTouch.touchId.ReadValue();

        return EventSystem.current.IsPointerOverGameObject(touchId);
    }

    private bool TryGetPinch(out Vector2 midpoint, out float difference)
    {
        midpoint = Vector2.zero;
        difference = 0f;

        if (Touchscreen.current == null)
            return false;

        var touches = Touchscreen.current.touches;

        if (touches.Count < 2 ||
            !touches[0].press.isPressed ||
            !touches[1].press.isPressed)
            return false;

        Vector2 pos0 = touches[0].position.ReadValue();
        Vector2 pos1 = touches[1].position.ReadValue();

        Vector2 prev0 = pos0 - touches[0].delta.ReadValue();
        Vector2 prev1 = pos1 - touches[1].delta.ReadValue();

        float previousDistance = Vector2.Distance(prev0, prev1);
        float currentDistance = Vector2.Distance(pos0, pos1);

        difference = currentDistance - previousDistance;
        midpoint = (pos0 + pos1) * 0.5f;

        return true;
    }

    private void BeginDrag(Vector2 screenPos)
    {
        pointerDownScreenPos = screenPos;
        dragOrigin = cam.ScreenToWorldPoint(
            new Vector3(screenPos.x, screenPos.y, -cam.transform.position.z)
        );

        isDragging = true;
        dragStarted = false;
    }

    private void ContinueDrag(Vector2 screenPos)
    {
        if (!isDragging)
            return;

        float screenDist = Vector2.Distance(pointerDownScreenPos, screenPos);

        if (!dragStarted)
        {
            if (screenDist < dragThreshold)
                return;

            dragStarted = true;
        }

        Vector3 currentWorld = cam.ScreenToWorldPoint(
            new Vector3(screenPos.x, screenPos.y, -cam.transform.position.z)
        );

        Vector3 delta = dragOrigin - currentWorld;

        MoveCamera(delta);

        dragOrigin = cam.ScreenToWorldPoint(
            new Vector3(screenPos.x, screenPos.y, -cam.transform.position.z)
        );
    }

    private void EndDrag()
    {
        isDragging = false;
        dragStarted = false;
    }

    private void MoveCamera(Vector3 delta)
    {
        Vector3 newPos = transform.position + delta;

        if (clampToBounds && mapBoundsReference != null)
            newPos = ClampToMapBounds(newPos);

        transform.position = newPos;
    }

    private void Zoom(float amount, Vector2 screenPosition)
    {
        float oldZoom = cam.orthographicSize;

        Vector3 screenPoint = new Vector3(
            screenPosition.x,
            screenPosition.y,
            -cam.transform.position.z
        );

        Vector3 worldBefore = cam.ScreenToWorldPoint(screenPoint);

        float newZoom = Mathf.Clamp(
            oldZoom - amount,
            minZoom,
            maxZoom
        );

        if (mapBoundsReference != null)
        {
            Bounds bounds = mapBoundsReference.bounds;

            float maxZoomByHeight = bounds.size.y * 0.5f;
            float maxZoomByWidth = bounds.size.x / (2f * cam.aspect);

            float maxZoomByMap = Mathf.Min(
                maxZoomByHeight,
                maxZoomByWidth
            );

            newZoom = Mathf.Min(newZoom, maxZoomByMap);
            newZoom = Mathf.Max(newZoom, minZoom);
        }

        if (Mathf.Approximately(oldZoom, newZoom))
            return;

        cam.orthographicSize = newZoom;

        Vector3 worldAfter = cam.ScreenToWorldPoint(screenPoint);
        transform.position += worldBefore - worldAfter;

        if (clampToBounds && mapBoundsReference != null)
            transform.position = ClampToMapBounds(transform.position);
    }

    private Vector3 ClampToMapBounds(Vector3 pos)
    {
        Bounds bounds = mapBoundsReference.bounds;

        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        float minX = bounds.min.x + halfWidth;
        float maxX = bounds.max.x - halfWidth;
        float minY = bounds.min.y + halfHeight;
        float maxY = bounds.max.y - halfHeight;

        float x = minX <= maxX
            ? Mathf.Clamp(pos.x, minX, maxX)
            : bounds.center.x;

        float y = minY <= maxY
            ? Mathf.Clamp(pos.y, minY, maxY)
            : bounds.center.y;

        return new Vector3(x, y, pos.z);
    }
}
