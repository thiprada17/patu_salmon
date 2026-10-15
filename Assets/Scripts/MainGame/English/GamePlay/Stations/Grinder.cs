using UnityEngine;
using System.Collections;

public class Grinder : MonoBehaviour
{
    private Portfilter currentPortfilter;

    private Vector3 originalPosition;
    private Vector3 portfilterOriginalPosition;

    [Header("Coffee Animation")]
    [SerializeField] private Animator coffeeAnimator;
    [SerializeField] private SpriteRenderer coffeeRenderer;

    private bool isGrinding = false;


    private void Start()
    {
        originalPosition = transform.localPosition;

        if (coffeeRenderer != null)
        {
            coffeeRenderer.enabled = false;
        }
    }

    private void PlayCoffeeAnimation()
    {
        if (coffeeRenderer != null)
        {
            coffeeRenderer.enabled = true;
        }

        if (coffeeAnimator != null)
        {
            coffeeAnimator.Play("grinderanimation", 0, 0f);
        }
    }

    public void AttachPortfilter(Portfilter portfilter)
    {
        currentPortfilter = portfilter;

        // จำตำแหน่งเดิมของ Portfilter
        portfilterOriginalPosition = portfilter.transform.localPosition;

        GameLogger.DebugLog("Grinder", "Portfilter attached to Grinder");
    }


    private void OnMouseDown()
    {
        if (currentPortfilter == null)
        {
            GameLogger.Warn("Grinder", "ยังไม่ได้ใส่ Portfilter");
            return;
        }

        if (isGrinding)
        {
            return;
        }

        if (currentPortfilter.hasCoffeeGround)
        {
            GameLogger.Warn("Grinder", "Portfilter มี Coffee Ground อยู่แล้ว!");
            return;
        }

        StartCoroutine(Grind(currentPortfilter));
    }


    private IEnumerator Shake()
    {
        float duration = 3f;
        float magnitude = 0.02f;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-magnitude, magnitude);
            float y = Random.Range(-magnitude, magnitude);

            transform.localPosition = originalPosition + new Vector3(x, y, 0);

            currentPortfilter.transform.localPosition = portfilterOriginalPosition + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;

            yield return null;
        }

        // กลับตำแหน่งเดิม
        transform.localPosition = originalPosition;

        if (currentPortfilter != null)
        {
            currentPortfilter.transform.localPosition =
                portfilterOriginalPosition;
        }
    }


    private IEnumerator Grind(Portfilter portfilter)
    {
        isGrinding = true;

        DraggableItem draggable =
            portfilter.GetComponent<DraggableItem>();

        if (draggable != null)
        {
            draggable.enabled = false;
        }

        GameLogger.DebugLog("Grinder", "Grinding coffee...");

        // เล่น Animation กาแฟ
        PlayCoffeeAnimation();
        yield return new WaitForSeconds(0f);
        // Grinder + Portfilter สั่น 3 วิ
        yield return StartCoroutine(Shake());

        // เติม Coffee Ground
        portfilter.AddCoffeeGround();

        if (coffeeRenderer != null)
        {
            coffeeRenderer.enabled = false;
        }

        GameLogger.Info("Grinder", "Coffee Ground added!");

        if (draggable != null)
        {
            draggable.enabled = true;
        }

        isGrinding = false;
    }
}