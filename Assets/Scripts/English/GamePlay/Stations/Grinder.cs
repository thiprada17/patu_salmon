using UnityEngine;
using System.Collections;

public class Grinder : MonoBehaviour
{
    private Portfilter currentPortfilter;

    private Vector3 originalPosition;
    private Vector3 portfilterOriginalPosition;

    private bool isGrinding = false;


    private void Start()
    {
        originalPosition = transform.localPosition;
    }


    public void AttachPortfilter(Portfilter portfilter)
    {
        currentPortfilter = portfilter;

        // จำตำแหน่งเดิมของ Portfilter
        portfilterOriginalPosition = portfilter.transform.localPosition;

        Debug.Log("Portfilter attached to Grinder");
    }


    private void OnMouseDown()
    {
        if (currentPortfilter == null)
        {
            Debug.Log("ยังไม่ได้ใส่ Portfilter");
            return;
        }

        if (isGrinding)
        {
            Debug.Log("เครื่องกำลังทำงานอยู่");
            return;
        }

        if (currentPortfilter.hasCoffeeGround)
        {
            Debug.Log("Portfilter มี Coffee Ground อยู่แล้ว!");
            return;
        }

        Debug.Log("เครื่องทำงานอยู่");

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


            float px = Random.Range(-magnitude, magnitude);
            float py = Random.Range(-magnitude, magnitude);

            currentPortfilter.transform.localPosition = portfilterOriginalPosition + new Vector3(px, py, 0);

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

        Debug.Log("Grinding coffee...");

        // Grinder + Portfilter สั่น 3 วิ
        yield return StartCoroutine(Shake());

        // เติม Coffee Ground
        portfilter.AddCoffeeGround();

        Debug.Log("Coffee Ground added!");

        if (draggable != null)
        {
            draggable.enabled = true;
        }

        isGrinding = false;
    }
}