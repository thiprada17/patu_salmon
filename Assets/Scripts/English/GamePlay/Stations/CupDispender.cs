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
        CleanupCups();


        if (spawnedCups.Count >= 2)
        {
            Debug.Log("มี Cup ครบ 2 ใบแล้ว");
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
            // Left มีแล้ว → Spawn Right
            SpawnCup(RspawnPoint, "R");
            return;
        }


        if (rightCup == existingCup)
        {
            // Right มีแล้ว → Spawn Left
            SpawnCup(LspawnPoint, "L");
            return;
        }


        if (spawnSide.ContainsKey(existingCup))
        {
            string previousSide = spawnSide[existingCup];


            if (previousSide == "L")
            {
                // Cup เดิมเกิดจาก Left
                // → Cup ใหม่เกิด Right
                SpawnCup(RspawnPoint, "R");
                return;
            }


            if (previousSide == "R")
            {
                // Cup เดิมเกิดจาก Right
                // → Cup ใหม่เกิด Left
                SpawnCup(LspawnPoint, "L");
                return;
            }
        }


        Debug.LogWarning("ไม่สามารถระบุ Slot ของ Cup เดิมได้");
    }

    private void SpawnCup(Transform spawnPoint, string side)
    {
        if (cupPrefab == null)
        {
            Debug.LogError("ยังไม่ได้ใส่ Cup Prefab");
            return;
        }


        if (spawnPoint == null)
        {
            Debug.LogError("ยังไม่ได้ใส่ Spawn Point");
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
            Debug.LogError("Cup Prefab ไม่มี Cup Component");

            Destroy(newCup);

            return;
        }


        spawnedCups.Add(cup);

        spawnSide[cup] = side;


        Debug.Log(
            "Spawn Cup: " + cup.name + " Spawn Side = " + side + " Total Cup = " + spawnedCups.Count
        );
    }


    public void OccupySlot(string slot, Cup cup)
    {
        if (cup == null)
            return;


        if (slot == "L")
        {
            leftCup = cup;

            Debug.Log(
                "Dispenser: LEFT occupied by " + cup.name
            );
        }
        else if (slot == "R")
        {
            rightCup = cup;

            Debug.Log(
                "Dispenser: RIGHT occupied by " + cup.name
            );
        }
    }


    public void ReleaseSlot(string slot, Cup cup)
    {
        if (cup == null)
            return;


        if (slot == "L")
        {
            // ต้องเป็น Cup ตัวเดียวกัน
            if (leftCup == cup)
            {
                leftCup = null;

                Debug.Log("Dispenser: LEFT released");
            }
        }
        else if (slot == "R")
        {
            // ต้องเป็น Cup ตัวเดียวกัน
            if (rightCup == cup)
            {
                rightCup = null;

                Debug.Log("Dispenser: RIGHT released");
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
}