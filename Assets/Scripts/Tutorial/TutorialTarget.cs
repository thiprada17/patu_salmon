
using UnityEngine;

public enum TutorialTargetID
{
    VocabCollection,
    CustomerCollection,
    DailyTask,
    Gacha,
    MailBox
}

public class TutorialTarget : MonoBehaviour
{
    [SerializeField] private TutorialTargetID targetID;

    public TutorialTargetID TargetID => targetID;
}
