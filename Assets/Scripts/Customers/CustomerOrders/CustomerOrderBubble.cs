using UnityEngine;
using TMPro;

public class CustomerOrderBubble : MonoBehaviour
{
    [Header("Bubble")]
    [SerializeField] private GameObject bubbleObject;
    [SerializeField] private TMP_Text orderText;

    public void ShowDialogue(string dialogue)
    {
        if (bubbleObject != null)
        {
            bubbleObject.SetActive(true);
        }

        if (orderText != null)
        {
            orderText.text = dialogue;
        }
    }

    public void HideDialogue()
    {
        if (bubbleObject != null)
        {
            bubbleObject.SetActive(false);
        }
    }
}