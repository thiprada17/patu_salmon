using UnityEngine;

public class CupDispenser : MonoBehaviour
{
    [Header("Cup")]
    [SerializeField]
    private GameObject cupPrefab;

    [Header("Spawn Point")]
    [SerializeField]
    private Transform spawnPoint;

    private void OnMouseDown()
    {
        DispenseCup();
    }

    public void DispenseCup()
    {
        if (cupPrefab == null)
        {
            Debug.LogError("ยังไม่ได้ใส่ Cup Prefab ใน CupDispenser");
            return;
        }

        if (spawnPoint == null)
        {
            Debug.LogError("ยังไม่ได้ใส่ Spawn Point ใน CupDispenser");
            return;
        }

        GameObject newCup = Instantiate(
            cupPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        Debug.Log("Spawn Cup ใหม่แล้ว: " + newCup.name);
    }
}