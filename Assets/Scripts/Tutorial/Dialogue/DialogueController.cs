
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;
using UnityEngine.Events;

public class DialogueController : MonoBehaviour
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

        [Tooltip("Index in the existing dialogueLines array.")]
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
    [SerializeField] private RectTransform dialogueBox;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Button advanceButton;

    [Header("Speaker Model")]
    [SerializeField] private GameObject speakerModel;

    [Header("Auto Button")]
    [SerializeField] private Button autoButton;
    [SerializeField] private TMP_Text autoButtonText;
    [SerializeField] private Image autoButtonImage;
    [SerializeField] private Sprite autoOffSprite;
    [SerializeField] private Sprite autoOnSprite;

    [Header("Normal Dialogue Content")]
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

    [Header("Temporary UI Highlights")]
    [SerializeField] private TempCanvasController tempCanvas;

    [Header("Tutorial Character")]
    [SerializeField] private GameObject mysteriousFox;

    [Header("Tutorial Pause")]
    [Tooltip("Pause gameplay time while this dialogue is active.")]
    [SerializeField] private bool pauseGameDuringDialogue = true;

    [Tooltip("Keep gameplay paused and counter interactions locked after this dialogue ends. Enable this in Main_Game when TutorialVDO opens afterward.")]
    [SerializeField] private bool keepGameplayLockedAfterDialogue = false;

    [Header("Dialogue End Event")]
    [SerializeField] private UnityEvent onDialogueEnded;

    private int currentLine;
    private bool autoMode;
    private bool isTyping;
    private bool dialogueStarted;
    private bool isTutorial;

    private Vector2 originalDialoguePosition;
    private bool originalPositionSaved;

    private Coroutine typingCoroutine;
    private Coroutine autoCoroutine;

    private string PlayerUsername;

    private bool wasGamePausedByDialogue;
    private float previousTimeScale = 1f;

    private void Awake()
    {
        if (dialogueBox != null)
        {
            originalDialoguePosition = dialogueBox.anchoredPosition;
            originalPositionSaved = true;
        }

        if (advanceButton != null)
            advanceButton.onClick.AddListener(NextDialogue);

        if (autoButton != null)
            autoButton.onClick.AddListener(ToggleAutoMode);

        if (mysteriousFox != null)
            mysteriousFox.SetActive(false);

        UpdateAutoButton();
    }

    private void Start()
    {
        Debug.Log("DialogueController Start() is running!");

        if (dialogueSequence == null || dialogueSequence.Length == 0)
        {
            Debug.LogWarning("No entries assigned to Dialogue Sequence.");
            return;
        }

        currentLine = 0;
        autoMode = false;
        dialogueStarted = true;
        isTutorial = false;

        PlayerUsername = PlayerPrefs.GetString("Username", "Player");

        // Lock counter objects for the entire dialogue.
        GameplayInputLock.Lock();

        // Pause the game if this option is enabled.
        PauseGameForDialogue();

        if (tutorialOverlay != null)
            tutorialOverlay.SetActive(true);

        if (dialogueRoot != null)
            dialogueRoot.SetActive(true);

        if (advanceButton != null)
            advanceButton.gameObject.SetActive(true);

        ShowDialogue();
    }

    // Used by other scripts to check whether dialogue is active.
    public bool IsTutorialInputLocked()
    {
        return dialogueStarted;
    }

    public void NextDialogue()
    {
        if (!dialogueStarted ||
            dialogueSequence == null ||
            currentLine < 0 ||
            currentLine >= dialogueSequence.Length)
        {
            return;
        }

        if (isTyping)
        {
            FinishTyping();
            return;
        }

        StopAutoAdvance();

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
        {
            isTutorial = false;
            HideTutorialObjects();
            return;
        }

        DialogueSequenceEntry entry = dialogueSequence[currentLine];

        if (entry == null)
        {
            isTutorial = false;

            Debug.LogWarning(
                "Dialogue Sequence entry is empty at index " + currentLine
            );

            HideTutorialObjects();
            return;
        }

        isTutorial = entry.EntryType == DialogueEntryType.Tutorial;

        Debug.Log(
            "Dialogue index: " + currentLine +
            ", Entry Type: " + entry.EntryType
        );

        TutorialStepData step = isTutorial ? entry.TutorialStep : null;

        string line;

        if (mysteriousFox != null)
            mysteriousFox.SetActive(true);

        if (isTutorial && step == null)
        {
            Debug.LogWarning(
                "Missing TutorialStepData at sequence index " + currentLine
            );

            HideTutorialObjects();
            return;
        }

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

                HideTutorialObjects();
                return;
            }

            line = dialogueLines[index] ?? "";
            RestoreDialoguePosition();
        }
        else
        {
            line = step.Dialogue ?? "";
            ApplyDialoguePosition(step);
        }

        // Update tutorial visuals and temporary UI highlights.
        if (isTutorial)
        {
            Debug.Log("Tutorial dialogue reached: " + currentLine);
            Debug.Log("Tutorial TargetID: " + step.TargetID);

            if (tutorialVisuals != null)
                tutorialVisuals.ShowStep(step);

            if (tempCanvas != null)
            {
                switch (step.TargetID)
                {
                    // ============ MAP ============

                    case global::TutorialTargetID.CountryIcon:
                        tempCanvas.ShowCountryIcon();
                        break;

                    case global::TutorialTargetID.CountryWindow:
                        tempCanvas.ShowCountryPopup();
                        break;

                    case global::TutorialTargetID.StartShift:
                        tempCanvas.ShowCountryPopupGlow();
                        break;

                    case global::TutorialTargetID.Wallet:
                        tempCanvas.ShowCoin();
                        break;

                    case global::TutorialTargetID.Calendar:
                        tempCanvas.ShowDay();
                        break;

                    // ============ MAIN GAME ============

                    case global::TutorialTargetID.CoffeeGrinder:
                        tempCanvas.ShowCoffeeGrinder();
                        break;

                    case global::TutorialTargetID.EspressoMachine:
                        tempCanvas.ShowEspressoMachine();
                        break;

                    case global::TutorialTargetID.Milk:
                        tempCanvas.ShowMilk();
                        break;

                    case global::TutorialTargetID.Cocoa:
                        tempCanvas.ShowCocoa();
                        break;

                    case global::TutorialTargetID.CoffeePortfilter:
                        tempCanvas.ShowCoffeePortfilter();
                        break;

                    case global::TutorialTargetID.Cup:
                        tempCanvas.ShowCup();
                        break;

                    case global::TutorialTargetID.Mat:
                        tempCanvas.ShowMat();
                        break;

                    case global::TutorialTargetID.Sink:
                        tempCanvas.ShowSink();
                        break;

                    case global::TutorialTargetID.TrashBin:
                        tempCanvas.ShowTrashBin();
                        break;

                    case global::TutorialTargetID.RecipePopup:
                        tempCanvas.ShowRecipePopup();
                        break;

                    default:
                        tempCanvas.HideAll();
                        break;
                }
            }
            else
            {
                Debug.LogWarning("TempCanvas is NOT assigned!");
            }
        }
        else
        {
            HideTutorialObjects();
        }

        // Hide the speaker model when requested by the tutorial step.
        bool hideSpeakerModel =
            isTutorial && step != null && step.HideSpeaker;

        if (speakerModel != null)
            speakerModel.SetActive(!hideSpeakerModel);

        if (dialogueText == null)
        {
            Debug.LogWarning("Dialogue Text is not assigned.");
            return;
        }

        line = line.Replace("{player_name}", PlayerUsername);

        dialogueText.text = line;
        dialogueText.maxVisibleCharacters = 0;
        dialogueText.ForceMeshUpdate();

        typingCoroutine = StartCoroutine(TypeText());
    }

    private void HideTutorialObjects()
    {
        if (tutorialVisuals != null)
            tutorialVisuals.HideAll();

        if (tempCanvas != null)
            tempCanvas.HideAll();
    }

    private void ApplyDialoguePosition(TutorialStepData step)
    {
        if (dialogueBox == null || step == null)
            return;

        if (step.UseCustomDialoguePosition)
            dialogueBox.anchoredPosition = step.DialoguePosition;
        else
            RestoreDialoguePosition();
    }

    private void RestoreDialoguePosition()
    {
        if (dialogueBox != null && originalPositionSaved)
            dialogueBox.anchoredPosition = originalDialoguePosition;
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
            autoButtonText.text = autoMode ? "PLAYING" : "AUTO";

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
            dialogueStarted &&
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
        StopAllCoroutines();

        dialogueStarted = false;
        isTyping = false;
        autoCoroutine = null;
        typingCoroutine = null;

        if (tutorialOverlay != null)
            tutorialOverlay.SetActive(false);

        if (dialogueRoot != null)
            dialogueRoot.SetActive(false);

        if (advanceButton != null)
            advanceButton.gameObject.SetActive(false);

        if (mysteriousFox != null)
            mysteriousFox.SetActive(false);

        HideTutorialObjects();

        // Notify listeners first. TutorialTransition will pause the game
        // when it opens TutorialVDO.
        onDialogueEnded?.Invoke();

        // Keep the existing option for other dialogues that need a gameplay lock.
        if (!keepGameplayLockedAfterDialogue)
        {
            GameplayInputLock.Unlock();
            ResumeGameAfterDialogue();
        }
    }

    private void PauseGameForDialogue()
    {
        if (!pauseGameDuringDialogue)
        {
            Debug.LogWarning(
                "Pause Game During Dialogue is disabled. " +
                "Gameplay input will still be locked."
            );

            return;
        }

        previousTimeScale = Time.timeScale;

        if (Time.timeScale != 0f)
        {
            Time.timeScale = 0f;
            wasGamePausedByDialogue = true;
        }
        else
        {
            // The game was already paused before this dialogue.
            wasGamePausedByDialogue = false;
        }

        Debug.Log(
            "Dialogue pause called. Time scale = " + Time.timeScale +
            ", Gameplay locked = " + GameplayInputLock.IsLocked
        );
    }

    private void ResumeGameAfterDialogue()
    {
        if (wasGamePausedByDialogue)
        {
            Time.timeScale = previousTimeScale;
            wasGamePausedByDialogue = false;
        }
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
