using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PortraitAspectRatio : MonoBehaviour
{
    [Header("Portrait Aspect Ratio")]
    [SerializeField] private float targetWidth = 9f;
    [SerializeField] private float targetHeight = 19.5f;

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;

        UpdateViewport();
    }

    private void Update()
    {
        UpdateViewport();
    }

    private void UpdateViewport()
    {
        if (Screen.width <= 0 || Screen.height <= 0)
            return;

        float targetAspect = targetWidth / targetHeight;
        float screenAspect = (float)Screen.width / Screen.height;

        if (screenAspect > targetAspect)
        {
            // Screen is wider than the target portrait ratio.
            float viewportWidth = targetAspect / screenAspect;

            cam.rect = new Rect(
                (1f - viewportWidth) / 2f,
                0f,
                viewportWidth,
                1f
            );
        }
        else
        {
            // Screen is narrower than the target portrait ratio.
            float viewportHeight = screenAspect / targetAspect;

            cam.rect = new Rect(
                0f,
                (1f - viewportHeight) / 2f,
                1f,
                viewportHeight
            );
        }
    }
}