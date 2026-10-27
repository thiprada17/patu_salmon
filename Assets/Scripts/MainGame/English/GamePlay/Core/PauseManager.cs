using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;

public class PauseManager : MonoBehaviour
{
    [SerializeField]
    private GameObject pausePanel;

    [SerializeField, Min(1)]
    private int pausePanelSortingOrder = 32767;

    private bool isPaused = false;
    private Canvas panelCanvas;

    private void Start()
    {
        Time.timeScale = 1f;
        GameplayInputLock.SetPauseLocked(false);

        if (pausePanel != null)
        {
            Canvas parentCanvas = pausePanel.GetComponentInParent<Canvas>();
            Canvas rootCanvas = parentCanvas != null
                ? parentCanvas.rootCanvas
                : null;

            panelCanvas = pausePanel.GetComponent<Canvas>();
            if (panelCanvas == null)
                panelCanvas = pausePanel.AddComponent<Canvas>();

            // PausePanel is nested under Tabbar. Match the scene's camera canvas
            // explicitly so its sorting override is respected at runtime.
            if (rootCanvas != null && rootCanvas != panelCanvas)
            {
                panelCanvas.renderMode = rootCanvas.renderMode;
                panelCanvas.worldCamera = rootCanvas.worldCamera;
                panelCanvas.planeDistance = rootCanvas.planeDistance;
                panelCanvas.targetDisplay = rootCanvas.targetDisplay;
            }
            else
            {
                panelCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }

            panelCanvas.overrideSorting = true;
            panelCanvas.sortingLayerName = "Popup";
            panelCanvas.sortingOrder = pausePanelSortingOrder;
            panelCanvas.enabled = true;

            // Keep any SpriteRenderer content in this panel on the same top layer.
            if (pausePanel.TryGetComponent(out SortingGroup sortingGroup))
            {
                sortingGroup.sortingLayerName = panelCanvas.sortingLayerName;
                sortingGroup.sortingOrder = panelCanvas.sortingOrder;
            }

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
        GameplayInputLock.SetPauseLocked(true);

        Time.timeScale = 0f;

        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
            pausePanel.transform.SetAsLastSibling();
            if (panelCanvas != null)
                panelCanvas.transform.SetAsLastSibling();
            Canvas.ForceUpdateCanvases();
        }

        Debug.Log("Game Paused");
    }


    public void ResumeGame()
    {
        isPaused = false;
        GameplayInputLock.SetPauseLocked(false);

        Time.timeScale = 1f;

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        Debug.Log("Game Resumed");
    }

    private void OnDestroy()
    {
        GameplayInputLock.SetPauseLocked(false);

        if (isPaused)
            Time.timeScale = 1f;
    }
}
