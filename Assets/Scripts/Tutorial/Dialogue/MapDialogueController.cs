
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

public class MapDialogueController : MonoBehaviour
{
    public enum DialogueEntryType
    {
        Normal,
        Tutorial
    }

    [Serializable]
    public class DialogueSequenceEntry
    {
        [SerializeField] private DialogueEntryType entryType;

        [Tooltip("Index in the existing speakerNames and dialogueLines arrays.")]
        [SerializeField] private int normalDialogueIndex;

        [Tooltip("Tutorial asset to display for this entry.")]
        [SerializeField] private TutorialStepData tutorialStep;

        public DialogueEntryType EntryType => entryType;
        public int NormalDialogueIndex => normalDialogueIndex;
        public TutorialStepData TutorialStep => tutorialStep;
    }

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

    [Header("Normal Dialogue Content")]
    [SerializeField] private string[] speakerNames;

    [TextArea(2, 4)]
    [SerializeField] private string[] dialogueLines;

    [Header("Dialogue Sequence")]
    [SerializeField] private DialogueSequenceEntry[] dialogueSequence;

    [Header("Typewriter Settings")]
    [SerializeField] private float typingSpeed = 0.035f;

    [Header("Auto Settings")]
    [SerializeField] private float autoAdvanceDelay = 2.5f;

    [Header("Tutorial Visuals")]
    [SerializeField] private TutorialVisualController tutorialVisuals;

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
        if (dialogueSequence == null || dialogueSequence.Length == 0)
        {
            Debug.LogWarning(
                "No entries assigned to Dialogue Sequence."
            );
            return;
        }

        currentLine = 0;
        autoMode = false;

        if (tutorialOverlay != null)
            tutorialOverlay.SetActive(true);

        if (dialogueRoot != null)
            dialogueRoot.SetActive(true);

        if (advanceButton != null)
            advanceButton.gameObject.SetActive(true);

        ShowDialogue();
    }

    public void NextDialogue()
    {
        if (dialogueSequence == null ||
            currentLine >= dialogueSequence.Length)
            return;

        // First click reveals the complete current line.
        if (isTyping)
        {
            FinishTyping();
            return;
        }

        StopAutoAdvance();

        // Close the dialogue after the last sequence entry.
        if (currentLine >= dialogueSequence.Length - 1)
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

        if (dialogueSequence == null ||
            currentLine < 0 ||
            currentLine >= dialogueSequence.Length)
            return;

        DialogueSequenceEntry entry = dialogueSequence[currentLine];

        if (entry == null)
        {
            Debug.LogWarning(
                "Dialogue Sequence entry is empty at index " + currentLine
            );
            return;
        }

        string line;
        string speaker;

        if (entry.EntryType == DialogueEntryType.Normal)
        {
            int index = entry.NormalDialogueIndex;

            if (dialogueLines == null ||
                index < 0 ||
                index >= dialogueLines.Length)
            {
                Debug.LogWarning(
                    "Invalid normal dialogue index: " + index
                );
                return;
            }

            line = dialogueLines[index] ?? "";

            speaker = speakerNames != null &&
                      index >= 0 &&
                      index < speakerNames.Length
                ? speakerNames[index] ?? ""
                : "";
        }
        else
        {
            TutorialStepData step = entry.TutorialStep;

            if (step == null)
            {
                Debug.LogWarning(
                    "Missing TutorialStepData at sequence index " +
                    currentLine
                );
                return;
            }

            line = step.Dialogue ?? "";
            speaker = step.SpeakerName ?? "";
        }

        // Show the finger for tutorial dialogue only.
        if (tutorialVisuals != null)
        {
            if (entry.EntryType == DialogueEntryType.Tutorial)
            {
                tutorialVisuals.ShowStep(entry.TutorialStep);
            }
            else
            {
                tutorialVisuals.HideAll();
            }
        }

        if (speakerText != null)
            speakerText.text = speaker;

        if (dialogueText == null)
        {
            Debug.LogWarning("Dialogue Text is not assigned.");
            return;
        }

        dialogueText.text = line;
        dialogueText.maxVisibleCharacters = 0;
        dialogueText.ForceMeshUpdate();

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
                ? "PLAYING"
                : "AUTO";
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

        if (dialogueRoot != null)
            dialogueRoot.SetActive(false);

        if (tutorialOverlay != null)
            tutorialOverlay.SetActive(false);

        if (advanceButton != null)
            advanceButton.gameObject.SetActive(false);

        if (tutorialVisuals != null)
            tutorialVisuals.HideAll();
    }

    private void OnDestroy()
    {
        StopTyping();
        StopAutoAdvance();

        if (advanceButton != null)
            advanceButton.onClick.RemoveListener(NextDialogue);

        if (autoButton != null)
            autoButton.onClick.RemoveListener(ToggleAutoMode);
    }
}
