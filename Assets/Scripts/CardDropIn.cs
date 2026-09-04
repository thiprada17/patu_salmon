using UnityEngine;
using System.Collections;

public class CardDropIn : MonoBehaviour
{
    [SerializeField] float dropFrom = 12f;      // เริ่มสูงเหนือจอเท่าไหร่
    [SerializeField] float dropTime = 0.7f;
    [SerializeField] float swingAngle = 6f;     // แกว่งซ้ายขวากี่องศา
    [SerializeField] float swingTime = 0.5f;

    Vector3 target;

    void Start()
    {
        target = transform.position;
        transform.position = target + Vector3.up * dropFrom;
        StartCoroutine(Drop());
    }

    IEnumerator Drop()
    {
        float e = 0f;
        Vector3 from = transform.position;

        while (e < dropTime)
        {
            e += Time.deltaTime;
            float p = e / dropTime;
            p = 1f - Mathf.Pow(1f - p, 3f);          // ตกเร็ว แล้วชะลอ
            transform.position = Vector3.Lerp(from, target, p);
            yield return null;
        }
        transform.position = target;

        yield return Swing();
    }

    IEnumerator Swing()
    {
        float e = 0f;
        while (e < swingTime)
        {
            e += Time.deltaTime;
            float p = e / swingTime;
            float decay = 1f - p;                     // แกว่งค่อย ๆ เบาลง
            float angle = Mathf.Sin(p * Mathf.PI * 4f) * swingAngle * decay;
            transform.rotation = Quaternion.Euler(0, 0, angle);
            yield return null;
        }
        transform.rotation = Quaternion.identity;
    }
}