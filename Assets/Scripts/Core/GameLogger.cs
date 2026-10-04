// GameLogger.cs — M14 extended with crash-reporting integration.
// All existing behaviour is preserved; new additions are clearly marked.

using System;
using KingSmash.Services;
using UnityEngine;

namespace KingSmash.Core
{
    public enum LogLevel { Debug = 0, Info = 1, Warning = 2, Error = 3, None = 4 }

    public static class GameLogger
    {
        private static LogLevel            _minLevel     = LogLevel.Debug;
        private static ICrashReportingService _crashService; // M14 — set via SetCrashService

        // -----------------------------------------------------------------------
        // Initialisation
        // -----------------------------------------------------------------------

        public static void Initialize(LogLevel minLevel)
        {
            _minLevel = minLevel;
#if !DEVELOPMENT_BUILD && !UNITY_EDITOR
            // In release builds default to Warning unless explicitly overridden
            if (minLevel < LogLevel.Warning)
                _minLevel = LogLevel.Warning;
#endif
        }

        // -----------------------------------------------------------------------
        // M14 — crash-service wiring
        // -----------------------------------------------------------------------

        /// <summary>
        /// Register the active ICrashReportingService so fatal/error logs are
        /// forwarded automatically.  Call after ServiceLocator registration.
        /// </summary>
        public static void SetCrashService(ICrashReportingService service)
        {
            _crashService = service;
        }

        // -----------------------------------------------------------------------
        // Logging — original methods (unchanged signatures)
        // -----------------------------------------------------------------------

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

        /// <summary>
        /// Log an error with an exception.  Also forwards to the crash service
        /// as a non-fatal if one is registered (M14).
        /// </summary>
        public static void Error(string tag, string message, Exception ex)
        {
            if (_minLevel > LogLevel.Error) return;
            UnityEngine.Debug.LogError(Format(LogLevel.Error, tag, $"{message} | {ex}"));

            // M14 — forward to crash service
            _crashService?.RecordNonFatalException(ex, $"[{tag}] {message}");
        }

        // -----------------------------------------------------------------------
        // M14 — new methods
        // -----------------------------------------------------------------------

        /// <summary>
        /// Always emits a LogError regardless of the current LogLevel, and always
        /// forwards to the crash service as a non-fatal.  Use for conditions that
        /// should never happen in any build.
        /// </summary>
        public static void Fatal(string tag, string message, Exception ex)
        {
            UnityEngine.Debug.LogError(Format(LogLevel.Error, tag, $"FATAL: {message} | {ex}"));
            _crashService?.RecordNonFatalException(ex, $"[{tag}] {message}");
        }

        /// <summary>
        /// Forward a Crashlytics custom-key context value if a crash service is
        /// registered.  Safe to call before the service is wired up — becomes a
        /// no-op in that case.
        /// </summary>
        public static void SetContext(string key, string value)
        {
            _crashService?.SetContext(key, value);
        }

        // -----------------------------------------------------------------------
        // Internal
        // -----------------------------------------------------------------------

        private static string Format(LogLevel level, string tag, string message)
            => $"[{level.ToString().ToUpper()}][{tag}] {message}";
    }
}
