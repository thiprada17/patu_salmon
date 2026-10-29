using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PhoneViewport : MonoBehaviour
{
    [Header("Target phone aspect ratio")]
    [SerializeField] private float phoneWidth = 9f;
    [SerializeField] private float phoneHeight = 19.5f;

    private Camera targetCamera;
    private Rect lastPixelRect;

    private void Awake()
    {
        targetCamera = GetComponent<Camera>();
        ApplyViewport();
    }

    private void OnEnable()
    {
        ApplyViewport();
    }

    private void LateUpdate()
    {
        ApplyViewport();
    }

    private void ApplyViewport()
    {
        if (targetCamera == null)
            targetCamera = GetComponent<Camera>();

        if (Screen.width <= 0 || Screen.height <= 0)
            return;

        float targetAspect = phoneWidth / phoneHeight;
        float screenAspect = (float)Screen.width / Screen.height;

        float width = 1f;
        float height = 1f;
        float x = 0f;
        float y = 0f;

        if (screenAspect > targetAspect)
        {
            width = targetAspect / screenAspect;
            x = (1f - width) * 0.5f;
        }
        else
        {
            height = screenAspect / targetAspect;
            y = (1f - height) * 0.5f;
        }

        Rect normalizedRect = new Rect(x, y, width, height);

        if (targetCamera.rect != normalizedRect)
            targetCamera.rect = normalizedRect;

        targetCamera.clearFlags = CameraClearFlags.SolidColor;
        targetCamera.backgroundColor = Color.black;
    }
}