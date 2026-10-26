using System.Collections;
using UnityEngine;
using TMPro;

public class DayManager : MonoBehaviour
{
    [Header("Day Settings")]
    [SerializeField, Min(1)] private int dayDurationSeconds = 30;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private GameObject TutorialPanel;
    [SerializeField] private GameObject TutorialPanelancle;

    //Shift Manager
    [Header("Shift References")]
    [SerializeField] private CustomerSpawner customerSpawner;
    [SerializeField] private ShiftEnd shiftEnd;

    [Header("Summary Settings")]
    [SerializeField, Min(1)] private int summaryEveryShifts = 10;

    private bool isDayEnding;
    private float timeRemaining;

    private void Start()
    {
        timeRemaining = dayDurationSeconds;

        if (customerSpawner == null)
            customerSpawner = FindFirstObjectByType<CustomerSpawner>();

        if (customerSpawner == null)
            Debug.LogError("[DayManager] CustomerSpawner not found.");

        if (shiftEnd == null)
            Debug.LogError("[DayManager] ShiftEnd is not assigned.");

        if (TutorialPanel != null)
            TutorialPanel.SetActive(TutorialSession.IsTutorialActive);

        if (TutorialPanelancle != null)
            TutorialPanelancle.SetActive(TutorialSession.IsTutorialActive);

        // Start spawning for the initial shift.
        if (customerSpawner != null)
            customerSpawner.BeginShift();

        GameLogger.Info(
            "DayManager",
            $"Starting shift: {dayDurationSeconds} seconds."
        );

        UpdateTimerText();
    }

    private void Update()
    {
        //Tutorial Session is active
        if (TutorialSession.IsTutorialActive)
        {
            if (timerText != null)
                timerText.text = "TUTORIAL";

            if (TutorialPanel != null)
                TutorialPanel.SetActive(true);

            if (TutorialPanelancle != null)
                TutorialPanelancle.SetActive(true);

            return;
        }

        if (TutorialPanel != null && TutorialPanel.activeSelf)
            TutorialPanel.SetActive(false);


        if (TutorialPanelancle != null && TutorialPanelancle.activeSelf)
            TutorialPanelancle.SetActive(false);

        if (isDayEnding)
            return;

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            UpdateTimerText();

            StartCoroutine(EndDay());
            return;
        }

        UpdateTimerText();
    }

    private void UpdateTimerText()
    {
        if (timerText == null)
            return;

        if (TutorialSession.IsTutorialActive)
        {
            timerText.text = "TUTORIAL";
            return;
        }

        int totalSeconds = Mathf.CeilToInt(timeRemaining);
        timerText.text =
            $"{totalSeconds / 60:00}:{totalSeconds % 60:00}";
    }


    private IEnumerator EndDay()
    {
        if (isDayEnding)
            yield break;

        // Do not end the shift during the tutorial.
        if (TutorialSession.IsTutorialActive)
            yield break;

        isDayEnding = true;

        // Stop customer spawning when the timer reaches zero.
        if (customerSpawner != null)
            customerSpawner.StopSpawning();

        if (CoinManager.Instance == null)
        {
            GameLogger.Error("DayManager", "CoinManager.Instance missing.");
            isDayEnding = false;
            yield break;
        }

        bool saveFinished = false;
        bool saveSucceeded = false;

        CoinManager.Instance.FinishWorkDay(success =>
        {
            saveSucceeded = success;
            saveFinished = true;

            GameLogger.Info(
                "DayManager",
                $"Shift save result: {success}"
            );
        });

        while (!saveFinished)
            yield return null;

        if (!saveSucceeded)
        {
            GameLogger.Error("DayManager", "Failed to save shift.");
            isDayEnding = false;
            yield break;
        }

        CoinManager.Instance.RefreshNavbar();

        int completedShifts = CoinManager.Instance.WorkDays;

        GameLogger.Info(
            "DayManager",
            $"Completed shifts: {completedShifts}"
        );

        bool showSummary =
            completedShifts > 0 &&
            completedShifts % summaryEveryShifts == 0;

        GameLogger.Info(
            "DayManager",
            $"Show summary: {showSummary}, " +
            $"Interval: {summaryEveryShifts}"
        );

        if (showSummary)
        {
            if (shiftEnd == null)
            {
                Debug.LogError(
                    "[DayManager] ShiftEnd reference is missing!"
                );
            }
            else
            {
                shiftEnd.gameObject.SetActive(true);
                shiftEnd.ShowSummary();

                // Wait for the Continue button.
                while (shiftEnd != null &&
                       shiftEnd.IsShowingSummary)
                {
                    yield return null;
                }
            }
        }

        // Prepare the next shift.
        Time.timeScale = 1f;
        timeRemaining = dayDurationSeconds;

        if (customerSpawner != null)
            customerSpawner.BeginShift();

        isDayEnding = false;

        GameLogger.Info(
            "DayManager",
            "Next shift started."
        );
    }

}
