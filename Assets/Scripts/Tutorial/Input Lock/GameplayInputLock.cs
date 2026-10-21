
using UnityEngine;

public static class GameplayInputLock
{
    public static bool IsLocked { get; private set; }

    public static void Lock()
    {
        IsLocked = true;
        Debug.Log("Gameplay LOCKED");
    }

    public static void Unlock()
    {
        IsLocked = false;
        Debug.Log("Gameplay UNLOCKED");
    }
}
