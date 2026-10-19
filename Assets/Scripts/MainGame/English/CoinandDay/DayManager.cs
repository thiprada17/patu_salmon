
using System.Collections;
using UnityEngine;
using TMPro;

public class DayManager : MonoBehaviour
{
    [Header("Day Settings")]
    [SerializeField, Min(1)] private int dayDurationSeconds = 30;
    [SerializeField] private TMP_Text timerText;

    // ป้องกันการจบวันซ้ำ
    private bool isDayEnding;
    private float timeRemaining;

    private void Start()
    {
        timeRemaining = dayDurationSeconds;

        GameLogger.Info("DayManager", $"เริ่มวันใหม่ ระยะเวลา {dayDurationSeconds} วินาที");

        UpdateTimerText();
    }

    private void Update()
    {
        // หยุดนับเมื่อกำลังจบวัน
        if (isDayEnding)
            return;

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            UpdateTimerText();
            GameLogger.Info("DayManager", "หมดเวลา เริ่ม EndDay()");
            // จบวันอัตโนมัติเมื่อหมดเวลา
            StartCoroutine(EndDay());
            return;
        }

        UpdateTimerText();
    }

    private void UpdateTimerText()
    {
        if (timerText == null)
            return;

        int seconds = Mathf.CeilToInt(timeRemaining);
        int minutes = seconds / 60;
        seconds %= 60;

        timerText.text = $"{minutes:00}:{seconds:00}";
    }

        private IEnumerator EndDay()
    {
        if (isDayEnding)
            yield break;

        isDayEnding = true;
        GameLogger.Info("DayManager", "กำลังเรียก FinishWorkDay()");

        if (CoinManager.Instance == null)
        {
            GameLogger.Error("DayManager", "ไม่พบ CoinManager.Instance");
            yield break;
        }

        bool saveFinished = false;
        bool saveSucceeded = false;

        CoinManager.Instance.FinishWorkDay(success =>
        {
            saveSucceeded = success;
            saveFinished = true;
            GameLogger.Info("DayManager", $"ผลบันทึกวัน: {success}");
        });

        while (!saveFinished)
            yield return null;

        if (!saveSucceeded)
        {
            GameLogger.Error("DayManager", "บันทึกวันไม่สำเร็จ");
            yield break;
        }
        GameLogger.Info("DayManager", "บันทึกวันสำเร็จ");

        // อัปเดต Navbar หลังรับค่าล่าสุดจาก Backend
        CoinManager.Instance.RefreshNavbar();

        timeRemaining = dayDurationSeconds;
        isDayEnding = false;

        GameLogger.Info("DayManager", "เริ่มวันถัดไปแล้ว");
    }
}
