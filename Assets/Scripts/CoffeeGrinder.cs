using UnityEngine;
using System.Collections;

public class CoffeeGrinder : MonoBehaviour
{
    public Transform imageToBounce;
    public float bounceScale = 1.3f;   
    public float bounceSpeed = 8f; 

    private Vector3 originalScale;
    private Coroutine bounceRoutine;

    private void Start()
    {
        originalScale = imageToBounce.localScale;
    }

    private void OnMouseDown()
    {
        Debug.Log("จิ้มโดนแล้ว!"); 

        if (bounceRoutine != null)
            StopCoroutine(bounceRoutine);

        bounceRoutine = StartCoroutine(Bounce());
    }

    private IEnumerator Bounce()
    {
        // ขยายขึ้น
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * bounceSpeed;
            imageToBounce.localScale = Vector3.Lerp(originalScale, originalScale * bounceScale, t);
            yield return null;
        }

        // หดกลับ
        t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * bounceSpeed;
            imageToBounce.localScale = Vector3.Lerp(originalScale * bounceScale, originalScale, t);
            yield return null;
        }
    }
}