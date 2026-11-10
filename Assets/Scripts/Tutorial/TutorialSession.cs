using UnityEngine;

public static class TutorialSession
{
    private const string PlayerIdKey = "UserID";
    private const string TutorialCompletedKeyPrefix = "TutorialCompleted_";

    public static bool IsInitialized { get; private set; }
    public static bool IsFirstTimePlayer { get; private set; }
    public static bool IsTutorialActive { get; private set; }
    public static bool ShowCongratulationsOnMap { get; private set; }

    private static int tutorialCustomersServed;
    private const int TutorialCustomerLimit = 3;

    public static GameObject TutorialPanel;
    public static GameObject TutorialPanelancle;

    public static bool IsFinalTutorialCustomer =>
        IsTutorialActive &&
        tutorialCustomersServed >= TutorialCustomerLimit;

    public static bool HasCompletedTutorial
    {
        get
        {
            string playerId = GetPlayerId();

            if (string.IsNullOrEmpty(playerId))
                return false;

            return PlayerPrefs.GetInt(
                GetTutorialCompletedKey(playerId),
                0
            ) == 1;
        }
    }

    public static void Initialize(int workDays)
    {
        IsInitialized = true;

        string playerId = GetPlayerId();

        IsFirstTimePlayer =
            !string.IsNullOrEmpty(playerId) &&
            !HasCompletedTutorial &&
            workDays == 0;

        IsTutorialActive = false;
        ShowCongratulationsOnMap = false;
        tutorialCustomersServed = 0;

        Debug.Log(
            $"[TutorialSession] playerId={playerId}, " +
            $"work_days={workDays}, " +
            $"HasCompletedTutorial={HasCompletedTutorial}, " +
            $"FirstTimePlayer={IsFirstTimePlayer}"
        );
    }

    public static void StartTutorial()
    {
        if (!IsInitialized || !IsFirstTimePlayer || IsTutorialActive)
            return;

        tutorialCustomersServed = 0;
        IsTutorialActive = true;

        GameplayInputLock.Unlock();
        Time.timeScale = 1f;

        Debug.Log("[TutorialSession] Tutorial started.");
    }

    public static void RegisterTutorialCustomerServed()
    {
        if (!IsTutorialActive ||
            tutorialCustomersServed >= TutorialCustomerLimit)
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

        string playerId = GetPlayerId();

        if (string.IsNullOrEmpty(playerId))
        {
            Debug.LogError(
                "[TutorialSession] Cannot save completion: UserID is missing."
            );
            return;
        }

        PlayerPrefs.SetInt(GetTutorialCompletedKey(playerId), 1);
        PlayerPrefs.Save();

        IsTutorialActive = false;
        IsFirstTimePlayer = false;
        ShowCongratulationsOnMap = true;

        Debug.Log(
            $"[TutorialSession] Tutorial completed and saved for player " +
            $"{playerId}. Returning to map."
        );

        if (TutorialPanel != null)
            TutorialPanel.SetActive(false);

        if (TutorialPanelancle != null)
            TutorialPanelancle.SetActive(false);

        MapGameLoading.Load("map");
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

    public static void ResetTutorialCompletion()
    {
        string playerId = GetPlayerId();

        if (string.IsNullOrEmpty(playerId))
        {
            Debug.LogWarning(
                "[TutorialSession] Cannot reset completion: UserID is missing."
            );
            return;
        }

        PlayerPrefs.DeleteKey(GetTutorialCompletedKey(playerId));
        PlayerPrefs.Save();

        Debug.Log(
            $"[TutorialSession] Tutorial completion reset for player {playerId}."
        );
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

    private static string GetPlayerId()
    {
        string playerId = PlayerPrefs.GetString(PlayerIdKey, "").Trim();

        // Also support projects that store UserID as an integer.
        if (string.IsNullOrEmpty(playerId))
        {
            int numericId = PlayerPrefs.GetInt(PlayerIdKey, 0);

            if (numericId != 0)
                playerId = numericId.ToString();
        }

        return playerId;
    }

    private static string GetTutorialCompletedKey(string playerId)
    {
        return TutorialCompletedKeyPrefix + playerId;
    }
}