using System.Collections.Generic;
using UnityEngine;

public class CupDispenser : MonoBehaviour
{
    [Header("Cup")]
    [SerializeField]
    private GameObject cupPrefab;

    [Header("Spawn Point")]
    [SerializeField]
    private Transform LspawnPoint;

    [SerializeField]
    private Transform RspawnPoint;

    private List<Cup> spawnedCups = new List<Cup>();


    private Cup leftCup;
    private Cup rightCup;


    private Dictionary<Cup, string> spawnSide = new Dictionary<Cup, string>();


    private float SlotRadius()
    {
        float gap = Vector2.Distance(LspawnPoint.position, RspawnPoint.position);
        return gap * 0.4f; // น้อยกว่าครึ่งหนึ่งของระยะ L-R เสมอ จึงไม่ทับซ้อนกัน
    }

    private bool IsSpawnPointBlocked(Transform spawnPoint)
    {
        float radius = SlotRadius();

        foreach (Cup cup in spawnedCups)
        {
            if (cup == null) continue;

            float dist = Vector2.Distance(cup.transform.position, spawnPoint.position);
            if (dist < radius)
                return true;
        }
        return false;
    }

    private void OnMouseDown()
    {
        CleanupCups();

        if (!IsSpawnPointBlocked(LspawnPoint))
        {
            SpawnCup(LspawnPoint, "L");
            return;
        }

        if (!IsSpawnPointBlocked(RspawnPoint))
        {
            SpawnCup(RspawnPoint, "R");
            return;
        }

        GameLogger.DebugLog("CupDis", "ช่องเต็มทั้งสองฝั่ง");
    }

    private void SpawnCup(Transform spawnPoint, string side)
    {
        if (cupPrefab == null)
        {
            GameLogger.Warn("CupDis", "ยังไม่ได้ใส่ Cup Prefab");
            return;
        }


        if (spawnPoint == null)
        {
            GameLogger.Warn("CupDis", "ยังไม่ได้ใส่ Spawn Point");
            return;
        }


        GameObject newCup = Instantiate(
            cupPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        newCup.SetActive(true);
        newCup.transform.SetParent(transform.parent, true);
  

        Cup cup = newCup.GetComponent<Cup>();
        

        if (cup == null)
        {
            GameLogger.Warn("CupDis", "Cup Prefab ไม่มี Cup Component");
            Destroy(newCup);
            return;
        }

        spawnedCups.Add(cup);
        spawnSide[cup] = side;
    }


    public void OccupySlot(string slot, Cup cup)
    {
        if (cup == null)
            return;

        if (slot == "L")
        {
            leftCup = cup;
        }
        else if (slot == "R")
        {
            rightCup = cup;
        }
    }


    public void ReleaseSlot(string slot, Cup cup)
    {
        if (cup == null)  return;


        if (slot == "L")
        {
            // ต้องเป็น Cup ตัวเดียวกัน
            if (leftCup == cup)
            {
                leftCup = null;
            }
        }
        else if (slot == "R")
        {
            // ต้องเป็น Cup ตัวเดียวกัน
            if (rightCup == cup)
            {
                rightCup = null;
            }
        }
    }

    private void CleanupCups()
    {
        // ลบ Cup ที่ถูก Destroy
        spawnedCups.RemoveAll(cup => cup == null);


        // ลบ reference ที่ถูก Destroy
        if (leftCup == null)
        {
            leftCup = null;
        }

        if (rightCup == null)
        {
            rightCup = null;
        }


        // ลบข้อมูล Spawn Side ของ Cup ที่หายไป
        List<Cup> keysToRemove = new List<Cup>();

        foreach (Cup cup in spawnSide.Keys)
        {
            if (cup == null)
            {
                keysToRemove.Add(cup);
            }
        }

        foreach (Cup cup in keysToRemove)
        {
            spawnSide.Remove(cup);
        }

    }

    public bool IsSlotOccupied(string slot)
    {
        if (slot == "L")
            return leftCup != null;

        if (slot == "R")
            return rightCup != null;

        return false;
    }
}