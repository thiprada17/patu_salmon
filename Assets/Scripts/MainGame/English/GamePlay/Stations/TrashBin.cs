using UnityEngine;

public class TrashBin : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        Cup cup = other.GetComponent<Cup>();

        if (cup != null)
        {
            GameLogger.Info("Trash", "Cup ¶Ù¡·Ôé§Å§¶Ñ§¢ÂÐ");

            cup.DestroyCup();
        }
    }
}