using UnityEngine;

public static class GameLogger
{
    public enum LogLevel
    {
        Error = 0,
        Warn = 1,
        Info = 2,
        Debug = 3
    }

    public static LogLevel CurrentLevel = LogLevel.Info;

    public static void Error(string category, string message)
    {
        if (CurrentLevel >= LogLevel.Error)
            Debug.LogError(Format("ERROR", category, message, "#FF5252"));
    }

    public static void Warn(string category, string message)
    {
        if (CurrentLevel >= LogLevel.Warn)
            Debug.LogWarning(Format("WARN", category, message, "#FF9800"));
    }

    public static void Info(string category, string message)
    {
        if (CurrentLevel >= LogLevel.Info)
            Debug.Log(Format("INFO", category, message, "#40C4FF"));
    }

    public static void DebugLog(string category, string message)
    {
        if (CurrentLevel >= LogLevel.Debug)
            Debug.Log(Format("DEBUG", category, message, "#B0BEC5"));
    }

    private static string Format(string level, string category, string message, string color)
    {
        return
            $"<color={color}>[{level}]</color> " +
            $"<b>[{category}]</b> {message}";
    }
}
