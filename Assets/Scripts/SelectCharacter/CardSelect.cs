using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

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
        GameData.selectedCharacter = characterName;
        glow.SetActive(true);

        Sequence s = DOTween.Sequence();
        s.Append(card.DOPunchScale(Vector3.one * -0.05f, 0.2f));
        s.Append(otherCard.DOMoveY(otherCard.position.y + 15f, 0.5f).SetEase(Ease.InBack));
        if (title != null) s.Join(title.DOScale(0, 0.3f).SetEase(Ease.InBack));
        s.Append(card.DOLocalMove(targetPosition, 0.4f).SetEase(Ease.OutBack));
        s.Join(card.DORotate(Vector3.zero, 0.4f));
        s.Join(card.DOScale(targetScale, 0.4f));
        s.OnComplete(() =>
        {
            clicked = false;
            SceneManager.LoadScene("id_card");
        });
        s.timeScale = speed;
    }
}