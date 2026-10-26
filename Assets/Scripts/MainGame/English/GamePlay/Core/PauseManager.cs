using UnityEngine;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    [SerializeField]
    private GameObject pausePanel;

    [SerializeField, Min(1)]
    private int pausePanelSortingOrder = 100;

    private bool isPaused = false;

    private void Start()
    {
        Time.timeScale = 1f;

        if (pausePanel != null)
        {
            Canvas panelCanvas = pausePanel.GetComponent<Canvas>();
            if (panelCanvas == null)
                panelCanvas = pausePanel.AddComponent<Canvas>();

            panelCanvas.overrideSorting = true;
            panelCanvas.sortingLayerName = "Popup";
            panelCanvas.sortingOrder = pausePanelSortingOrder;

            if (pausePanel.GetComponent<GraphicRaycaster>() == null)
                pausePanel.AddComponent<GraphicRaycaster>();

            pausePanel.SetActive(false);
        }
        else
        {
            Debug.LogError("PauseManager: Pause Panel is not assigned.");
        }
    }


    public void TogglePause()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }


    public void PauseGame()
    {
        isPaused = true;

        Time.timeScale = 0f;

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
            pausePanel.transform.SetAsLastSibling();
        }

        Debug.Log("Game Paused");
    }


    public void ResumeGame()
    {
        isPaused = false;

        Time.timeScale = 1f;

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        Debug.Log("Game Resumed");
    }
}
