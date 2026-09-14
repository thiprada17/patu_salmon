using UnityEngine;

public class CustomerTimeBar : MonoBehaviour
{
    [Header("Sprite Mask")]
    [SerializeField] private Transform maskTransform;

    [Header("Fill")]
    [SerializeField] private SpriteRenderer fillRenderer;

    [Header("Colors")]
    [SerializeField]
    private Color normalColor =
        new Color32(152, 219, 146, 255);

    [SerializeField]
    private Color angryColor =
        new Color32(219, 146, 146, 255);

    private float originalScaleY;

    private void Awake()
    {
        if (maskTransform != null)
        {
            originalScaleY =
                maskTransform.localScale.y;
        }

        if (fillRenderer != null)
        {
            fillRenderer.color = normalColor;
        }
    }

    public void Show()
    {
        gameObject.SetActive(true);

        if (maskTransform != null)
        {
            Vector3 scale =
                maskTransform.localScale;

            scale.y = originalScaleY;

            maskTransform.localScale = scale;
        }

        if (fillRenderer != null)
        {
            fillRenderer.color = normalColor;
        }
    }

    public void SetFillAmount(float amount)
    {
        amount = Mathf.Clamp01(amount);

        if (maskTransform == null)
            return;

        Vector3 scale =
            maskTransform.localScale;

        scale.y =
            originalScaleY * amount;

        maskTransform.localScale = scale;
    }

    public void SetAngry()
    {
        if (fillRenderer != null)
        {
            fillRenderer.color = angryColor;
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}