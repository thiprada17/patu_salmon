using UnityEngine;
using TMPro;

public class CustomerOrderBubble : MonoBehaviour
{
    [Header("Bubble")]
    [SerializeField] private GameObject bubbleObject;
    [SerializeField] private TMP_Text orderText;

    private void Awake()
    {
        HideDialogue();
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
}