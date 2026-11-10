using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class Japan : MonoBehaviour
{
    [Header("Popup Objects")]
    [SerializeField] private GameObject japanPopup;
    [SerializeField] private GameObject textJapanPopup;
    [SerializeField] private GameObject backgroundOverlay;

    private Camera cam;
    private Collider2D countryCollider;
    private bool pointerDownOnCountry;
    private Vector2 lastPointerPos;

    private void Awake()
    {
        cam = Camera.main;
        countryCollider = GetComponent<Collider2D>();

        if (countryCollider == null)
            GameLogger.Error("Japan", "Collider2D not found!");
    }

    private void Start()
    {
        SetPopup(false);
    }

    private void Update()
    {
        if (cam == null || countryCollider == null) return;

        var pointer = Pointer.current;
        if (pointer == null) return;

        Vector2 pos = pointer.position.ReadValue();

        if (pointer.press.wasPressedThisFrame)
        {
            lastPointerPos = pos;

            if (IsPointerOverUI())
            {
                pointerDownOnCountry = false;
                return;
            }

            pointerDownOnCountry = IsPointerOnCountry(pos);
        }

        if (pointer.press.isPressed)
            lastPointerPos = pos;

        if (pointer.press.wasReleasedThisFrame)
        {
            bool releasedOnCountry = IsPointerOnCountry(lastPointerPos);

            if (pointerDownOnCountry && releasedOnCountry)
                TryOpenPopup();

            pointerDownOnCountry = false;
        }
    }

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
            return EventSystem.current.IsPointerOverGameObject(
                Touchscreen.current.primaryTouch.touchId.ReadValue());

        return EventSystem.current.IsPointerOverGameObject();
    }

    private bool IsPointerOnCountry(Vector2 screenPosition)
    {
        Vector3 world = cam.ScreenToWorldPoint(
            new Vector3(screenPosition.x, screenPosition.y, -cam.transform.position.z));

        Collider2D hit = Physics2D.OverlapPoint(new Vector2(world.x, world.y));
        return hit == countryCollider;
    }

    private void TryOpenPopup()
    {
        if (MapInputController.Instance == null)
        {
            GameLogger.Warn("Japan", "MapInputController is NULL");
            return;
        }

        if (!MapInputController.Instance.CanClickMap())
        {
            GameLogger.DebugLog("Japan", "CanClickMap returned false");
            return;
        }

        OpenPopup();
    }

    public void OpenPopup() => SetPopup(true);
    public void ClosePopup() => SetPopup(false);

    private void SetPopup(bool active)
    {
        if (japanPopup != null) japanPopup.SetActive(active);
        if (textJapanPopup != null) textJapanPopup.SetActive(active);
        if (backgroundOverlay != null) backgroundOverlay.SetActive(active);
    }
}
