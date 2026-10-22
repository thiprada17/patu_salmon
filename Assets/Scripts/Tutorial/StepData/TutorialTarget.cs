
using UnityEngine;

public enum TutorialTargetID
{

    //Map Element
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
    StartShift,


    //Main Game Element
    CoffeeGrinder,
    EspressoMachine,
    Milk,
    Cocoa,
    CoffeePortfilter,
    Cup,
    Mat,
    Sink,
    TrashBin,
    RecipePopup,


    //Main Game Tab Bar
    Map,
    Setting,
    RecipeBook,
    LightBulb

}

public class TutorialTarget : MonoBehaviour
{
    [SerializeField] private TutorialTargetID targetID;

    public TutorialTargetID TargetID => targetID;
}
