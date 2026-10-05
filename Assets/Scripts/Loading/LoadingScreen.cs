using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class LoadingScreen : MonoBehaviour
{
    [Header("info")]
    [SerializeField] SpriteRenderer infoBoard;
    [SerializeField] Sprite[] infoSprites;

    [Header("Loading")]
    [SerializeField] TMP_Text loadingText;
    [SerializeField] float hopHeight = 10f;
    [SerializeField] float hopTime = 0.25f;
    [SerializeField] float hopGap = 0.06f;
    [SerializeField] float restTime = 0.4f;
    [SerializeField] float loadTime = 3f;
    [SerializeField] string nextScene = "main_game";

    void Awake()
    {
        int id = PlayerPrefs.GetInt("SelectedCharacter", 0);
        int index = GameData.selectedCharacter == "platu" ? 1 : 0;
        infoBoard.sprite = infoSprites[index];
    }

    void Start()
    {
        if (loadingText != null) StartCoroutine(Hop());
        StartCoroutine(GoNext());
    }

    IEnumerator Hop()
    {
        while (true)
        {
            loadingText.ForceMeshUpdate();
            TMP_TextInfo info = loadingText.textInfo;
            int count = info.characterCount;

            float total = hopGap * count + hopTime;
            float e = 0f;

            while (e < total)
            {
                e += Time.deltaTime;
                loadingText.ForceMeshUpdate();
                info = loadingText.textInfo;

                for (int i = 0; i < info.characterCount; i++)
                {
                    if (!info.characterInfo[i].isVisible) continue;

                    float start = i * hopGap;
                    float p = (e - start) / hopTime;

                    float offset = 0f;
                    if (p > 0f && p < 1f)
                        offset = Mathf.Sin(p * Mathf.PI) * hopHeight;

                    int mat = info.characterInfo[i].materialReferenceIndex;
                    int vert = info.characterInfo[i].vertexIndex;
                    Vector3[] verts = info.meshInfo[mat].vertices;

                    for (int j = 0; j < 4; j++)
                        verts[vert + j] += new Vector3(0f, offset, 0f);
                }

                for (int i = 0; i < info.meshInfo.Length; i++)
                {
                    info.meshInfo[i].mesh.vertices = info.meshInfo[i].vertices;
                    loadingText.UpdateGeometry(info.meshInfo[i].mesh, i);
                }

                yield return null;
            }

            yield return new WaitForSeconds(restTime);
        }
    }

    IEnumerator GoNext()
    {
        yield return new WaitForSeconds(loadTime);
        SceneManager.LoadScene(nextScene);
    }
}