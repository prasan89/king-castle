// FirebaseCrashlyticsService.cs — M14 Firebase Crashlytics implementation.
// SECURITY: never pass passwords, tokens, auth data, purchase tokens, or personal info
// to any SetContext / Log / RecordError call.
// Non-fatal recording is reserved for genuine unexpected failures only.

using System;
using UnityEngine;
using KingSmash.Analytics;
using KingSmash.Core;

namespace KingSmash.Services.Firebase
{
    public class FirebaseCrashlyticsService : ICrashReportingService
    {
        private const string Tag             = "Crashlytics";
        private const int    MaxKeyLength    = 64;
        private const int    MaxValueLength  = 64;
        private const int    MaxLogLength    = 1024;

        // -----------------------------------------------------------------------
        // Initialisation
        // -----------------------------------------------------------------------

        /// <summary>
        /// Call once after Firebase.FirebaseApp has been initialised.
        /// </summary>
        public void Initialize()
        {
#if FIREBASE_ENABLED
            Firebase.Crashlytics.Crashlytics.ReportUncaughtExceptionsAsFatal(true);
#endif
            SetContext("app_environment", FirebaseEnvironmentConfig.Environment);
            SetContext("app_version",     Application.version);
            GameLogger.Info(Tag, $"Initialized — env:{FirebaseEnvironmentConfig.Environment} ver:{Application.version}");
        }

        // -----------------------------------------------------------------------
        // ICrashReportingService
        // -----------------------------------------------------------------------

        /// <inheritdoc/>
        public void RecordNonFatalException(Exception ex, string context = "")
        {
            if (ex == null) return;

            SetContext("error_context", context ?? string.Empty);

#if FIREBASE_ENABLED
            Firebase.Crashlytics.Crashlytics.RecordException(ex);
#endif
            GameLogger.Error(Tag, $"NonFatal recorded — context: \"{context}\"", ex);
        }

        /// <inheritdoc/>
        public void RecordError(string message, ErrorCategory category, string context = "")
        {
            if (string.IsNullOrEmpty(message)) return;

            SetContext("error_category", category.ToString());
            SetContext("error_context",  context ?? string.Empty);

            var logLine = $"[{category}] {message} | {context}";

#if FIREBASE_ENABLED
            Firebase.Crashlytics.Crashlytics.Log(Truncate(logLine, MaxLogLength));
#endif
            GameLogger.Warning(Tag, logLine);
        }

        /// <inheritdoc/>
        public void SetContext(string key, string value)
        {
            if (string.IsNullOrEmpty(key)   || string.IsNullOrEmpty(value)) return;
            if (key.Length   > MaxKeyLength)   key   = key.Substring(0, MaxKeyLength);
            if (value.Length > MaxValueLength) value = value.Substring(0, MaxValueLength);

#if FIREBASE_ENABLED
            Firebase.Crashlytics.Crashlytics.SetCustomKey(key, value);
#endif
        }

        /// <inheritdoc/>
        public void SetPlayerContext(int kingLevel, int currentLevel, int currentWorld)
        {
            SetContext("king_level",    kingLevel.ToString());
            SetContext("current_level", currentLevel.ToString());
            SetContext("current_world", currentWorld.ToString());
        }

        /// <inheritdoc/>
        public void SetSceneContext(string sceneName)
        {
            SetContext("current_scene", sceneName ?? string.Empty);
        }

        /// <inheritdoc/>
        public void Log(string message)
        {
            if (string.IsNullOrEmpty(message)) return;
            var safe = Truncate(message, MaxLogLength);

#if FIREBASE_ENABLED
            Firebase.Crashlytics.Crashlytics.Log(safe);
#endif
        }

#if UNITY_EDITOR || KING_SMASH_DEV
        /// <inheritdoc/>
        public void ForceCrash()
        {
            GameLogger.Warning(Tag, "ForceCrash invoked — throwing controlled test exception.");
            throw new Exception("Controlled crash test — M14");
        }
#else
        // Compiled into staging/production as a no-op to satisfy the interface
        // when the build does NOT define UNITY_EDITOR or KING_SMASH_DEV.
        // ICrashReportingService.ForceCrash() is also conditionally compiled out
        // in those configurations, so this branch is only relevant if the
        // interface is ever used without the guard — kept for safety.
#pragma warning disable CS0628 // new protected member in sealed class
        public void ForceCrash()
        {
            GameLogger.Warning(Tag, "ForceCrash called in a non-dev build — no-op.");
        }
#pragma warning restore CS0628
#endif

        // -----------------------------------------------------------------------
        // Helpers
        // -----------------------------------------------------------------------

        private static string Truncate(string value, int maxLength)
            => (value != null && value.Length > maxLength) ? value.Substring(0, maxLength) : value;
    }
}
