using UnityEngine;

public class TempCanvasController : MonoBehaviour
{
    [Header("Temporary Elements")]
    //Map
    [SerializeField] private GameObject usaIcon;
    [SerializeField] private GameObject usaPopup;
    [SerializeField] private GameObject usaPopupGlow;
    [SerializeField] private GameObject Coin;
    [SerializeField] private GameObject Day;

    //Main Game
    [SerializeField] private GameObject CoffeeGrinder;
    [SerializeField] private GameObject EspressoMachine;
    [SerializeField] private GameObject Milk;
    [SerializeField] private GameObject Cocoa;
    [SerializeField] private GameObject CoffeePortfilter;
    [SerializeField] private GameObject Cup;
    [SerializeField] private GameObject Mat;
    [SerializeField] private GameObject Sink;
    [SerializeField] private GameObject TrashBin;

    private void Awake()
    {
        HideAll();
    }

    public void HideAll()
    {
        if (usaIcon != null)
            usaIcon.SetActive(false);

        if (usaPopup != null)
            usaPopup.SetActive(false);

        if (usaPopup != null)
            usaPopupGlow.SetActive(false);

        if (Coin != null)
            Coin.SetActive(false);

        if (Day != null)
            Day.SetActive(false);

        if (CoffeeGrinder != null)
            CoffeeGrinder.SetActive(false);

        if (EspressoMachine != null)
            EspressoMachine.SetActive(false);

        if (Milk != null)
            Milk.SetActive(false);

        if (Cocoa != null)
            Cocoa.SetActive(false);

        if (CoffeePortfilter != null)
            CoffeePortfilter.SetActive(false);

        if (Cup != null)
            Cup.SetActive(false);

        if (Mat != null) 
            Mat.SetActive(false);

        if (Sink != null)
            Sink.SetActive(false);

        if(TrashBin != null)
            TrashBin.SetActive(false);
    }

    //Map Element
    public void ShowCountryIcon()
    {
        HideAll();

        if (usaIcon != null)
        {
            usaIcon.SetActive(true);
            Debug.Log("USA Icon activated: " + usaIcon.name);
        }
        else
        {
            Debug.LogWarning("USA Icon is not assigned!");
        }
    }

    public void ShowCountryPopup()
    {
        HideAll();

        if (usaPopup != null)
            usaPopup.SetActive(true);
        else
            Debug.LogWarning("USA Popup is not assigned.");
    }

    public void ShowCountryPopupGlow()
    {
        HideAll();

        if (usaPopupGlow != null)
            usaPopupGlow.SetActive(true);
        else
            Debug.LogWarning("USA Popup Glow is not assigned.");
    }

    public void ShowCoin()
    {
        HideAll();

        if (Coin != null)
            Coin.SetActive(true);
        else
            Debug.LogWarning("Coin Bar is not assigned.");
    }

    public void ShowDay()
    {
        HideAll();

        if (Day != null)
            Day.SetActive(true);
        else
            Debug.LogWarning("Day Bar is not assigned.");
    }

    //USA - Main Game Element
    public void ShowCoffeeGrinder()
    {
        HideAll();

        if (CoffeeGrinder != null)
            CoffeeGrinder.SetActive(true);
        else
            Debug.LogWarning("Coffee Grinder is not assigned.");
    }

    public void ShowEspressoMachine()
    {
        HideAll();

        if (EspressoMachine != null)
            EspressoMachine.SetActive(true);
        else
            Debug.LogWarning("Espresso Machine is not assigned.");
    }

    public void ShowMilk()
    {
        HideAll();

        if (Milk != null)
            Milk.SetActive(true);
        else
            Debug.LogWarning("Milk is not assigned.");
    }

    public void ShowCocoa()
    {
        HideAll();

        if (Cocoa != null)
            Cocoa.SetActive(true);
        else
            Debug.LogWarning("Cocoa is not assigned.");
    }

    public void ShowCoffeePortfilter()
    {
        HideAll();

        if (CoffeePortfilter != null)
            CoffeePortfilter.SetActive(true);
        else
            Debug.LogWarning("Coffee Portfilter is not assigned.");
    }

    public void ShowCup()
    {
        HideAll();

        if (Cup != null)
            Cup.SetActive(true);
        else
            Debug.LogWarning("Cup is not assigned.");
    }

    public void ShowMat()
    {
        HideAll();

        if (Mat != null)
            Mat.SetActive(true);
        else
            Debug.LogWarning("Mat is not assigned.");
    }

    public void ShowSink()
    {
        HideAll();

        if (Sink != null)
            Sink.SetActive(true);
        else
            Debug.LogWarning("Sink is not assigned.");
    }

    public void ShowTrashBin()
    {
        HideAll();

        if (TrashBin != null)
            TrashBin.SetActive(true);
        else
            Debug.LogWarning("Trash Bin is not assigned.");
    }
}