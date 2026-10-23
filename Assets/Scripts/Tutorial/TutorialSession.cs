using UnityEngine;
using UnityEngine.SceneManagement;

public static class TutorialSession
{
    public static bool IsInitialized { get; private set; }
    public static bool IsFirstTimePlayer { get; private set; }
    public static bool IsTutorialActive { get; private set; }

    public static bool ShowCongratulationsOnMap { get; private set; }

    private static int tutorialCustomersServed;
    private const int TutorialCustomerLimit = 3;

    // True when the third tutorial customer has been served
    // and the tutorial is waiting for that customer to leave.
    public static bool IsFinalTutorialCustomer =>
        IsTutorialActive &&
        tutorialCustomersServed >= TutorialCustomerLimit;

    public static void Initialize(int workDays)
    {
        IsInitialized = true;
        IsFirstTimePlayer = workDays == 0;
        IsTutorialActive = false;
        ShowCongratulationsOnMap = false;
        tutorialCustomersServed = 0;

        Debug.Log(
            $"[TutorialSession] work_days={workDays}, " +
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

        Debug.Log("[TutorialSession] Tutorial started.");
    }

    public static void RegisterTutorialCustomerServed()
    {
        if (!IsTutorialActive)
            return;

        // Do not count beyond the tutorial limit.
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

        IsTutorialActive = false;
        IsFirstTimePlayer = false;
        ShowCongratulationsOnMap = true;

        Debug.Log(
            "[TutorialSession] Final customer left. Returning to map."
        );

        SceneManager.LoadScene("map");
    }

    public static void CompleteTutorial()
    {
        IsTutorialActive = false;

        Debug.Log("[TutorialSession] Tutorial deactivated.");
    }

    public static void ConsumeCongratulationsPopup()
    {
        ShowCongratulationsOnMap = false;
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