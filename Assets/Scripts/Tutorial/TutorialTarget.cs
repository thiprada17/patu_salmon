
using UnityEngine;

public enum TutorialTargetID
{
    VocabCollection,
    CustomerCollection,
    DailyTask,
    Gacha,
    MailBox,
    Wallet,
    Calendar,
    Profile,
    CountryIcon,
    CountryWindow,
    StartShift
}

public class TutorialTarget : MonoBehaviour
{
    [SerializeField] private TutorialTargetID targetID;

    public TutorialTargetID TargetID => targetID;
}
