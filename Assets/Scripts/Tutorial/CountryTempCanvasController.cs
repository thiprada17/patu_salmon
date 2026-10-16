using UnityEngine;

public class CountryTempCanvasController : MonoBehaviour
{
    [Header("Temporary Country Elements")]
    [SerializeField] private GameObject usaIcon;
    [SerializeField] private GameObject usaPopup;
    [SerializeField] private GameObject usaPopupGlow;

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
}