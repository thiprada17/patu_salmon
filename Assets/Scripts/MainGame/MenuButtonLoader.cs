
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuButtonLoader : MonoBehaviour
{
    [Header("Dropdown Animation")]
    [SerializeField] private MenuDropdownAnimation dropdownAnimation;
    [SerializeField] private GameObject arrowDown;
    [SerializeField] private GameObject arrowUp;

    private bool isOpen = true;

    private void Start()
    {
        // ไม่ปิด Dropdown Panel อีกต่อไป
        UpdateArrow();
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
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    public void LoadSceneOverlay(string sceneName)
    {
        if (SceneManager.GetSceneByName(sceneName).isLoaded) return;

        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
    }
}
