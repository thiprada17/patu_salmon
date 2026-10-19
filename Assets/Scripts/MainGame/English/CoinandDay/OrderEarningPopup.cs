
using System.Collections;
using TMPro;
using UnityEngine;

public class OrderEarningPopup : MonoBehaviour
{
    [Header("Popup Panel")]
    [SerializeField] private GameObject panel;

    [Header("Text References")]
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private TMP_Text tipText;
    [SerializeField] private TMP_Text penaltyText;

    [Header("Settings")]
    [SerializeField] private float displayDuration = 3f;

    private Coroutine hideCoroutine;

    private void Awake()
    {
        if (panel != null)
            panel.SetActive(false);
    }

    public void Show(int price, int tip, int penalty)
    {
        // This component must live on an active GameObject.
        if (!isActiveAndEnabled)
        {
            Debug.LogWarning(
                "OrderEarningPopup is inactive. " +
                "Move this component to an active GameObject."
            );
            return;
        }

        if (priceText != null)
            priceText.text = $"Price: {price}";

        if (tipText != null)
            tipText.text = $"Tip: {tip}";

        if (penaltyText != null)
            penaltyText.text = $"Pen: -{penalty}";

        if (panel != null)
            panel.SetActive(true);
        else
            Debug.LogWarning("OrderEarningPopup: Panel is not assigned.");

        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }

        hideCoroutine = StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(displayDuration);

        if (panel != null)
            panel.SetActive(false);

        hideCoroutine = null;
    }

    private void OnDisable()
    {
        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
            hideCoroutine = null;
        }
    }
}
