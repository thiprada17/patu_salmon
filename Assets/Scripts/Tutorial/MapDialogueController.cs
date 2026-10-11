
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class MapDialogueController : MonoBehaviour
{
    [Header("Dialogue UI")]
    [SerializeField] private GameObject tutorialOverlay;
    [SerializeField] private GameObject dialogueRoot;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private TMP_Text speakerText;
    [SerializeField] private Button advanceButton;

    [Header("Auto Button")]
    [SerializeField] private Button autoButton;
    [SerializeField] private TMP_Text autoButtonText;
    [SerializeField] private Image autoButtonImage;
    [SerializeField] private Sprite autoOffSprite;
    [SerializeField] private Sprite autoOnSprite;

    [Header("Dialogue Content")]
    [SerializeField] private string[] speakerNames;

    [TextArea(2, 4)]
    [SerializeField] private string[] dialogueLines;

    [Header("Typewriter Settings")]
    [SerializeField] private float typingSpeed = 0.035f;

    [Header("Auto Settings")]
    [SerializeField] private float autoAdvanceDelay = 2.5f;

    private int currentLine;
    private bool autoMode;
    private bool isTyping;

    private Coroutine typingCoroutine;
    private Coroutine autoCoroutine;

    private void Awake()
    {
        if (advanceButton != null)
            advanceButton.onClick.AddListener(NextDialogue);

        if (autoButton != null)
            autoButton.onClick.AddListener(ToggleAutoMode);

        UpdateAutoButton();
    }


    private void Start()
    {
        if (dialogueLines == null || dialogueLines.Length == 0)
        {
            Debug.LogWarning("No dialogue lines assigned.");
            return;
        }

        currentLine = 0;
        autoMode = false;

        // Show the tutorial overlay at the beginning.
        if (tutorialOverlay != null)
            tutorialOverlay.SetActive(true);

        // Show the dialogue.
        if (dialogueRoot != null)
            dialogueRoot.SetActive(true);

        if (advanceButton != null)
            advanceButton.gameObject.SetActive(true);

        ShowDialogue();
    }


    public void NextDialogue()
    {
        if (dialogueLines == null ||
            currentLine >= dialogueLines.Length)
            return;

        // First click: reveal the entire current line.
        if (isTyping)
        {
            FinishTyping();
            return;
        }

        StopAutoAdvance();

        // Reached the end of the dialogue.
        if (currentLine >= dialogueLines.Length - 1)
        {
            EndDialogue();
            return;
        }

        currentLine++;
        ShowDialogue();
    }

    private void ShowDialogue()
    {
        StopTyping();
        StopAutoAdvance();

        if (dialogueText == null)
            return;

        dialogueText.text = dialogueLines[currentLine];
        dialogueText.maxVisibleCharacters = 0;
        dialogueText.ForceMeshUpdate();

        if (speakerText != null &&
            speakerNames != null &&
            currentLine < speakerNames.Length)
        {
            speakerText.text = speakerNames[currentLine];
        }

        typingCoroutine = StartCoroutine(TypeText());
    }

    private IEnumerator TypeText()
    {
        isTyping = true;

        int totalCharacters = dialogueText.textInfo.characterCount;

        for (int i = 0; i < totalCharacters; i++)
        {
            dialogueText.maxVisibleCharacters = i + 1;

            if (typingSpeed > 0f)
                yield return new WaitForSecondsRealtime(typingSpeed);
            else
                yield return null;
        }

        dialogueText.maxVisibleCharacters = totalCharacters;

        isTyping = false;
        typingCoroutine = null;

        // Start Auto timer only after typing finishes.
        if (autoMode)
            ScheduleAutoAdvance();
    }

    private void FinishTyping()
    {
        StopTyping();

        if (dialogueText != null)
        {
            dialogueText.maxVisibleCharacters =
                dialogueText.textInfo.characterCount;
        }

        isTyping = false;

        if (autoMode)
            ScheduleAutoAdvance();
    }

    private void StopTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        isTyping = false;
    }

    public void ToggleAutoMode()
    {
        autoMode = !autoMode;
        UpdateAutoButton();

        if (!autoMode)
        {
            StopAutoAdvance();
        }
        else if (!isTyping)
        {
            ScheduleAutoAdvance();
        }
    }

    private void UpdateAutoButton()
    {
        if (autoButtonText != null)
        {
            autoButtonText.text = autoMode
                ? "PLAYING" //Auto: on
                : "AUTO"; //Auto: off
        }

        if (autoButtonImage != null)
        {
            autoButtonImage.sprite = autoMode
                ? autoOnSprite
                : autoOffSprite;
        }
    }

    private void ScheduleAutoAdvance()
    {
        StopAutoAdvance();
        autoCoroutine = StartCoroutine(AutoAdvanceRoutine());
    }

    private IEnumerator AutoAdvanceRoutine()
    {
        yield return new WaitForSecondsRealtime(autoAdvanceDelay);

        autoCoroutine = null;

        if (autoMode &&
            dialogueRoot != null &&
            dialogueRoot.activeSelf)
        {
            NextDialogue();
        }
    }

    private void StopAutoAdvance()
    {
        if (autoCoroutine != null)
        {
            StopCoroutine(autoCoroutine);
            autoCoroutine = null;
        }
    }


    private void EndDialogue()
    {
        StopTyping();
        StopAutoAdvance();

        autoMode = false;
        UpdateAutoButton();

        // Hide the dialogue panel.
        if (dialogueRoot != null)
            dialogueRoot.SetActive(false);

        // Hide the tutorial overlay when the dialogue ends.
        if (tutorialOverlay != null)
            tutorialOverlay.SetActive(false);

        // Disable the full-screen advance button.
        if (advanceButton != null)
            advanceButton.gameObject.SetActive(false);
    }


    private void OnDestroy()
    {
        StopTyping();
        StopAutoAdvance();
    }
}
