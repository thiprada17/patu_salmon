using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CustomerOrderBubble : MonoBehaviour
{
    [Header("Bubble")]
    [SerializeField] private GameObject bubbleObject;
    [SerializeField] private TMP_Text orderText;

    [SerializeField] private Image orderImage;
    private RectTransform orderTextRect;

    [Header("Time Bar")]
    [SerializeField] private CustomerTimeBar timeBar;

    private void Awake()
    {
        HideDialogue();

        if (orderText != null)
        {
            orderTextRect = orderText.rectTransform;
            orderTextRect.anchorMin = orderTextRect.anchorMax =
                new Vector2(0.5f, 0.5f);
            orderTextRect.anchoredPosition = new Vector2(10.9f, 72f);
            orderTextRect.sizeDelta = new Vector2(363.6f, 71f);
            Vector3 textPosition = orderTextRect.localPosition;
            textPosition.z = 0f;
            orderTextRect.localPosition = textPosition;
            if (orderImage == null)
            {
                orderImage = FindMenuImage();
            }

            DisableOtherMenuImages();
            if (orderImage == null)
            {
                CreateOrderImage();
            }
            else
            {
                orderImage.preserveAspect = true;
                orderImage.raycastTarget = false;
                orderImage.enabled = false;
            }
        }

        if (timeBar == null)
        {
            timeBar =
                GetComponentInChildren<CustomerTimeBar>(true);
        }

        if (timeBar != null)
        {
            timeBar.Hide();
        }
    }

    public void ShowDialogue(string dialogue)
    {
        if (bubbleObject != null)
        {
            bubbleObject.SetActive(true);
        }

        if (orderText != null)
        {
            orderText.gameObject.SetActive(true);
            orderText.enabled = true;
            orderText.text = dialogue;
        }

        if (orderImage != null)
        {
            orderImage.enabled = false;
            orderImage.sprite = null;
        }
    }

    public void ShowOrder(Sprite orderSprite, string menuName)
    {
        if (bubbleObject != null)
        {
            bubbleObject.SetActive(true);
        }

        if (orderText == null)
        {
            return;
        }

        orderText.gameObject.SetActive(true);
        orderText.enabled = true;
        orderText.text = menuName;

        if (orderSprite == null)
        {
            return;
        }

        if (orderImage == null)
        {
            CreateOrderImage();
        }

        orderImage.sprite = orderSprite;
        orderImage.enabled = true;
        RectTransform imageRect = orderImage.rectTransform;
        Vector3 imagePosition = imageRect.localPosition;
        imagePosition.z = 0f;
        imageRect.localPosition = imagePosition;
        orderImage.transform.SetAsFirstSibling();
    }

    private void CreateOrderImage()
    {
        if (orderTextRect == null)
        {
            return;
        }

        GameObject imageObject = new GameObject(
            "menu",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );
        RectTransform imageRect = imageObject.GetComponent<RectTransform>();
        imageRect.SetParent(orderTextRect.parent, false);
        imageRect.anchorMin = imageRect.anchorMax = new Vector2(0.5f, 0.5f);
        imageRect.pivot = new Vector2(0.5f, 0.5f);
        imageRect.anchoredPosition = new Vector2(10.9f, -67.25f);
        imageRect.sizeDelta = new Vector2(104.7f, 207.5f);

        orderImage = imageObject.GetComponent<Image>();
        orderImage.preserveAspect = true;
        orderImage.raycastTarget = false;
        orderImage.enabled = false;

    }

    private Image FindMenuImage()
    {
        Transform[] children = GetComponentsInChildren<Transform>(true);
        foreach (Transform child in children)
        {
            if (string.Equals(child.name, "menu", System.StringComparison.OrdinalIgnoreCase))
            {
                Image image = child.GetComponent<Image>();
                if (image != null)
                {
                    return image;
                }
            }
        }

        return null;
    }

    private void DisableOtherMenuImages()
    {
        Transform[] children = GetComponentsInChildren<Transform>(true);
        foreach (Transform child in children)
        {
            if (!string.Equals(child.name, "menu", System.StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            Image image = child.GetComponent<Image>();
            if (image != null && image != orderImage)
            {
                image.enabled = false;
            }
        }
    }

    public void HideDialogue()
    {
        if (bubbleObject != null)
        {
            bubbleObject.SetActive(false);
        }

        if (orderText != null)
        {
            orderText.enabled = false;
            orderText.gameObject.SetActive(false);
        }

        if (orderImage != null)
        {
            orderImage.enabled = false;
            orderImage.sprite = null;
        }
    }

    public void ShowTimeBar()
    {
        if (timeBar != null)
        {
            timeBar.Show();
        }
    }

    public void UpdateTimeBar(
        float currentTime,
        float maxTime)
    {
        if (timeBar == null)
            return;

        if (maxTime <= 0f)
        {
            timeBar.SetFillAmount(0f);
            return;
        }

        float remaining =
            1f - Mathf.Clamp01(
                currentTime / maxTime
            );

        timeBar.SetFillAmount(remaining);
    }

    public void SetTimeBarAngry()
    {
        if (timeBar != null)
        {
            timeBar.SetAngry();
        }
    }

    public void HideTimeBar()
    {
        if (timeBar != null)
        {
            timeBar.Hide();
        }
    }
}
