
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
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
    [SerializeField, Min(0f)] private float typingSpeed = 0.035f;

    [Header("Auto Settings")]
    [SerializeField, Min(0f)] private float autoAdvanceDelay = 2.5f;

    [Header("Tutorial Visuals")]
    [SerializeField] private TutorialVisualController tutorialVisuals;

    [Header("Temporary UI Highlights")]
    [SerializeField] private TempCanvasController tempCanvas;

    [Header("Tutorial Character")]
    [SerializeField] private GameObject mysteriousFox;

    [Header("Tutorial Pause")]
    [SerializeField] private bool pauseGameDuringDialogue = true;

    [Tooltip("Keep gameplay paused and input locked after this dialogue ends.")]
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

    //Username Variable
    private string playerUsername;

    private bool wasGamePausedByDialogue;
    private float previousTimeScale = 1f;

    private bool inputLockAcquired;

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
        string sceneName = SceneManager.GetActiveScene().name;

        Debug.Log(
            $"[DialogueController] Start. Scene={sceneName}, " +
            $"Initialized={TutorialSession.IsInitialized}, " +
            $"FirstTime={TutorialSession.IsFirstTimePlayer}"
        );

        // Only run the dialogue in the map or gameplay scene.
        if (sceneName != "map" && sceneName != "main_game")
        {
            HideDialogueUI();
            return;
        }

        // Do not show dialogue until player stats are initialized.
        if (!TutorialSession.IsInitialized)
        {
            Debug.LogWarning(
                "[DialogueController] Player stats are not initialized. " +
                "Dialogue will remain hidden."
            );

            HideDialogueUI();
            return;
        }

        // Returning players skip the entire dialogue sequence,
        // including Normal and Tutorial entries.
        if (!TutorialSession.IsFirstTimePlayer)
        {
            Debug.Log(
                "[DialogueController] Returning player. " +
                "Skipping the entire dialogue sequence."
            );

            HideDialogueUI();
            return;
        }

        // Start the gameplay tutorial only in main_game.
        if (sceneName == "main_game")
        {
            TutorialSession.StartTutorial();
        }

        if (dialogueSequence == null || dialogueSequence.Length == 0)
        {
            Debug.LogWarning(
                "[DialogueController] No Dialogue Sequence entries assigned."
            );

            HideDialogueUI();
            return;
        }

        currentLine = 0;

        autoMode = false;
        dialogueStarted = true;
        isTutorial = false;

        //Get Player Usernmae From PlayerPrefs
        playerUsername = PlayerPrefs.GetString("Username", "Player");

        bool allowGameplayDuringTutorial =
            TutorialSession.IsTutorialActive && sceneName == "main_game";

        if (!allowGameplayDuringTutorial)
        {
            GameplayInputLock.Lock();
            inputLockAcquired = true;
            PauseGameForDialogue();
        }

        if (tutorialOverlay != null)
        {
            tutorialOverlay.SetActive(true);

            // The tutorial overlay is visual only in main_game. Let station
            // and item input pass through it while the tutorial is active.
            if (allowGameplayDuringTutorial)
            {
                CanvasGroup overlayGroup =
                    tutorialOverlay.GetComponent<CanvasGroup>();

                if (overlayGroup != null)
                    overlayGroup.blocksRaycasts = false;
            }
        }

        if (dialogueRoot != null)
            dialogueRoot.SetActive(true);

        if (advanceButton != null)
            advanceButton.gameObject.SetActive(true);

        ShowDialogue();
    }


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

        // First click finishes the typewriter.
        if (isTyping)
        {
            FinishTyping();
            return;
        }

        StopAutoAdvance();

        int nextLine = FindNextEligibleEntry(currentLine + 1);

        if (nextLine >= dialogueSequence.Length)
        {
            EndDialogue();
            return;
        }

        currentLine = nextLine;
        ShowDialogue();
    }

    private int FindNextEligibleEntry(int startIndex)
    {
        int index = Mathf.Max(0, startIndex);

        while (index < dialogueSequence.Length &&
            ShouldSkipTutorialEntry(dialogueSequence[index]))
        {
            Debug.Log(
                $"[DialogueController] Skipping entry {index}: " +
                $"{dialogueSequence[index].EntryType}"
            );

            index++;
        }

        return index;
    }

    private bool ShouldSkipTutorialEntry(DialogueSequenceEntry entry)
    {
        if (entry == null)
            return true;

        // Normal dialogue should remain available to everyone.
        if (entry.EntryType == DialogueEntryType.Normal)
            return false;

        // Do not show tutorial entries until player stats are loaded.
        if (!TutorialSession.IsInitialized)
        {
            Debug.LogWarning(
                "Player stats are not initialized. Skipping tutorial entry."
            );

            return true;
        }

        // Only players with work_days == 0 can see tutorial entries.
        bool shouldSkip = !TutorialSession.IsFirstTimePlayer;

        if (shouldSkip)
        {
            Debug.Log(
                $"Skipping tutorial entry at index {currentLine}. " +
                $"FirstTimePlayer={TutorialSession.IsFirstTimePlayer}"
            );
        }

        return shouldSkip;
    }

    private void ShowDialogue()
    {
        StopTyping();
        StopAutoAdvance();

        if (dialogueSequence == null ||
            currentLine < 0 ||
            currentLine >= dialogueSequence.Length)
        {
            EndDialogue();
            return;
        }

        DialogueSequenceEntry entry = dialogueSequence[currentLine];

        if (entry == null)
        {
            Debug.LogWarning(
                $"[DialogueController] Empty entry at index {currentLine}."
            );

            AdvancePastInvalidEntry();
            return;
        }

        isTutorial = entry.EntryType == DialogueEntryType.Tutorial;

        Debug.Log(
            $"[DialogueController] Index={currentLine}, " +
            $"Type={entry.EntryType}, Scene={SceneManager.GetActiveScene().name}"
        );

        TutorialStepData step = isTutorial ? entry.TutorialStep : null;
        string line;

        if (mysteriousFox != null)
            mysteriousFox.SetActive(true);

        if (isTutorial && step == null)
        {
            Debug.LogWarning(
                $"[DialogueController] Missing TutorialStepData " +
                $"at index {currentLine}."
            );

            HideTutorialObjects();
            AdvancePastInvalidEntry();
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
                    $"[DialogueController] Invalid normal dialogue index: {index}."
                );

                AdvancePastInvalidEntry();
                return;
            }

            line = dialogueLines[index] ?? string.Empty;
            RestoreDialoguePosition();
        }
        else
        {
            line = step.Dialogue ?? string.Empty;
            ApplyDialoguePosition(step);
        }

        // Show tutorial visuals only for Tutorial entries.
        if (isTutorial)
        {
            if (tutorialVisuals != null)
                tutorialVisuals.ShowStep(step);

            ShowTargetHighlight(step.TargetID);
        }
        else
        {
            HideTutorialObjects();
        }

        bool hideSpeaker =
            isTutorial && step != null && step.HideSpeaker;

        if (speakerModel != null)
            speakerModel.SetActive(!hideSpeaker);

        if (dialogueText == null)
        {
            Debug.LogError(
                "[DialogueController] Dialogue Text is not assigned."
            );

            AdvancePastInvalidEntry();
            return;
        }

        line = line.Replace("{player_name}", playerUsername);

        dialogueText.text = line;
        dialogueText.maxVisibleCharacters = 0;
        dialogueText.ForceMeshUpdate();

        typingCoroutine = StartCoroutine(TypeText());
    }

    private void ShowTargetHighlight(TutorialTargetID targetID)
    {
        if (tempCanvas == null)
        {
            Debug.LogWarning(
                "[DialogueController] TempCanvas is not assigned."
            );

            return;
        }

        switch (targetID)
        {
            //========TEMP CANVAS========
            //Map Icon & Popup
            case global::TutorialTargetID.CountryIcon: tempCanvas.ShowCountryIcon(); break;
            case global::TutorialTargetID.CountryWindow: tempCanvas.ShowCountryPopup(); break;
            case global::TutorialTargetID.StartShift: tempCanvas.ShowCountryPopupGlow(); break;

            //Map Navbar
            case global::TutorialTargetID.Wallet: tempCanvas.ShowCoin(); break;
            case global::TutorialTargetID.Calendar: tempCanvas.ShowDay(); break;

            //Main_Game
            case global::TutorialTargetID.CoffeeGrinder: tempCanvas.ShowCoffeeGrinder(); break;
            case global::TutorialTargetID.EspressoMachine: tempCanvas.ShowEspressoMachine(); break;
            case global::TutorialTargetID.Milk: tempCanvas.ShowMilk(); break;
            case global::TutorialTargetID.Cocoa: tempCanvas.ShowCocoa(); break;
            case global::TutorialTargetID.CoffeePortfilter: tempCanvas.ShowCoffeePortfilter(); break;
            case global::TutorialTargetID.Cup: tempCanvas.ShowCup(); break;
            case global::TutorialTargetID.Mat: tempCanvas.ShowMat(); break;
            case global::TutorialTargetID.Sink: tempCanvas.ShowSink(); break;
            case global::TutorialTargetID.TrashBin: tempCanvas.ShowTrashBin(); break;
            case global::TutorialTargetID.RecipePopup: tempCanvas.ShowRecipePopup(); break;

            default: tempCanvas.HideAll(); break;
        }
    }

    private void AdvancePastInvalidEntry()
    {
        int nextLine = FindNextEligibleEntry(currentLine + 1);

        if (nextLine >= dialogueSequence.Length)
        {
            EndDialogue();
            return;
        }

        currentLine = nextLine;
        ShowDialogue();
    }

    private void HideDialogueUI()
    {
        dialogueStarted = false;
        isTyping = false;

        if (tutorialOverlay != null)
            tutorialOverlay.SetActive(false);

        if (dialogueRoot != null)
            dialogueRoot.SetActive(false);

        if (advanceButton != null)
            advanceButton.gameObject.SetActive(false);

        if (mysteriousFox != null)
            mysteriousFox.SetActive(false);

        HideTutorialObjects();
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
            autoButtonImage.sprite = autoMode ? autoOnSprite : autoOffSprite;
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
        if (!dialogueStarted)
            return;

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

        // Keep the existing Inspector event.
        onDialogueEnded?.Invoke();

        if (!keepGameplayLockedAfterDialogue)
        {
            UnlockGameplayInput();
            ResumeGameAfterDialogue();
        }
    }

    private void PauseGameForDialogue()
    {
        if (!pauseGameDuringDialogue)
        {
            Debug.LogWarning(
                "[DialogueController] Pause is disabled; " +
                "gameplay input is still locked."
            );

            return;
        }

        previousTimeScale = Time.timeScale;
        wasGamePausedByDialogue = Time.timeScale != 0f;

        if (wasGamePausedByDialogue)
            Time.timeScale = 0f;

        Debug.Log(
            $"[DialogueController] TimeScale={Time.timeScale}, " +
            $"GameplayLocked={GameplayInputLock.IsLocked}"
        );
    }

    private void ResumeGameAfterDialogue()
    {
        if (!wasGamePausedByDialogue)
            return;

        Time.timeScale = previousTimeScale;
        wasGamePausedByDialogue = false;
    }

    private void UnlockGameplayInput()
    {
        if (!inputLockAcquired)
            return;

        GameplayInputLock.Unlock();
        inputLockAcquired = false;
    }

    private void OnDestroy()
    {
        StopTyping();
        StopAutoAdvance();

        // Avoid leaving gameplay paused/locked if this controller is destroyed.
        UnlockGameplayInput();
        ResumeGameAfterDialogue();

        if (advanceButton != null)
            advanceButton.onClick.RemoveListener(NextDialogue);

        if (autoButton != null)
            autoButton.onClick.RemoveListener(ToggleAutoMode);
    }

    //For Tutorial
    public void LoadMainGameAfterDialogue()
    {
        // Only transition after the map dialogue finishes.
        if (SceneManager.GetActiveScene().name != "map")
            return;

        Debug.Log("[DialogueController] Map dialogue finished. Loading main_game.");

        // Make sure the tutorial session remains active for gameplay.
        if (TutorialSession.IsInitialized &&
            TutorialSession.IsFirstTimePlayer)
        {
            TutorialSession.StartTutorial();
        }

        SceneManager.LoadScene("main_game");
    }

}
