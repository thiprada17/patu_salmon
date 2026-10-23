using UnityEngine;

public static class TutorialSession
{
    public static bool IsInitialized { get; private set; }
    public static bool IsFirstTimePlayer { get; private set; }
    public static bool IsTutorialActive { get; private set; }

    public static void Initialize(int workDays)
    {
        IsInitialized = true;
        IsFirstTimePlayer = workDays == 0;
        IsTutorialActive = false;

        Debug.Log(
            $"[TutorialSession] work_days={workDays}, " +
            $"FirstTimePlayer={IsFirstTimePlayer}"
        );
    }

    public static void StartTutorial()
    {
        if (!IsInitialized || !IsFirstTimePlayer)
            return;

        IsTutorialActive = true;
        Debug.Log("[TutorialSession] Tutorial started.");
    }

    public static void CompleteTutorial()
    {
        IsTutorialActive = false;
        IsFirstTimePlayer = false;

        Debug.Log("[TutorialSession] Tutorial completed.");
    }
}