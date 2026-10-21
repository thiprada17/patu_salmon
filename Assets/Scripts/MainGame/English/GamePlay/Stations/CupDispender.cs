
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
        if (lastSpawnFrame == Time.frameCount)
        {
            GameLogger.Warn("CupDis", "Duplicate spawn blocked.");
            return;
        }

        CleanupCups();

        if (spawnedCups.Count >= 2)
        {
            GameLogger.DebugLog("CupDis", "มี Cup ครบ 2 ใบแล้ว");
            return;
        }

        Transform spawnPoint;
        string side;

        if (spawnedCups.Count == 0)
        {
            spawnPoint = LspawnPoint;
            side = "L";
        }
        else
        {
            Cup existingCup = spawnedCups[0];

            if (leftCup == existingCup ||
                (spawnSide.TryGetValue(existingCup, out string previousSide)
                 && previousSide == "L"))
            {
                spawnPoint = RspawnPoint;
                side = "R";
            }
            else if (rightCup == existingCup ||
                     (spawnSide.TryGetValue(existingCup, out previousSide)
                      && previousSide == "R"))
            {
                spawnPoint = LspawnPoint;
                side = "L";
            }
            else
            {
                GameLogger.Warn("CupDis", "ไม่สามารถระบุ Slot ของ Cup เดิมได้");
                return;
            }
        }

        if (cupPrefab == null || spawnPoint == null)
        {
            GameLogger.Warn("CupDis", "Cup Prefab หรือ Spawn Point ยังไม่ได้กำหนด");
            return;
        }

        // บันทึกเฟรมก่อน Instantiate เพื่อป้องกันการเรียกซ้ำ
        lastSpawnFrame = Time.frameCount;

        SpawnCup(spawnPoint, side);
    }

    private void SpawnCup(Transform spawnPoint, string side)
    {
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
            GameLogger.Error("CupDis", "Cup Prefab ไม่มี Cup Component");
            Destroy(newCup);
            return;
        }

        spawnedCups.Add(cup);
        spawnSide[cup] = side;

        GameLogger.Info("CupDis", $"Spawned Cup on {side} side.");
    }

    public void OccupySlot(string slot, Cup cup)
    {
        if (cup == null) return;

        if (slot == "L")
            leftCup = cup;
        else if (slot == "R")
            rightCup = cup;
    }

    public void ReleaseSlot(string slot, Cup cup)
    {
        if (slot == "L" && leftCup == cup)
            leftCup = null;
        else if (slot == "R" && rightCup == cup)
            rightCup = null;
    }

    private void CleanupCups()
    {
        spawnedCups.RemoveAll(cup => cup == null);

        if (leftCup == null) leftCup = null;
        if (rightCup == null) rightCup = null;

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
        if (slot == "L") return leftCup != null;
        if (slot == "R") return rightCup != null;

        return false;
    }
}