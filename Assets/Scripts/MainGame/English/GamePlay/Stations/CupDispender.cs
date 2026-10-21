
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

    private Dictionary<Cup, string> spawnSide =
        new Dictionary<Cup, string>();

    private void OnMouseDown()
    {
        // Block cup spawning during dialogue/tutorial.
        if (GameplayInputLock.IsLocked)
            return;

        CleanupCups();

        if (spawnedCups.Count >= 2)
        {
            GameLogger.DebugLog(
                "CupDis",
                "มี Cup ครบ 2 ใบแล้ว"
            );
            return;
        }

        if (spawnedCups.Count == 0)
        {
            SpawnCup(LspawnPoint, "L");
            return;
        }

        Cup existingCup = spawnedCups[0];

        if (leftCup == existingCup)
        {
            // Left already has a cup; spawn on the right.
            SpawnCup(RspawnPoint, "R");
            return;
        }

        if (rightCup == existingCup)
        {
            // Right already has a cup; spawn on the left.
            SpawnCup(LspawnPoint, "L");
            return;
        }

        if (spawnSide.ContainsKey(existingCup))
        {
            string previousSide = spawnSide[existingCup];

            if (previousSide == "L")
            {
                SpawnCup(RspawnPoint, "R");
                return;
            }

            if (previousSide == "R")
            {
                SpawnCup(LspawnPoint, "L");
                return;
            }
        }

        GameLogger.Warn(
            "CupDis",
            "ไม่สามารถระบุ Slot ของ Cup เดิมได้"
        );
    }

    private void SpawnCup(Transform spawnPoint, string side)
    {
        if (cupPrefab == null)
        {
            GameLogger.Warn(
                "CupDis",
                "ยังไม่ได้ใส่ Cup Prefab"
            );
            return;
        }

        if (spawnPoint == null)
        {
            GameLogger.Warn(
                "CupDis",
                "ยังไม่ได้ใส่ Spawn Point"
            );
            return;
        }

        GameObject newCup = Instantiate(
            cupPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        Cup cup = newCup.GetComponent<Cup>();

        if (cup == null)
        {
            GameLogger.Warn(
                "CupDis",
                "Cup Prefab ไม่มี Cup Component"
            );
            Destroy(newCup);
            return;
        }

        spawnedCups.Add(cup);
        spawnSide[cup] = side;
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
    }

    public void ReleaseSlot(string slot, Cup cup)
    {
        if (cup == null)
            return;

        if (slot == "L")
        {
            if (leftCup == cup)
                leftCup = null;
        }
        else if (slot == "R")
        {
            if (rightCup == cup)
                rightCup = null;
        }
    }

    private void CleanupCups()
    {
        // Remove destroyed cups.
        spawnedCups.RemoveAll(cup => cup == null);

        // Clear references to destroyed cups.
        if (leftCup == null)
            leftCup = null;

        if (rightCup == null)
            rightCup = null;

        // Remove spawn-side data for destroyed cups.
        List<Cup> keysToRemove = new List<Cup>();

        foreach (Cup cup in spawnSide.Keys)
        {
            if (cup == null)
                keysToRemove.Add(cup);
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
