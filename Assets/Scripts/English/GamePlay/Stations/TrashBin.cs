using UnityEngine;

public class TrashBin : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        Cup cup = other.GetComponent<Cup>();

        if (cup != null)
        {
            Debug.Log("Cup ¶Ù¡·Ôé§Å§¶Ñ§¢ÂÐ");

            cup.DestroyCup();
        }
    }
}