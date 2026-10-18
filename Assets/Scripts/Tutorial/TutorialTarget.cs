
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

    //Stations
    CoffeeGrinder,
    EspressoMachine,
    Milk,
    Cocoa,
    CoffeePortfilter,
    Cup,
    Mat,
    TrashBin,
    Sink,

    //Tab Bar in main_game
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
