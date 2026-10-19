
using System.Collections;
using UnityEngine;
using TMPro;

public class DayManager : MonoBehaviour
{
    [Header("Day Settings")]
    [SerializeField, Min(1)] private int dayDurationSeconds = 60;
    [SerializeField] private TMP_Text timerText;

    private bool isDayEnding;
    private float timeRemaining;

    private void Start()
    {
        StartCoroutine(WaitForStats());
    }

    private IEnumerator WaitForStats()
    {
        while (CoinManager.Instance == null)
            yield return null;

        while (!CoinManager.Instance.StatsLoaded &&
               !CoinManager.Instance.StatsLoadFailed)
        {
            yield return null;
        }

        if (CoinManager.Instance.StatsLoadFailed)
        {
            GameLogger.Error(
                "DayManager",
                "โหลดข้อมูลผู้เล่นไม่สำเร็จ จึงยังไม่เริ่มวัน"
            );
            yield break;
        }

        timeRemaining = dayDurationSeconds;
        UpdateTimerText();

        GameLogger.Info("DayManager", "โหลดข้อมูลแล้ว เริ่มวันใหม่");
    }

    private void Update()
    {
        if (isDayEnding || timeRemaining <= 0f)
            return;

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            UpdateTimerText();

            GameLogger.Info("DayManager", "หมดเวลา เริ่ม EndDay()");
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

        CoinManager.Instance.RefreshNavbar();

        timeRemaining = dayDurationSeconds;
        isDayEnding = false;

        GameLogger.Info("DayManager", "เริ่มวันถัดไปแล้ว");
    }
}
