using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

public class HandleBounce : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public float pressedScale = 1.25f;
    public float time = 0.15f;

    Transform handle;
    Vector3 baseScale;

    void Awake()
    {
        handle = GetComponent<Slider>().handleRect;
        baseScale = handle.localScale;
    }

    public void OnPointerDown(PointerEventData e)
    {
        handle.DOKill();
        handle.DOScale(baseScale * pressedScale, time).SetEase(Ease.OutBack).SetUpdate(true);
    }

    public void OnPointerUp(PointerEventData e)
    {
        handle.DOKill();
        handle.DOScale(baseScale, time * 1.5f).SetEase(Ease.OutBack).SetUpdate(true);
    }

    void OnDisable()
    {
        if (handle == null) return;
        handle.DOKill();
        handle.localScale = baseScale;
    }
}