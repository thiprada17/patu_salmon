
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class MapDialogueController : MonoBehaviour
{
    [Header("Dialogue UI")]
    [SerializeField] private GameObject dialogueRoot;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private TMP_Text speakerText;
    [SerializeField] private Button advanceButton;
    [SerializeField] private Button autoButton;
    [SerializeField] private TMP_Text autoButtonText;

    [Header("Dialogue Content")]
    [SerializeField] private string[] speakerNames;

    [TextArea(2, 4)]
    [SerializeField] private string[] dialogueLines;

    [Header("Auto Settings")]
    [SerializeField] private float autoAdvanceDelay = 2.5f;

    [Header("Auto Button Sprites")]
    [SerializeField] private Image autoButtonImage;
    [SerializeField] private Sprite autoOffSprite;
    [SerializeField] private Sprite autoOnSprite;

    private int currentLine;
    private bool autoMode;
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
        dialogueRoot.SetActive(true);
        ShowDialogue();
    }

    public void NextDialogue()
    {
        StopAutoAdvance();

        currentLine++;

        if (currentLine >= dialogueLines.Length)
        {
            dialogueRoot.SetActive(false);
            return;
        }

        ShowDialogue();
    }

    private void ShowDialogue()
    {
        dialogueText.text = dialogueLines[currentLine];

        if (speakerText != null &&
            speakerNames != null &&
            speakerNames.Length > currentLine)
        {
            speakerText.text = speakerNames[currentLine];
        }

        if (autoMode)
            ScheduleAutoAdvance();
    }

    public void ToggleAutoMode()
    {
        autoMode = !autoMode;
        UpdateAutoButton();

        if (autoMode)
            ScheduleAutoAdvance();
        else
            StopAutoAdvance();
    }

    private void UpdateAutoButton()
    {
        if (autoButtonText != null)
        {
            autoButtonText.text = autoMode
                ? "PLAYING" //AUTO: ON
                : "AUTO"; //AUTO: OFF
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

        if (autoMode && dialogueRoot.activeSelf)
            NextDialogue();
    }

    private void StopAutoAdvance()
    {
        if (autoCoroutine != null)
        {
            StopCoroutine(autoCoroutine);
            autoCoroutine = null;
        }
    }

    private void OnDestroy()
    {
        StopAutoAdvance();
    }
}
