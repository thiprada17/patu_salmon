
using System.Collections.Generic;
using UnityEngine;

public class CupDispenser : MonoBehaviour
{
    [Header("Cup")]
    [SerializeField] private GameObject cupPrefab;

    [Header("Spawn Point")]
    [SerializeField] private Transform LspawnPoint;
    [SerializeField] private Transform RspawnPoint;

    private readonly List<Cup> spawnedCups = new List<Cup>();

    private readonly Dictionary<Cup, string> spawnSide =
        new Dictionary<Cup, string>();

    private Cup leftCup;
    private Cup rightCup;

    // ป้องกัน Dispenser หลายตัวสร้าง Cup ซ้ำในเฟรมเดียวกัน
    private static int lastSpawnFrame = -1;

    private void OnMouseDown()
    {
        // ห้ามสร้าง Cup ระหว่าง Dialogue หรือ Tutorial
        if (GameplayInputLock.IsLocked)
            return;

        // ป้องกันการสร้าง Cup ซ้ำในเฟรมเดียวกัน
        if (lastSpawnFrame == Time.frameCount)
        {
            GameLogger.Warn("CupDis", "Duplicate spawn blocked.");
            return;
        }

        CleanupCups();

        // สร้าง Cup ได้สูงสุด 2 ใบต่อ Dispenser
        if (spawnedCups.Count >= 2)
        {
            GameLogger.DebugLog("CupDis", "มี Cup ครบ 2 ใบแล้ว");
            return;
        }

        Transform spawnPoint;
        string side;

        if (spawnedCups.Count == 0)
        {
            // Cup ใบแรกเกิดทางซ้าย
            spawnPoint = LspawnPoint;
            side = "L";
        }
        else
        {
            Cup existingCup = spawnedCups[0];

            // ถ้า Cup เดิมอยู่ซ้าย ให้สร้างใบใหม่ทางขวา
            if (leftCup == existingCup ||
                (spawnSide.TryGetValue(existingCup, out string previousSide)
                 && previousSide == "L"))
            {
                spawnPoint = RspawnPoint;
                side = "R";
            }
            // ถ้า Cup เดิมอยู่ขวา ให้สร้างใบใหม่ทางซ้าย
            else if (rightCup == existingCup ||
                     (spawnSide.TryGetValue(existingCup, out previousSide)
                      && previousSide == "R"))
            {
                spawnPoint = LspawnPoint;
                side = "L";
            }
            else
            {
                GameLogger.Warn(
                    "CupDis",
                    "ไม่สามารถระบุ Slot ของ Cup เดิมได้"
                );
                return;
            }
        }

        if (cupPrefab == null || spawnPoint == null)
        {
            GameLogger.Warn(
                "CupDis",
                "Cup Prefab หรือ Spawn Point ยังไม่ได้กำหนด"
            );
            return;
        }

        // บันทึกเฟรมก่อน Instantiate เพื่อป้องกันการเรียกซ้ำ
        lastSpawnFrame = Time.frameCount;

        SpawnCup(spawnPoint, side);
    }

    private void SpawnCup(Transform spawnPoint, string side)
    {
        if (cupPrefab == null || spawnPoint == null)
        {
            GameLogger.Warn(
                "CupDis",
                "Cup Prefab หรือ Spawn Point ยังไม่ได้กำหนด"
            );
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
            GameLogger.Error(
                "CupDis",
                "Cup Prefab ไม่มี Cup Component"
            );

            Destroy(newCup);
            return;
        }

        spawnedCups.Add(cup);
        spawnSide[cup] = side;

        GameLogger.Info(
            "CupDis",
            $"สร้าง Cup สำเร็จ ฝั่ง {side}"
        );
    }

    public void OccupySlot(string slot, Cup cup)
    {
        if (GameplayInputLock.IsLocked || cup == null)
            return;

        if (slot == "L")
        {
            leftCup = cup;
        }
        else if (slot == "R")
        {
            rightCup = cup;
        }
        else
        {
            GameLogger.Warn(
                "CupDis",
                $"ไม่รู้จัก Slot: {slot}"
            );
        }
    }

    public void ReleaseSlot(string slot, Cup cup)
    {
        if (slot == "L" && leftCup == cup)
        {
            leftCup = null;
        }
        else if (slot == "R" && rightCup == cup)
        {
            rightCup = null;
        }
    }

    private void CleanupCups()
    {
        // ลบ Cup ที่ถูกทำลายออกจากรายการ
        spawnedCups.RemoveAll(cup => cup == null);

        // คืน Slot ที่ไม่มี Cup อยู่แล้ว
        if (leftCup == null)
            leftCup = null;

        if (rightCup == null)
            rightCup = null;

        // ลบข้อมูลของ Cup ที่ถูกทำลายออกจาก Dictionary
        List<Cup> keysToRemove = new List<Cup>();

        foreach (Cup cup in spawnSide.Keys)
        {
            if (cup == null)
                keysToRemove.Add(cup);
        }

        foreach (Cup cup in keysToRemove)
            spawnSide.Remove(cup);
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
