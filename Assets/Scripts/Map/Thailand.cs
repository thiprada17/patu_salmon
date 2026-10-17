
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class Thailand : MonoBehaviour
{
    [Header("Popup Objects")]
    [SerializeField] private GameObject thailandPopup;
    [SerializeField] private GameObject textThailandPopup;
    [SerializeField] private GameObject backgroundOverlay;

    private Camera cam;
    private Collider2D countryCollider;
    private bool pointerDownOnCountry;

    private void Awake()
    {
        cam = Camera.main;
        countryCollider = GetComponent<Collider2D>();

        if (countryCollider == null)
        {
            GameLogger.Error("Thailand", "Collider2D not found!");
        }
    }

    private void Start()
    {
        if (thailandPopup != null)
            thailandPopup.SetActive(false);

        if (textThailandPopup != null)
            textThailandPopup.SetActive(false);

        if (backgroundOverlay != null)
            backgroundOverlay.SetActive(false);
    }

    private void Update()
    {
        if (cam == null || countryCollider == null)
            return;

        // Mouse click
        if (Mouse.current != null)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                pointerDownOnCountry = IsPointerOnCountry(
                    Mouse.current.position.ReadValue()
                );
            }

            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                bool releasedOnCountry = IsPointerOnCountry(
                    Mouse.current.position.ReadValue()
                );

                if (pointerDownOnCountry && releasedOnCountry)
                    TryOpenPopup();

                pointerDownOnCountry = false;
            }
        }

        // Touch input
        if (Touchscreen.current != null)
        {
            var touch = Touchscreen.current.primaryTouch;

            if (touch.press.wasPressedThisFrame)
            {
                pointerDownOnCountry = IsPointerOnCountry(
                    touch.position.ReadValue()
                );
            }

            if (touch.press.wasReleasedThisFrame)
            {
                bool releasedOnCountry = IsPointerOnCountry(
                    touch.position.ReadValue()
                );

                if (pointerDownOnCountry && releasedOnCountry)
                    TryOpenPopup();

                pointerDownOnCountry = false;
            }
        }
    }

    private bool IsPointerOnCountry(Vector2 screenPosition)
    {
        Vector3 worldPosition = cam.ScreenToWorldPoint(
            new Vector3(
                screenPosition.x,
                screenPosition.y,
                -cam.transform.position.z
            )
        );

        Collider2D hit = Physics2D.OverlapPoint(
            new Vector2(worldPosition.x, worldPosition.y)
        );

        return hit == countryCollider;
    }

    private void TryOpenPopup()
    {
        // ตรวจว่ามี MapInputController
        if (MapInputController.Instance == null)
        {
            GameLogger.Warn("Thailand", "MapInputController is NULL");
            return;
        }

        // ป้องกันการเปิด Popup ระหว่าง Tutorial ล็อกการคลิก
        if (!MapInputController.Instance.CanClickMap())
        {
            GameLogger.DebugLog(
                "Thailand",
                "CanClickMap returned false"
            );
            return;
        }

        OpenPopup();
    }

    public void OpenPopup()
    {
        GameLogger.DebugLog("Thailand", "Opening Thailand popup");

        if (thailandPopup != null)
            thailandPopup.SetActive(true);

        if (textThailandPopup != null)
            textThailandPopup.SetActive(true);

        if (backgroundOverlay != null)
            backgroundOverlay.SetActive(true);
    }

    public void ClosePopup()
    {
        if (thailandPopup != null)
            thailandPopup.SetActive(false);

        if (textThailandPopup != null)
            textThailandPopup.SetActive(false);

        if (backgroundOverlay != null)
            backgroundOverlay.SetActive(false);
    }
}
