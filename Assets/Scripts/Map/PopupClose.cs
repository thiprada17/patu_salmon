using UnityEngine;
using UnityEngine.EventSystems;

public class PopupClose : MonoBehaviour
{
    [SerializeField] private GameObject thailandPopup;
    [SerializeField] private GameObject americaPopup;
    [SerializeField] private GameObject japanPopup;
    [SerializeField] private GameObject backgroundOverlay;

    public void ClosePopup()
    {
        if (thailandPopup != null)
            thailandPopup.SetActive(false);

        if (americaPopup != null)
            americaPopup.SetActive(false);

        if (japanPopup != null)
            japanPopup.SetActive(false);

        if (backgroundOverlay != null)
            backgroundOverlay.SetActive(false);
    }
}