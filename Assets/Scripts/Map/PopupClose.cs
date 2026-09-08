using UnityEngine;
using UnityEngine.EventSystems;

public class PopupClose : MonoBehaviour
{
    [SerializeField] private GameObject thailandPopup;
    [SerializeField] private GameObject americaPopup;
    [SerializeField] private GameObject backgroundOverlay;

    public void ClosePopup()
    {
        if (thailandPopup != null)
            thailandPopup.SetActive(false);

        if (americaPopup != null)
            americaPopup.SetActive(false);

        if (backgroundOverlay != null)
            backgroundOverlay.SetActive(false);
    }
}