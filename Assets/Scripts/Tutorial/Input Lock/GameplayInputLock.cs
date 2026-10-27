
using UnityEngine;

public static class GameplayInputLock
{
    private static bool isDialogueLocked;
    private static bool isPauseLocked;

    public static bool IsLocked => isDialogueLocked || isPauseLocked;

    public static void Lock()
    {
        isDialogueLocked = true;
        Debug.Log("Gameplay LOCKED");
    }

    public static void Unlock()
    {
        isDialogueLocked = false;
        Debug.Log("Gameplay UNLOCKED");
    }

    public static void SetPauseLocked(bool locked)
    {
        isPauseLocked = locked;
    }
}
