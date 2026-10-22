
using UnityEngine;

public class MapInputController : MonoBehaviour
{
    public static MapInputController Instance { get; private set; }

    [SerializeField] private DialogueController dialogueController;

    private void Awake()
    {
        Instance = this;

        if (dialogueController == null)
        {
            dialogueController =
                FindFirstObjectByType<DialogueController>(
                    FindObjectsInactive.Include
                );
        }
    }

    public bool CanClickMap()
    {
        // Allow map clicks only when the dialogue sequence is not active.
        if (dialogueController == null)
        {
            Debug.LogWarning(
                "MapInputController: MapDialogueController not found."
            );

            // Fail safely: don't allow map clicks if the controller is missing.
            return false;
        }

        return !dialogueController.IsTutorialInputLocked();
    }
}
