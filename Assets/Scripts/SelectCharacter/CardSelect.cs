using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEngine.Rendering;

public class CardSelect : MonoBehaviour
{
    public string characterName;      
    public Transform card;            
    public Transform otherCard;       
    public Transform title;      
    public GameObject glow;
    public Vector3 targetPosition;
    public Vector3 targetScale = Vector3.one;

    [Range(0.1f, 2f)] public float speed = 0.5f;

    static bool clicked;

    void OnMouseDown()
    {
        if (clicked) return;
        clicked = true;
        foreach (Renderer r in card.GetComponentsInChildren<Renderer>(true))
            r.sortingOrder += 100;
        GameData.selectedCharacter = characterName;
        PlayerPrefs.SetString("Character", characterName);
        glow.SetActive(true);

        Sequence s = DOTween.Sequence();
        float t = 0.5f;
        s.Append(card.DOPunchScale(Vector3.one * -0.05f, 0.15f));
        s.Append(otherCard.DOMoveY(otherCard.position.y + 15f, t).SetEase(Ease.InCubic));
        if (title != null) s.Join(title.DOScale(0, t * 0.6f).SetEase(Ease.InBack));
        s.Join(card.DOLocalMove(targetPosition, t).SetEase(Ease.OutCubic));
        s.Join(card.DORotate(Vector3.zero, t).SetEase(Ease.OutCubic));
        s.Join(card.DOScale(targetScale, t).SetEase(Ease.OutCubic));
        s.OnComplete(() =>
        {
            clicked = false;
            SceneManager.LoadScene("id_card");
        });
        s.timeScale = speed;
    }
}