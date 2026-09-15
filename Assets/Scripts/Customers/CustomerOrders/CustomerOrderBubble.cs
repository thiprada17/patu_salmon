using UnityEngine;
using TMPro;

public class CustomerOrderBubble : MonoBehaviour
{
    [Header("Bubble")]
    [SerializeField] private GameObject bubbleObject;
    [SerializeField] private TMP_Text orderText;

    [Header("Time Bar")]
    [SerializeField] private CustomerTimeBar timeBar;

    private void Awake()
    {
        HideDialogue();

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
            orderText.text = dialogue;
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
            orderText.gameObject.SetActive(false);
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