using UnityEngine;
using UnityEngine.SceneManagement;

public class BackButtonTemp : MonoBehaviour
{
    [SerializeField] private string sceneName = "signup"; // ชื่อ Scene ที่จะย้อนกลับไป

    // ผูกกับ OnClick ของปุ่ม
    public void GoBack()
    {
        SceneManager.LoadScene(sceneName);
    }

    // รองรับปุ่ม Back ของ Android (ใช้กับ Input Manager แบบเก่า)
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GoBack();
        }
    }
}