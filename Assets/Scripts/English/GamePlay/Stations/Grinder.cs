using UnityEngine;
using System.Collections;

public class Grinder : MonoBehaviour
{
    private Portfilter currentPortfilter;

    private Vector3 originalPosition;

    private bool isGrinding = false;


    private void Start()
    {
        originalPosition = transform.localPosition;
    }


    public void AttachPortfilter(Portfilter portfilter)
    {
        currentPortfilter = portfilter;

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
            float x = Random.Range(
                -magnitude,
                magnitude
            );

            float y = Random.Range(
                -magnitude,
                magnitude
            );

            transform.localPosition =
                originalPosition +
                new Vector3(x, y, 0);

            elapsed += Time.deltaTime;

            yield return null;
        }

        transform.localPosition =
            originalPosition;
    }


    private IEnumerator Grind(Portfilter portfilter)
    {
        isGrinding = true;

        Debug.Log("Grinding coffee...");

        // เครื่องสั่น 3 วินาที
        yield return StartCoroutine(Shake());


        portfilter.AddCoffeeGround();

        Debug.Log("Coffee Ground added!");

        isGrinding = false;
    }
}