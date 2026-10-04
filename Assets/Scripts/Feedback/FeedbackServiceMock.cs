// FeedbackServiceMock.cs — M16 soft-launch: development/staging mock of IFeedbackService.
// This implementation logs locally and fires analytics; it does NOT send data to
// any external endpoint. Replace with a real backend implementation when a server-side
// endpoint is available.

using KingSmash.Core;

namespace KingSmash.Services
{
    /// <summary>
    /// Mock implementation of <see cref="IFeedbackService"/> for development and
    /// closed-alpha builds. Logs via <see cref="GameLogger"/> and fires a static
    /// event so other systems (tests, debug overlay) can observe submissions.
    ///
    /// Analytics: forwards a <c>player_feedback</c> event via
    /// <see cref="IAnalyticsService"/> when one is registered.
    ///
    /// PII guard: caller (<see cref="KingSmash.UI.FeedbackScreen"/>) is responsible
    /// for the primary PII screen; this class adds a secondary log warning if a
    /// suspicious pattern slips through.
    /// </summary>
    public class FeedbackServiceMock : IFeedbackService
    {
        private const string Tag = "FeedbackMock";

        // Simple regex-free heuristics for common PII patterns.
        // These are a secondary safety net — not a substitute for server-side review.
        private static readonly string[] PiiHints = { "@", "phone:", "tel:", "+1", "ssn" };

        // ── Static event ─────────────────────────────────────────────────────────

        /// <summary>
        /// Fired on the main thread immediately after a feedback submission is
        /// logged. Useful for automated tests and in-editor debug panels.
        /// </summary>
        public static event System.Action<FeedbackCategory, string> OnFeedbackSubmitted;

        // ── IFeedbackService ─────────────────────────────────────────────────────

        /// <inheritdoc/>
        public bool IsAvailable => true;

        /// <inheritdoc/>
        public void SubmitFeedback(FeedbackCategory category, string message, FeedbackContext context)
        {
            // Secondary PII screen — log a warning but do not block submission.
            if (!string.IsNullOrEmpty(message))
            {
                string lower = message.ToLowerInvariant();
                foreach (string hint in PiiHints)
                {
                    if (lower.Contains(hint))
                    {
                        GameLogger.Warning(Tag,
                            $"Feedback message may contain PII (matched hint '{hint}'). " +
                            "Review submission pipeline. Message suppressed from this log entry.");
                        break;
                    }
                }
            }

            // Primary local log (message only appears in development/editor builds).
            GameLogger.Info(Tag,
                $"Feedback [{category}]: {message} | " +
                $"Level={context.currentLevel} World={context.currentWorld} " +
                $"PlayerLevel={context.playerLevel} Version={context.appVersion} " +
                $"Env={context.environment}");

            // Analytics event — optional dependency.
            if (ServiceLocator.TryGet<IAnalyticsService>(out var analytics))
            {
                analytics.LogEvent(
                    "player_feedback",
                    ("category",  category.ToString()),
                    ("level_id",  context.currentLevel),
                    ("world_id",  context.currentWorld),
                    ("app_version", context.appVersion ?? string.Empty),
                    ("environment", context.environment ?? string.Empty));
            }

            // Notify observers (tests, debug panels).
            OnFeedbackSubmitted?.Invoke(category, message);
        }
    }
}
