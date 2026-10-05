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
    Sequence intro, swing;

    bool saved;
    Vector3 canvasPos, canvasScale, salmonScale, platuScale;
    Quaternion canvasRot, salmonRot, platuRot;

    void SaveStart()
    {
        saved = true;
        salmonScale = salmonCard.transform.localScale;
        salmonRot = salmonCard.transform.localRotation;
        platuScale = platuCard.transform.localScale;
        platuRot = platuCard.transform.localRotation;

        if (inputField != null)
        {
            canvasT = inputField.GetComponentInParent<Canvas>(true).transform;
            canvasPos = canvasT.localPosition;
            canvasRot = canvasT.localRotation;
            canvasScale = canvasT.localScale;
        }
    }

    void OnEnable()
    {
        if (!saved) SaveStart();

        salmonCard.transform.localScale = salmonScale;
        salmonCard.transform.localRotation = salmonRot;
        platuCard.transform.localScale = platuScale;
        platuCard.transform.localRotation = platuRot;
        if (canvasT != null)
        {
            canvasT.localPosition = canvasPos;
            canvasT.localRotation = canvasRot;
            canvasT.localScale = canvasScale;
        }

        if (inputField != null)
        {
            var oldField = inputField.GetComponentInChildren<TMP_InputField>(true);
            if (oldField != null) oldField.text = "";
        }
        bool isSalmon = GameData.selectedCharacter != "platu";
        salmonCard.SetActive(isSalmon);
        platuCard.SetActive(!isSalmon);
        Transform card = (isSalmon ? salmonCard : platuCard).transform;
        float sx = card.localScale.x;

        Transform front = card.Find("front");
        if (front != null) front.gameObject.SetActive(true);

        if (inputField != null)
        {
            anchor = new GameObject("inputAnchor").transform;
            anchor.SetPositionAndRotation(canvasT.position, canvasT.rotation);
            anchor.localScale = canvasT.lossyScale;
            anchor.SetParent(card, true);
            inputField.SetActive(false);
        }
        enterButton.localScale = Vector3.zero;

        intro = DOTween.Sequence();
        intro.timeScale = speed;
        intro.Append(card.DOScaleX(0, 0.15f));
        intro.AppendCallback(() =>
        {
            if (front != null) front.gameObject.SetActive(false);
            if (inputField != null) inputField.SetActive(true);
        });
        intro.Append(card.DOScaleX(sx, 0.15f));
        intro.AppendCallback(() =>
        {
            swing = DOTween.Sequence();
            swing.timeScale = swingSpeed;
            swing.Append(card.DORotate(new Vector3(0, 0, 2), 0.35f).SetEase(Ease.OutSine));
            swing.Append(card.DORotate(new Vector3(0, 0, -1), 0.55f).SetEase(Ease.InOutSine));
            swing.Append(card.DORotate(Vector3.zero, 0.45f).SetEase(Ease.InOutSine));
        });
        intro.Append(enterButton.DOScale(1, 0.3f).SetEase(Ease.OutBack));
        intro.OnComplete(() =>
        {
            if (inputField != null)
            {
                var field = inputField.GetComponentInChildren<TMP_InputField>(true);
                if (field != null) field.ActivateInputField();
            }
        });
    }

    void OnDisable()
    {
        intro?.Kill();
        swing?.Kill();
        if (anchor != null) Destroy(anchor.gameObject);
        anchor = null;
    }

    void LateUpdate()
    {
        if (anchor == null) return;
        canvasT.SetPositionAndRotation(anchor.position, anchor.rotation);
        canvasT.localScale = anchor.lossyScale;
    }
}