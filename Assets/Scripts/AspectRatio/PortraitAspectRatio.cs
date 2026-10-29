
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PortraitAspectRatio : MonoBehaviour
{
    [Header("Target Phone Aspect Ratio")]
    [SerializeField] private float targetWidth = 9f;
    [SerializeField] private float targetHeight = 19.5f;

    private Camera cam;
    private int lastWidth;
    private int lastHeight;

    private void Awake()
    {
        cam = GetComponent<Camera>();

        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;

        ApplyViewport();
    }

    private void LateUpdate()
    {
        if (Screen.width != lastWidth ||
            Screen.height != lastHeight)
        {
            ApplyViewport();
        }
    }

    private void ApplyViewport()
    {
        if (cam == null || Screen.width <= 0 || Screen.height <= 0)
            return;

        lastWidth = Screen.width;
        lastHeight = Screen.height;

        float targetAspect = targetWidth / targetHeight;
        float screenAspect = (float)Screen.width / Screen.height;

        if (screenAspect > targetAspect)
        {
            float width = targetAspect / screenAspect;

            cam.rect = new Rect(
                (1f - width) / 2f,
                0f,
                width,
                1f
            );
        }
        else
        {
            float height = screenAspect / targetAspect;

            cam.rect = new Rect(
                0f,
                (1f - height) / 2f,
                1f,
                height
            );
        }
    }
}