
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuButtonLoader : MonoBehaviour
{
    [Header("Dropdown Animation")]
    [SerializeField] private MenuDropdownAnimation dropdownAnimation;
    [SerializeField] private GameObject arrowDown;
    [SerializeField] private GameObject arrowUp;
    [SerializeField] private Button mapButton;
    [SerializeField] private Button settingButton;

    private bool isOpen = true;

    private void Start()
    {
        // ไม่ปิด Dropdown Panel อีกต่อไป
        UpdateArrow();
        UpdateTutorialNavigation();
    }

    private void Update()
    {
        UpdateTutorialNavigation();
    }

    private void UpdateTutorialNavigation()
    {
        bool navigationEnabled = !TutorialSession.IsTutorialActive;

        if (mapButton != null)
            mapButton.interactable = navigationEnabled;

        if (settingButton != null)
            settingButton.interactable = navigationEnabled;
    }

    public void ToggleDropdown()
    {
        isOpen = !isOpen;

        if (dropdownAnimation != null)
            dropdownAnimation.ToggleDropdown();

        UpdateArrow();
    }

    private void UpdateArrow()
    {
        if (arrowDown != null)
            arrowDown.SetActive(!isOpen);

        if (arrowUp != null)
            arrowUp.SetActive(isOpen);
    }

    public void LoadScene(string sceneName)
    {
        if (IsBlockedDuringTutorial(sceneName))
            return;

        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    public void LoadSceneOverlay(string sceneName)
    {
        if (IsBlockedDuringTutorial(sceneName))
            return;

        if (SceneManager.GetSceneByName(sceneName).isLoaded) return;

        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
    }

    private bool IsBlockedDuringTutorial(string sceneName)
    {
        if (!TutorialSession.IsTutorialActive)
            return false;

        return sceneName == "map" || sceneName == "setting";
    }
}
