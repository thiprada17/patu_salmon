using UnityEngine;

public class MapInputController : MonoBehaviour
{
    public static MapInputController Instance { get; private set; }

    [SerializeField] private DialogueController dialogueController;

    private bool congratulationsPopupOpen;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning(
                "[MapInputController] Duplicate instance found."
            );
            return;
        }

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
        // Block map input while the congratulations popup is open.
        if (congratulationsPopupOpen)
            return false;

        // Preserve the existing dialogue tutorial input lock.
        if (dialogueController == null)
        {
            Debug.LogWarning(
                "[MapInputController] DialogueController not found."
            );
            return false;
        }

        return !dialogueController.IsTutorialInputLocked();
    }

    public void SetCongratulationsPopupOpen(bool isOpen)
    {
        congratulationsPopupOpen = isOpen;

        Debug.Log(
            "[MapInputController] Congratulations popup lock: " + isOpen
        );
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}