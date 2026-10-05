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
    public GameObject currentPage;
    public GameObject nextPage;

    [Range(0.1f, 2f)] public float speed = 0.5f;

    static bool clicked;

    Vector3 startPos, startScale, titleScale;
    Quaternion startRot;
    Renderer[] renderers;
    int[] startOrders;

    void Awake()
    {
        startPos = card.localPosition;
        startRot = card.localRotation;
        startScale = card.localScale;
        if (title != null) titleScale = title.localScale;

        renderers = card.GetComponentsInChildren<Renderer>(true);
        startOrders = new int[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
            startOrders[i] = renderers[i].sortingOrder;
    }

    void OnEnable()
    {
        card.DOKill();
        card.localPosition = startPos;
        card.localRotation = startRot;
        card.localScale = startScale;
        if (title != null) title.localScale = titleScale;

        for (int i = 0; i < renderers.Length; i++)
            renderers[i].sortingOrder = startOrders[i];
        if (glow != null) glow.SetActive(false);

        clicked = false;
    }

    void OnMouseDown()
    {
        if (clicked) return;
        clicked = true;
        foreach (Renderer r in renderers)
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
            currentPage.SetActive(false);
            nextPage.SetActive(true);
        });
        s.timeScale = speed;
    }
}