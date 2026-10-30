using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>Controls the policy popup and its content tabs.</summary>
public sealed class PolicyPopup : MonoBehaviour
{
    public static bool IsOpen { get; private set; }

    [SerializeField] private GameObject policyPanel;
    [SerializeField] private GameObject policyTextPanel;
    [SerializeField] private GameObject userTextPanel;
    [SerializeField] private GameObject termsTextPanel;
    [SerializeField] private TMP_Text policyContentText;
    [SerializeField] private TMP_Text userContentText;
    [SerializeField] private TMP_Text termsContentText;
    [SerializeField] private Image policyTabImage;
    [SerializeField] private Image userAgreementTabImage;
    [SerializeField] private Image termsTabImage;
    [SerializeField] private Button policyTabButton;
    [SerializeField] private Button userAgreementTabButton;
    [SerializeField] private Button termsTabButton;
    [SerializeField, TextArea(8, 30)] private string privacyPolicyContent;
    [SerializeField, TextArea(8, 30)] private string userAgreementContent;
    [SerializeField, TextArea(8, 30)] private string termsConditionsContent;

    private static readonly Color SelectedTabColor = new Color(1f, 0.910512f, 0.7528302f, 1f);
    private static readonly Color UnselectedTabColor = Color.white;

    private float previousTimeScale = 1f;

    private void Start()
    {
        IsOpen = false;
        if (policyPanel != null)
            policyPanel.SetActive(false);

        PrepareScrollableText(policyTextPanel, policyContentText);
        PrepareScrollableText(userTextPanel, userContentText);
        if (termsContentText == null && termsTextPanel != null)
            termsContentText = termsTextPanel.GetComponentInChildren<TMP_Text>(true);
        PrepareScrollableText(termsTextPanel, termsContentText);
        BindTabButton(policyTabButton, ShowPolicy);
        BindTabButton(userAgreementTabButton, ShowUserAgreement);
        BindTabButton(termsTabButton, ShowTermsConditions);
        ShowPolicy();
    }

