using System;
using UnityEngine;

namespace KingSmash.Core
{
    public enum LogLevel { Debug = 0, Info = 1, Warning = 2, Error = 3, None = 4 }

    public static class GameLogger
    {
        private static LogLevel _minLevel = LogLevel.Debug;

        public static void Initialize(LogLevel minLevel)
        {
            _minLevel = minLevel;
#if !DEVELOPMENT_BUILD && !UNITY_EDITOR
            // In release builds default to Warning unless explicitly overridden
            if (minLevel < LogLevel.Warning)
                _minLevel = LogLevel.Warning;
#endif
        }

        public static void Debug(string tag, string message)
        {
            if (_minLevel > LogLevel.Debug) return;
            UnityEngine.Debug.Log(Format(LogLevel.Debug, tag, message));
        }

        public static void Info(string tag, string message)
        {
            if (_minLevel > LogLevel.Info) return;
            UnityEngine.Debug.Log(Format(LogLevel.Info, tag, message));
        }

        public static void Warning(string tag, string message)
        {
            if (_minLevel > LogLevel.Warning) return;
            UnityEngine.Debug.LogWarning(Format(LogLevel.Warning, tag, message));
        }

        public static void Error(string tag, string message)
        {
            if (_minLevel > LogLevel.Error) return;
            UnityEngine.Debug.LogError(Format(LogLevel.Error, tag, message));
        }

        public static void Error(string tag, string message, Exception ex)
        {
            if (_minLevel > LogLevel.Error) return;
            UnityEngine.Debug.LogError(Format(LogLevel.Error, tag, $"{message} | {ex}"));
        }

        private static string Format(LogLevel level, string tag, string message)
            => $"[{level.ToString().ToUpper()}][{tag}] {message}";
    }
}
