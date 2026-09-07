using UnityEngine;
using UnityEngine.EventSystems;

public class PopupClose : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private GameObject popup;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (popup != null)
            popup.SetActive(false);
    }
}