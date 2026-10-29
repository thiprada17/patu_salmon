
using UnityEngine;
using UnityEngine.UI;

public class PersistentBlackBars : MonoBehaviour
{
    private static PersistentBlackBars instance;

    [Header("Portrait Aspect Ratio")]
    [SerializeField] private float targetWidth = 9f;
    [SerializeField] private float targetHeight = 19.5f;

    private Canvas canvas;
    private RectTransform leftBar;
    private RectTransform rightBar;

    private int lastScreenWidth;
    private int lastScreenHeight;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        CreateBars();
        UpdateBars();
    }

    private void LateUpdate()
    {
        if (Screen.width != lastScreenWidth ||
            Screen.height != lastScreenHeight)
        {
            UpdateBars();
        }
    }

    private void CreateBars()
    {
        canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32767;

        gameObject.AddComponent<CanvasScaler>();
        gameObject.AddComponent<GraphicRaycaster>();

        leftBar = CreateBar("LeftBlackBar");
        rightBar = CreateBar("RightBlackBar");
    }

    private RectTransform CreateBar(string barName)
    {
        GameObject bar = new GameObject(
            barName,
            typeof(RectTransform),
            typeof(Image)
        );

        bar.transform.SetParent(transform, false);

        Image image = bar.GetComponent<Image>();
        image.color = Color.black;
        image.raycastTarget = false;

        return bar.GetComponent<RectTransform>();
    }

    private void UpdateBars()
    {
        if (leftBar == null || rightBar == null)
            return;

        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;

        if (Screen.width <= 0 || Screen.height <= 0)
            return;

        float targetAspect = targetWidth / targetHeight;
        float screenAspect = (float)Screen.width / Screen.height;

        if (screenAspect > targetAspect)
        {
            // Wider screen: black bars on the left and right.
            float barWidth = (1f - targetAspect / screenAspect) / 2f;

            SetBar(leftBar, 0f, 0f, barWidth, 1f);
            SetBar(rightBar, 1f - barWidth, 0f, barWidth, 1f);
        }
        else
        {
            // Narrower screen: black bars at the top and bottom.
            float barHeight = (1f - screenAspect / targetAspect) / 2f;

            SetBar(leftBar, 0f, 1f - barHeight, 1f, barHeight);
            SetBar(rightBar, 0f, 0f, 1f, barHeight);
        }
    }

    private void SetBar(
        RectTransform rect,
        float x,
        float y,
        float width,
        float height)
    {
        rect.anchorMin = new Vector2(x, y);
        rect.anchorMax = new Vector2(x + width, y + height);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
