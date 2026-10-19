
using UnityEngine;

public class SelectedCharacterCard : MonoBehaviour
{
    [Header("Card Prefabs")]
    [SerializeField] private GameObject platuCardPrefab;
    [SerializeField] private GameObject salmonCardPrefab;

    [Header("Spawn Parent")]
    [SerializeField] private Transform cardParent;

    private GameObject spawnedCard;

    private void Start()
    {
        ShowSelectedCard();
    }

    public void ShowSelectedCard()
    {
        string character = PlayerPrefs
            .GetString("SelectedCharacter", "")
            .Trim()
            .ToLower();

        GameObject selectedPrefab = character switch
        {
            "platu" => platuCardPrefab,
            "salmon" => salmonCardPrefab,
            _ => null
        };

        if (selectedPrefab == null)
            return;

        Transform parent = cardParent != null
            ? cardParent
            : transform;

        // ลบการ์ดที่ Script นี้เคยสร้าง
        if (spawnedCard != null)
            Destroy(spawnedCard);

        // ซ่อนการ์ดตัวเดิมที่วางไว้ใน Scene
        foreach (Transform child in parent)
        {
            if (child.gameObject != gameObject)
                child.gameObject.SetActive(false);
        }

        // สร้างการ์ดตามตัวละครที่เลือก
        spawnedCard = Instantiate(selectedPrefab, parent, false);
        spawnedCard.transform.localPosition = Vector3.zero;
        spawnedCard.transform.localRotation = Quaternion.identity;
        spawnedCard.transform.localScale = Vector3.one;
        spawnedCard.SetActive(true);
    }
}