    private static void BindTabButton(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button != null)
            button.onClick.AddListener(action);
    }

    public void ShowPopup()
    {
        if (policyPanel == null)
        {
            Debug.LogError("PolicyPopup: PolicyPanel is not assigned.");
            return;
        }

        policyPanel.SetActive(true);
        policyPanel.transform.SetAsLastSibling();
        if (!IsOpen)
            previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        IsOpen = true;
        ShowPolicy();
    }

    public void ShowPolicy()
    {
        if (policyContentText != null && !string.IsNullOrWhiteSpace(privacyPolicyContent))
            policyContentText.text = privacyPolicyContent;
        if (policyTextPanel != null)
            policyTextPanel.SetActive(true);
        if (userTextPanel != null)
            userTextPanel.SetActive(false);
        if (termsTextPanel != null)
            termsTextPanel.SetActive(false);
        SetSelectedTab(policyTabImage);
        ScrollToTop(policyTextPanel, policyContentText);
    }

    public void ShowUserAgreement()
    {
        if (userContentText != null && !string.IsNullOrWhiteSpace(userAgreementContent))
            userContentText.text = userAgreementContent;
        if (policyTextPanel != null)
            policyTextPanel.SetActive(false);
        if (termsTextPanel != null)
            termsTextPanel.SetActive(false);
        if (userTextPanel != null)
            userTextPanel.SetActive(true);
        SetSelectedTab(userAgreementTabImage);
        ScrollToTop(userTextPanel, userContentText);
    }

    public void ShowTermsConditions()
    {
        if (termsContentText != null && !string.IsNullOrWhiteSpace(termsConditionsContent))
            termsContentText.text = termsConditionsContent;
        if (policyTextPanel != null)
            policyTextPanel.SetActive(false);
        if (userTextPanel != null)
            userTextPanel.SetActive(false);
        if (termsTextPanel != null)
            termsTextPanel.SetActive(true);
        SetSelectedTab(termsTabImage);
        ScrollToTop(termsTextPanel, termsContentText);
    }

    private void SetSelectedTab(Image selected)
    {
        SetTabColor(policyTabImage, selected);
        SetTabColor(userAgreementTabImage, selected);
        SetTabColor(termsTabImage, selected);
    }

    private static void SetTabColor(Image tab, Image selected)
    {
        if (tab != null)
            tab.color = tab == selected ? SelectedTabColor : UnselectedTabColor;
    }

    private static void PrepareScrollableText(GameObject panelObject, TMP_Text text)
    {
        if (panelObject == null || text == null)
            return;

        RectTransform viewport = panelObject.GetComponent<RectTransform>();
        if (panelObject.GetComponent<RectMask2D>() == null)
            panelObject.AddComponent<RectMask2D>();

        ScrollRect scroll = panelObject.GetComponent<ScrollRect>();
        if (scroll == null)
            scroll = panelObject.AddComponent<ScrollRect>();

        scroll.viewport = viewport;
        scroll.content = text.rectTransform;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.inertia = true;
        scroll.scrollSensitivity = 35f;

        RectTransform content = text.rectTransform;
        content.anchorMin = new Vector2(0.5f, 1f);
        content.anchorMax = new Vector2(0.5f, 1f);
        content.pivot = new Vector2(0.5f, 1f);
        content.anchoredPosition = new Vector2(0f, -20f);
        content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, viewport.rect.width - 65f);
        ResizeContentToText(viewport, content, text);

        Scrollbar scrollbar = CreateScrollbar(panelObject.transform);
        scroll.verticalScrollbar = scrollbar;
        scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
        scroll.verticalScrollbarSpacing = -18f;
        text.enableWordWrapping = true;
        text.overflowMode = TextOverflowModes.Overflow;
        text.ForceMeshUpdate(true);
        ResizeContentToText(viewport, content, text);
    }

    private static void ResizeContentToText(RectTransform viewport, RectTransform content, TMP_Text text)
    {
        text.ForceMeshUpdate();
        float neededHeight = text.preferredHeight + 40f;
        content.SetSizeWithCurrentAnchors(
            RectTransform.Axis.Vertical,
            Mathf.Max(viewport.rect.height, neededHeight));
    }

    private static Scrollbar CreateScrollbar(Transform parent)
    {
        GameObject barObject = new GameObject(
            "Policy Scrollbar",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image),
            typeof(Scrollbar));
        barObject.transform.SetParent(parent, false);

        RectTransform barRect = barObject.GetComponent<RectTransform>();
        barRect.anchorMin = new Vector2(1f, 0f);
        barRect.anchorMax = new Vector2(1f, 1f);
        barRect.pivot = new Vector2(1f, 0.5f);
        barRect.anchoredPosition = new Vector2(-16f, 0f);
        barRect.sizeDelta = new Vector2(20f, -50f);

        Image background = barObject.GetComponent<Image>();
        background.color = new Color(0.35f, 0.28f, 0.23f, 0.32f);

        GameObject handleObject = new GameObject(
            "Handle",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));
        handleObject.transform.SetParent(barObject.transform, false);
        RectTransform handleRect = handleObject.GetComponent<RectTransform>();
        handleRect.anchorMin = Vector2.zero;
        handleRect.anchorMax = Vector2.one;
        handleRect.offsetMin = new Vector2(2f, 2f);
        handleRect.offsetMax = new Vector2(-2f, -2f);

        Image handle = handleObject.GetComponent<Image>();
        handle.color = new Color(0.42f, 0.31f, 0.24f, 0.9f);

        Scrollbar scrollbar = barObject.GetComponent<Scrollbar>();
        scrollbar.handleRect = handleRect;
        scrollbar.targetGraphic = handle;
        scrollbar.direction = Scrollbar.Direction.BottomToTop;
        scrollbar.size = 0.2f;
        scrollbar.value = 1f;
        return scrollbar;
    }

    private static void ScrollToTop(GameObject panelObject, TMP_Text text)
    {
        if (panelObject == null)
            return;

        ScrollRect scroll = panelObject.GetComponent<ScrollRect>();
        if (scroll != null)
        {
            Canvas.ForceUpdateCanvases();
            if (text != null && scroll.content != null)
                ResizeContentToText(scroll.viewport, scroll.content, text);
            scroll.verticalNormalizedPosition = 1f;
        }
    }

    public void ClosePopup()
    {
        if (policyPanel != null)
            policyPanel.SetActive(false);
        Time.timeScale = previousTimeScale;
        IsOpen = false;
    }
}
