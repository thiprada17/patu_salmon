using UnityEngine;

[CreateAssetMenu(
    fileName = "CustomerDialogue",
    menuName = "Cafe/Customer Dialogue"
)]
public class CustomerDialogueData : ScriptableObject
{
    [TextArea(2, 3)]
    public string[] dialogues;
}