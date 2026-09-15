using UnityEngine;

public class MapDragPan : MonoBehaviour
{
    public bool IsDragging => dragStarted; //เก็บสถานะ ลากอยู่มั้ย 

    [Header("Bounds")]
    [SerializeField]
    private SpriteRenderer mapBoundsReference; // แผนที่ท้ังแผ่น 

    [SerializeField]
    private bool clampToBounds = true; //ลากออกนอกแผนที่ป่าว 

    [Header("Drag Feel")]
    [SerializeField]
    // ฟีลระยะการลาก ลากไกลแค่ไหนถึงนับเป็นการลาก 
    private float dragThreshold = 1f; // หน่วย world space 

    [Header("Zoom")]
    [SerializeField]
    private float zoomSpeed = 2f;

    [SerializeField]
    private float minZoom = 3f;

    [SerializeField]
    private float maxZoom = 10f;

    // จำตำแหน่งทุกสิ่ง 
    private Camera cam;
    private Vector3 dragOrigin; // ตน. world space ล่าสุด 
    private Vector3 pointerDownScreenPos; // ตอนกดนิ้วอยู่ไหน 
    private bool isDragging = false; // ลากยุมั้ย 
    private bool dragStarted = false; // เริ่มลากจิงรึยัง วัดจากไอ dragThreshold  

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void Update()
    {
        // เช็คว่าเป็นนิ้ว หรือเมาส์ 
        if (Input.touchCount > 0)
        {
            if (Input.touchCount >= 2) // 2 นิ้ว
            {
                isDragging = false;
                dragStarted = false;
                HandlePinchZoom();
            }
            else
            {
                HandleTouch(Input.GetTouch(0));
            }

            return;
        }

        HandleMouse();
    }

    private void HandleTouch(Touch touch)
    {
        switch (touch.phase)
        {
            case TouchPhase.Began:
                BeginDrag(touch.position);
                break;

            case TouchPhase.Moved:
                ContinueDrag(touch.position);
                break;

            case TouchPhase.Ended:
            case TouchPhase.Canceled:
                EndDrag();
                break;
        }
    }

    private void HandleMouse()
    {
        if (Input.GetMouseButtonDown(0))
        {
            BeginDrag(Input.mousePosition);
        }
        else if (Input.GetMouseButton(0) && isDragging)
        {
            ContinueDrag(Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            EndDrag();
        }

        float scroll = Input.mouseScrollDelta.y;

        if (Mathf.Abs(scroll) > 0.01f)
        {
            Zoom(scroll * zoomSpeed, Input.mousePosition);
        }
    }

    private void BeginDrag(Vector3 screenPos)
    {
        pointerDownScreenPos = screenPos;
        dragOrigin = cam.ScreenToWorldPoint(screenPos);
        isDragging = true;
        dragStarted = false;
    }

    private void ContinueDrag(Vector3 screenPos)
    {
        if (!isDragging)
            return;

        // เช็คว่าขยับเกิน threshold รึยัง ถึงจะเริ่มถือว่าเป็นการลาก กันไม่ให้แค่กดเบาๆ ตรงปุ่มประเทศแล้วโดนตีความเป็นลาก 
        if (!dragStarted)
        {
            float screenDist = Vector3.Distance(pointerDownScreenPos, screenPos);
            if (screenDist < dragThreshold) // ยังไม่ขยับพอที่จะเช็ค world dist 
                return;

            dragStarted = true;
        }

        Vector3 currentWorld = cam.ScreenToWorldPoint(screenPos);
        Vector3 delta = dragOrigin - currentWorld;

        MoveCamera(delta);

        dragOrigin = cam.ScreenToWorldPoint(screenPos); //อัพเดทจุดอ้างอิงใหม่ทุกครั้ง 
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
        {
            newPos = ClampToMapBounds(newPos);
        }

        transform.position = newPos;
    }

    private void HandlePinchZoom()
    {
        Touch touch0 = Input.GetTouch(0);
        Touch touch1 = Input.GetTouch(1);

        Vector2 previousPos0 = touch0.position - touch0.deltaPosition;
        Vector2 previousPos1 = touch1.position - touch1.deltaPosition;

        float previousDistance = Vector2.Distance(previousPos0, previousPos1);
        float currentDistance = Vector2.Distance(touch0.position, touch1.position);

        float difference = currentDistance - previousDistance;

        if (Mathf.Abs(difference) < 0.01f)
            return;

        Vector2 midpoint = (touch0.position + touch1.position) / 2f;

        Zoom(difference * zoomSpeed * 0.01f, midpoint);
    }

    private void Zoom(float amount, Vector3 screenPosition)
    {
        float oldZoom = cam.orthographicSize;

        Vector3 worldBefore = cam.ScreenToWorldPoint(screenPosition);

        float newZoom = oldZoom - amount;

        if (mapBoundsReference != null)
        {
            Bounds mapBounds = mapBoundsReference.bounds;

            float maxZoomByHeight = mapBounds.size.y * 0.5f;
            float maxZoomByWidth = mapBounds.size.x / (2f * cam.aspect);

            float maxZoomByMap = Mathf.Min(maxZoomByHeight, maxZoomByWidth);

            newZoom = Mathf.Min(newZoom, maxZoomByMap);
        }

        newZoom = Mathf.Clamp(newZoom, minZoom, maxZoom);

        if (Mathf.Approximately(oldZoom, newZoom))
            return;

        cam.orthographicSize = newZoom;

        Vector3 worldAfter = cam.ScreenToWorldPoint(screenPosition);
        Vector3 offset = worldBefore - worldAfter;

        transform.position += offset;

        if (clampToBounds && mapBoundsReference != null)
        {
            transform.position = ClampToMapBounds(transform.position);
        }
    }

    // clamp logic (กันลากทะลุขอบ) 
    private Vector3 ClampToMapBounds(Vector3 pos)
    {
        Bounds mapBounds = mapBoundsReference.bounds;

        float camHalfHeight = cam.orthographicSize;
        float camHalfWidth = camHalfHeight * cam.aspect;

        float minX = mapBounds.min.x + camHalfWidth;
        float maxX = mapBounds.max.x - camHalfWidth;
        float minY = mapBounds.min.y + camHalfHeight;
        float maxY = mapBounds.max.y - camHalfHeight;

        // ถ้าแผนที่เล็กกว่าจอ (min > max) ให้ fix อยู่ตรงกลางแทนการ clamp กลับไปกลับมา 
        float x = (minX <= maxX) ? Mathf.Clamp(pos.x, minX, maxX) : mapBounds.center.x;
        float y = (minY <= maxY) ? Mathf.Clamp(pos.y, minY, maxY) : mapBounds.center.y;

        return new Vector3(x, y, pos.z);
    }
}