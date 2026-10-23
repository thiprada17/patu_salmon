using UnityEngine;
using UnityEngine.UI;

public class TutorialCongratulationsPopup : MonoBehaviour
{
    [Header("Popup References")]
    [SerializeField] private GameObject popup;
    [SerializeField] private Button continueButton;

    [Header("Dim Background")]
    [SerializeField] private GameObject dimImage;

    private bool isShowing;

    private void Awake()
    {
        if (popup != null)
            popup.SetActive(false);

        if (dimImage != null)
            dimImage.SetActive(false);

        if (continueButton != null)
            continueButton.onClick.AddListener(ClosePopup);
        else
            Debug.LogWarning(
                "[TutorialCongratulationsPopup] Continue Button is not assigned."
            );
    }

    private void Start()
    {
        // Only show after the tutorial has been completed.
        if (TutorialSession.ShowCongratulationsOnMap)
            ShowPopup();
    }

    private void ShowPopup()
    {
        if (popup == null)
        {
            Debug.LogError(
                "[TutorialCongratulationsPopup] Popup is not assigned."
            );
            return;
        }

        // Block map clicks, including Physics2D country clicks.
        if (MapInputController.Instance != null)
        {
            MapInputController.Instance.SetCongratulationsPopupOpen(true);
        }
        else
        {
            Debug.LogError(
                "[TutorialCongratulationsPopup] MapInputController not found!"
            );
        }

        // Show the dim background and congratulations popup.
        if (dimImage != null)
            dimImage.SetActive(true);

        popup.SetActive(true);
        isShowing = true;

        // Consume the flag so it won't appear again on another map visit.
        TutorialSession.ConsumeCongratulationsPopup();

        Debug.Log(
            "[TutorialCongratulationsPopup] Popup shown; map input blocked."
        );
    }

    private void ClosePopup()
    {
        if (!isShowing)
            return;

        isShowing = false;

        if (popup != null)
            popup.SetActive(false);

        if (dimImage != null)
            dimImage.SetActive(false);

        // Restore map clicks. The dialogue tutorial lock remains respected.
        if (MapInputController.Instance != null)
        {
            MapInputController.Instance.SetCongratulationsPopupOpen(false);
        }

        Debug.Log(
            "[TutorialCongratulationsPopup] Popup closed; map input restored."
        );
    }

    private void OnDestroy()
    {
        if (continueButton != null)
            continueButton.onClick.RemoveListener(ClosePopup);

        // Avoid leaving map input locked if this object is destroyed.
        if (isShowing && MapInputController.Instance != null)
        {
            MapInputController.Instance.SetCongratulationsPopupOpen(false);
        }
    }
}