using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class CardSelect : MonoBehaviour
{
    [SerializeField] GameObject glow;
    [SerializeField] int characterId;
    [SerializeField] string nextScene = "GameScene";
    [SerializeField] float delay = 0.6f;

    static bool locked = false;

    void Start()
    {
        locked = false;
        if (glow != null) glow.SetActive(false);
    }

    void OnMouseDown()
    {
        Debug.Log("กดโดนแล้ว: " + gameObject.name);
        if (locked) return;
        locked = true;

        if (glow != null) glow.SetActive(true);

        PlayerPrefs.SetInt("SelectedCharacter", characterId);
        PlayerPrefs.Save();

        StartCoroutine(GoNext());
    }

    IEnumerator GoNext()
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(nextScene);
    }
}