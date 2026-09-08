using UnityEngine;

public class Thailand : MonoBehaviour
{
    [SerializeField] private GameObject thailandPopup;
    [SerializeField] private GameObject textThailandPopup;
    [SerializeField] private GameObject backgroundOverlay; // ªÿË¡‚ª√Ëß„ ∑’Ë§√Õ∫‡µÁ¡®Õ

    private void Start()
    {
        thailandPopup.SetActive(false);
        textThailandPopup.SetActive(false);
        if (backgroundOverlay != null)
            backgroundOverlay.SetActive(false);
    }

    private void OnMouseUpAsButton()
    {
        OpenPopup();
    }

    public void OpenPopup()
    {
        if (thailandPopup != null)
            thailandPopup.SetActive(true);

        if (textThailandPopup != null)
            textThailandPopup.SetActive(true);

        if (backgroundOverlay != null)
            backgroundOverlay.SetActive(true);
    }

    public void ClosePopup()
    {
        if (thailandPopup != null)
            thailandPopup.SetActive(false);

        if (textThailandPopup != null)
            textThailandPopup.SetActive(false);

        if (backgroundOverlay != null)
            backgroundOverlay.SetActive(false);
    }
}