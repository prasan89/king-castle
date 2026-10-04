// ICrashReportingService.cs — M14 Crash Reporting Interface
// SECURITY: never pass passwords, tokens, auth data, purchase tokens, or personal info.

using System;

namespace KingSmash.Services
{
    public interface ICrashReportingService
    {
        /// <summary>
        /// Record a genuine unexpected non-fatal exception. Do NOT call for routine
        /// failures such as "no internet" — reserve for truly unexpected states.
        /// </summary>
        void RecordNonFatalException(Exception ex, string context = "");

        /// <summary>
        /// Record a structured error with category metadata.
        /// </summary>
        void RecordError(string message, KingSmash.Analytics.ErrorCategory category, string context = "");

        /// <summary>
        /// Set a Crashlytics custom key. Never pass secrets or personal data.
        /// </summary>
        void SetContext(string key, string value);

        /// <summary>
        /// Snapshot the player's progression state into Crashlytics custom keys.
        /// </summary>
        void SetPlayerContext(int kingLevel, int currentLevel, int currentWorld);

        /// <summary>
        /// Record the active Unity scene name.
        /// </summary>
        void SetSceneContext(string sceneName);

        /// <summary>
        /// Write a plain-text breadcrumb message to Crashlytics.
        /// </summary>
        void Log(string message);

#if UNITY_EDITOR || KING_SMASH_DEV
        /// <summary>
        /// DEV / EDITOR ONLY — force a controlled crash for QA validation.
        /// This method is compiled out of staging and production builds.
        /// </summary>
        void ForceCrash();
#endif
    }
}
