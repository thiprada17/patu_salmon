
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class BackButtonTemp : MonoBehaviour
{
    [SerializeField] private string sceneName = "signup";

    public void GoBack()
    {
        SceneManager.LoadScene(sceneName);
    }

    void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            GoBack();
        }

        if (Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            return;
        }
    }
}
