// MockCrashReportingService.cs — M14 Editor / unit-test stand-in for ICrashReportingService.
// All methods log through GameLogger.Debug so test output is visible without Firebase.

using System;
using KingSmash.Analytics;
using KingSmash.Core;

namespace KingSmash.Services
{
    public class MockCrashReportingService : ICrashReportingService
    {
        private const string Tag = "CrashMock";

        public void RecordNonFatalException(Exception ex, string context = "")
        {
            GameLogger.Debug(Tag, $"RecordNonFatalException — context: \"{context}\" | exception: {ex?.GetType().Name}: {ex?.Message}");
        }

        public void RecordError(string message, ErrorCategory category, string context = "")
        {
            GameLogger.Debug(Tag, $"RecordError — [{category}] message: \"{message}\" | context: \"{context}\"");
        }

        public void SetContext(string key, string value)
        {
            GameLogger.Debug(Tag, $"SetContext — key: \"{key}\" value: \"{value}\"");
        }

        public void SetPlayerContext(int kingLevel, int currentLevel, int currentWorld)
        {
            GameLogger.Debug(Tag, $"SetPlayerContext — kingLevel:{kingLevel} currentLevel:{currentLevel} currentWorld:{currentWorld}");
        }

        public void SetSceneContext(string sceneName)
        {
            GameLogger.Debug(Tag, $"SetSceneContext — scene: \"{sceneName}\"");
        }

        public void Log(string message)
        {
            GameLogger.Debug(Tag, $"Log — \"{message}\"");
        }

#if UNITY_EDITOR || KING_SMASH_DEV
        public void ForceCrash()
        {
            GameLogger.Debug(Tag, "ForceCrash — throwing controlled crash test exception.");
            throw new Exception("Controlled crash test");
        }
#endif
    }
}
