using UnityEngine;

public class America : MonoBehaviour
{
    [SerializeField] private GameObject AmericaPopup;
    [SerializeField] private GameObject textAmericaPopup;
    [SerializeField] private GameObject backgroundOverlay; // ªÿË¡‚ª√Ëß„ ∑’Ë§√Õ∫‡µÁ¡®Õ

    private void Start()
    {
        AmericaPopup.SetActive(false);
        textAmericaPopup.SetActive(false);
        if (backgroundOverlay != null)
            backgroundOverlay.SetActive(false);
    }

    private void OnMouseUpAsButton()
    {
        OpenPopup();
    }

    public void OpenPopup()
    {
        if (AmericaPopup != null)
            AmericaPopup.SetActive(true);

        if (textAmericaPopup != null)
            textAmericaPopup.SetActive(true);

        if (backgroundOverlay != null)
            backgroundOverlay.SetActive(true);
    }

    public void ClosePopup()
    {
        if (AmericaPopup != null)
            AmericaPopup.SetActive(false);

        if (textAmericaPopup != null)
            textAmericaPopup.SetActive(false);

        if (backgroundOverlay != null)
            backgroundOverlay.SetActive(false);
    }
}