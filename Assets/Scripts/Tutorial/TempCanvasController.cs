using UnityEngine;

public class TempCanvasController : MonoBehaviour
{
    [Header("Temporary Elements")]
    [SerializeField] private GameObject usaIcon;
    [SerializeField] private GameObject usaPopup;
    [SerializeField] private GameObject usaPopupGlow;
    [SerializeField] private GameObject Coin;
    [SerializeField] private GameObject Day;

    //Main_game Element
    [SerializeField] private GameObject CoffeeGrinder;

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

        if(CoffeeGrinder != null)
            CoffeeGrinder.SetActive(false);
    }

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

    public void ShowCoffeeGrinder()
    {
        HideAll();

        if (CoffeeGrinder != null)
            CoffeeGrinder.SetActive(true);
        else
            Debug.LogWarning("Coffee Grinder is not assigned.");
    }
}