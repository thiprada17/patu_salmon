using UnityEngine;
using UnityEngine.SceneManagement;

public static class TutorialSession
{
    private const string TutorialCompletedKey =
        "TutorialCompleted";

    public static bool IsInitialized { get; private set; }
    public static bool IsFirstTimePlayer { get; private set; }
    public static bool IsTutorialActive { get; private set; }

    public static bool ShowCongratulationsOnMap { get; private set; }

    // True if the tutorial was completed in a previous session
    // or during the current session.
    public static bool HasCompletedTutorial =>
        PlayerPrefs.GetInt(TutorialCompletedKey, 0) == 1;

    private static int tutorialCustomersServed;
    private const int TutorialCustomerLimit = 3;

    public static GameObject TutorialPanel;
    public static GameObject TutorialPanelancle;

    public static bool IsFinalTutorialCustomer =>
        IsTutorialActive &&
        tutorialCustomersServed >= TutorialCustomerLimit;

    public static void Initialize(int workDays)
    {
        IsInitialized = true;

        // Show the tutorial only if it has not been completed
        // and the player has not completed a workday.
        IsFirstTimePlayer =
            !HasCompletedTutorial && workDays == 0;

        IsTutorialActive = false;
        ShowCongratulationsOnMap = false;
        tutorialCustomersServed = 0;

        Debug.Log(
            $"[TutorialSession] work_days={workDays}, " +
            $"HasCompletedTutorial={HasCompletedTutorial}, " +
            $"FirstTimePlayer={IsFirstTimePlayer}"
        );
    }

    public static void StartTutorial()
    {
        if (!IsInitialized || !IsFirstTimePlayer)
            return;

        if (IsTutorialActive)
            return;

        tutorialCustomersServed = 0;
        IsTutorialActive = true;

        GameplayInputLock.Unlock();
        Time.timeScale = 1f;

        Debug.Log("[TutorialSession] Tutorial started.");
    }

    public static void RegisterTutorialCustomerServed()
    {
        if (!IsTutorialActive)
            return;

        if (tutorialCustomersServed >= TutorialCustomerLimit)
            return;

        tutorialCustomersServed++;

        Debug.Log(
            $"[TutorialSession] Customers served: " +
            $"{tutorialCustomersServed}/{TutorialCustomerLimit}"
        );

        if (tutorialCustomersServed >= TutorialCustomerLimit)
        {
            Debug.Log(
                "[TutorialSession] Final customer served. " +
                "Waiting for customer to leave."
            );
        }
    }

    public static void FinishTutorialAfterLastCustomerLeaves()
    {
        if (!IsTutorialActive)
            return;

        if (tutorialCustomersServed < TutorialCustomerLimit)
        {
            Debug.LogWarning(
                "[TutorialSession] Cannot finish tutorial yet. " +
                "Three customers have not been served."
            );
            return;
        }

        // Save completion permanently on this device.
        PlayerPrefs.SetInt(TutorialCompletedKey, 1);
        PlayerPrefs.Save();

        IsTutorialActive = false;
        IsFirstTimePlayer = false;
        ShowCongratulationsOnMap = true;

        Debug.Log(
            "[TutorialSession] Tutorial completed and saved. " +
            "Returning to map."
        );

        if (TutorialPanel != null && TutorialPanelancle != null)
        {
            TutorialPanel.SetActive(false);
            TutorialPanelancle.SetActive(false);
        }

        SceneManager.LoadScene("map");
    }

    public static void CompleteTutorial()
    {
        // This method only deactivates the tutorial.
        // It does not mark it completed because the final customer
        // may not have left yet.
        IsTutorialActive = false;

        Debug.Log("[TutorialSession] Tutorial deactivated.");
    }

    public static void ConsumeCongratulationsPopup()
    {
        ShowCongratulationsOnMap = false;
    }

    public static void ResetTutorialCompletion()
    {
        // Use only when intentionally allowing the tutorial
        // to be played again, such as during testing.
        PlayerPrefs.DeleteKey(TutorialCompletedKey);
        PlayerPrefs.Save();

        Debug.Log("[TutorialSession] Tutorial completion reset.");
    }

    public static void ResetSession()
    {
        IsInitialized = false;
        IsFirstTimePlayer = false;
        IsTutorialActive = false;
        ShowCongratulationsOnMap = false;
        tutorialCustomersServed = 0;

        Debug.Log("[TutorialSession] Session reset.");
    }
}