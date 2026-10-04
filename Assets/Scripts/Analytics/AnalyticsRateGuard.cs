using KingSmash.Core;
using UnityEngine;

namespace KingSmash.Analytics
{
    /// <summary>
    /// Lightweight static rate guard for analytics events.
    /// Allows up to 200 distinct LogEvent calls per 60-second rolling window.
    /// Call Allow() before dispatching each event; it returns false and logs a
    /// warning when the budget is exhausted, preventing accidental spam.
    /// </summary>
    public static class AnalyticsRateGuard
    {
        private const int   MaxEventsPerWindow = 200;
        private const float WindowSeconds      = 60f;
        private const string Tag               = "AnalyticsRateGuard";

        private static int   _count;
        private static float _windowStart;

        static AnalyticsRateGuard()
        {
            _windowStart = Time.realtimeSinceStartup;
            _count       = 0;
        }

        /// <summary>
        /// Returns true if the event is allowed within the current rate window.
        /// Returns false (and logs a warning) when the per-minute budget is exceeded.
        /// </summary>
        public static bool Allow(string eventName)
        {
            float now = Time.realtimeSinceStartup;

            // Roll the window forward when more than 60 s have elapsed.
            if (now - _windowStart > WindowSeconds)
            {
                _windowStart = now;
                _count       = 0;
            }

            if (_count >= MaxEventsPerWindow)
            {
                GameLogger.Warning(Tag,
                    $"Rate limit ({MaxEventsPerWindow}/min) exceeded — dropping '{eventName}'");
                return false;
            }

            _count++;
            return true;
        }

        /// <summary>
        /// Manually resets the rate window.
        /// Useful in tests or after a confirmed session boundary.
        /// </summary>
        public static void Reset()
        {
            _windowStart = Time.realtimeSinceStartup;
            _count       = 0;
        }
    }
}
