using UnityEngine;
using UnityEngine.SceneManagement;

public class BackButtonTemp : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("BackButtonTemp is running!");
    }

    public void GoBack()
    {
        Debug.Log("BACK BUTTON CLICKED!");

        SceneManager.LoadScene("map");
    }
}