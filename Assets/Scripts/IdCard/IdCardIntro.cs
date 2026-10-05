using UnityEngine;
using DG.Tweening;
using TMPro;

public class IdCardIntro : MonoBehaviour
{
    public GameObject salmonCard, platuCard;
    public GameObject inputField;
    public Transform enterButton;
    [Range(0.1f, 2f)] public float speed = 0.5f;
    [Range(0.1f, 2f)] public float swingSpeed = 0.5f;

    Transform canvasT, anchor;

    void Start()
    {
        bool isSalmon = GameData.selectedCharacter != "platu";
        salmonCard.SetActive(isSalmon);
        platuCard.SetActive(!isSalmon);
        Transform card = (isSalmon ? salmonCard : platuCard).transform;
        float sx = card.localScale.x;

        if (inputField != null)
        {
            canvasT = inputField.GetComponentInParent<Canvas>(true).transform;
            anchor = new GameObject("inputAnchor").transform;
            anchor.SetPositionAndRotation(canvasT.position, canvasT.rotation);
            anchor.localScale = canvasT.lossyScale;
            anchor.SetParent(card, true);
            inputField.SetActive(false);
        }
        enterButton.localScale = Vector3.zero;

        Sequence s = DOTween.Sequence();
        s.timeScale = speed;
        s.Append(card.DOScaleX(0, 0.15f));
        s.AppendCallback(() =>
        {
            Transform front = card.Find("front");
            if (front != null) front.gameObject.SetActive(false);
            if (inputField != null) inputField.SetActive(true);
        });
        s.Append(card.DOScaleX(sx, 0.15f));
        s.AppendCallback(() =>
        {
            Sequence swing = DOTween.Sequence();
            swing.timeScale = swingSpeed;
            swing.Append(card.DORotate(new Vector3(0, 0, 2), 0.35f).SetEase(Ease.OutSine));
            swing.Append(card.DORotate(new Vector3(0, 0, -1), 0.55f).SetEase(Ease.InOutSine));
            swing.Append(card.DORotate(Vector3.zero, 0.45f).SetEase(Ease.InOutSine));
        });
        s.Append(enterButton.DOScale(1, 0.3f).SetEase(Ease.OutBack));
        s.OnComplete(() =>
        {
            if (inputField != null)
            {
                var field = inputField.GetComponentInChildren<TMP_InputField>(true);
                if (field != null) field.ActivateInputField();
            }
        });
    }

    void LateUpdate()
    {
        if (anchor == null) return;
        canvasT.SetPositionAndRotation(anchor.position, anchor.rotation);
        canvasT.localScale = anchor.lossyScale;
    }
}