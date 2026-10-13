
using UnityEngine;

public enum TutorialHandAnimation
{
    Static,
    Bounce
}

[CreateAssetMenu(
    fileName = "NewTutorialStep",
    menuName = "Platu Salmon/Tutorial Step")]
public class TutorialStepData : ScriptableObject
{
    [Header("Step Information")]
    [SerializeField] private string stepID;
    [SerializeField] private int order = 1;

    [Header("Dialogue")]
    [SerializeField] private string speakerName;

    [TextArea(2, 5)]
    [SerializeField] private string dialogue;

    [Header("Destination")]
    [Tooltip("ID of the existing object in the scene.")]
    [SerializeField] private TutorialTargetID targetID;

    [Tooltip("Optional preview of the destination's existing sprite.")]
    [SerializeField] private Sprite targetPreviewSprite;

    [Header("Hand Pointer")]
    [SerializeField] private Sprite handPointerSprite;
    [SerializeField] private Vector2 handOffset = new Vector2(0f, 55f);
    [SerializeField]
    private TutorialHandAnimation handAnimation =
        TutorialHandAnimation.Bounce;

    [Header("Progression")]
    [SerializeField] private bool waitForTargetClick;

    public string StepID => stepID;
    public int Order => order;
    public string SpeakerName => speakerName;
    public string Dialogue => dialogue;
    public TutorialTargetID TargetID => targetID;
    public Sprite TargetPreviewSprite => targetPreviewSprite;
    public Sprite HandPointerSprite => handPointerSprite;
    public Vector2 HandOffset => handOffset;
    public TutorialHandAnimation HandAnimation => handAnimation;
    public bool WaitForTargetClick => waitForTargetClick;
}
