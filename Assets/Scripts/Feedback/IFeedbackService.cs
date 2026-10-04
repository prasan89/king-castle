// IFeedbackService.cs — M16 soft-launch: in-game player feedback collection interface.
// Privacy note: no PII is to be collected. Free-text messages must be screened
// before transmission; see FeedbackServiceMock for the caller-side check pattern.

using System;
using UnityEngine;

namespace KingSmash.Services
{
    // ── Category ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Top-level category the player selects before submitting feedback.
    /// Keep values stable — they are stored as strings in analytics events.
    /// </summary>
    public enum FeedbackCategory
    {
        Gameplay,
        Difficulty,
        UI,
        Performance,
        Ads,
        Purchases,
        Technical,
        Other
    }

    // ── Context ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Snapshot of player/session state at the moment feedback is submitted.
    /// Serializable so it can be written to disk or sent over the wire.
    /// No PII fields — do not add user name, email, device ID, or IP address.
    /// </summary>
    [Serializable]
    public struct FeedbackContext
    {
        /// <summary>Zero-based level index active when feedback was submitted.</summary>
        public int currentLevel;

        /// <summary>Zero-based world index active when feedback was submitted.</summary>
        public int currentWorld;

        /// <summary>Player's current king level (XP tier, not level index).</summary>
        public int playerLevel;

        /// <summary>Application.version string at build time.</summary>
        public string appVersion;

        /// <summary>Firebase environment tag: "development", "staging", or "production".</summary>
        public string environment;
    }

    // ── Interface ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Service contract for in-game player feedback collection.
    ///
    /// Implementations must:
    ///  - Never transmit PII.
    ///  - Be safe to call from the main thread only.
    ///  - Tolerate null or empty <paramref name="message"/> gracefully (no throw).
    ///
    /// Register via <c>ServiceLocator.Register&lt;IFeedbackService&gt;(impl)</c> during
    /// app startup. The screen layer calls <c>ServiceLocator.TryGet</c> so the
    /// service is optional; absence is handled silently.
    /// </summary>
    public interface IFeedbackService
    {
        /// <summary>
        /// Submit a feedback entry.
        /// Caller is responsible for basic validation (length, PII screen) before
        /// invoking — implementations may impose their own secondary validation.
        /// </summary>
        /// <param name="category">Player-selected category tag.</param>
        /// <param name="message">Free-text message, already trimmed and length-capped.</param>
        /// <param name="context">Snapshot of player/session state at submission time.</param>
        void SubmitFeedback(FeedbackCategory category, string message, FeedbackContext context);

        /// <summary>
        /// Returns true when the service is ready to accept submissions.
        /// FeedbackScreen should disable its submit button when false.
        /// </summary>
        bool IsAvailable { get; }
    }
}
