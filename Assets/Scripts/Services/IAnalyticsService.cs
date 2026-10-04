namespace KingSmash.Services
{
    /// <summary>
    /// Analytics service interface.
    /// Implement typed Track methods for structured, parameter-safe event logging.
    /// The raw LogEvent fallback remains available for one-off or custom events.
    /// </summary>
    public interface IAnalyticsService
    {
        // ── Low-level ─────────────────────────────────────────────────────────
        void LogEvent(string eventName, params (string key, object value)[] parameters);
        void SetUserProperty(string key, string value);
        void SetUserId(string userId);

        // ── Level ─────────────────────────────────────────────────────────────
        void TrackLevelStarted(int levelId, int worldId, int attemptNumber);
        void TrackLevelCompleted(int levelId, int worldId, int stars, int attemptNumber,
                                 float completionTime, int remainingKings);
        void TrackLevelFailed(int levelId, int worldId, int attemptNumber, string reason);
        void TrackLevelAbandoned(int levelId, int worldId, int attemptNumber);
        void TrackLevelRetried(int levelId, int worldId, int attemptNumber);

        // ── Power-Ups ─────────────────────────────────────────────────────────
        void TrackPowerUpActivated(string powerupId, int levelId, int worldId, int quantityBefore);

        // ── Progression ───────────────────────────────────────────────────────
        void TrackKingUpgrade(string statName, float oldValue, float newValue,
                              long cost, int playerLevel);

        // ── Economy ───────────────────────────────────────────────────────────
        void TrackCurrencyEvent(string eventName, string currency, long amount, string source);
        void TrackRewardClaimed(string rewardType, long amount, string source);

        // ── Ads ───────────────────────────────────────────────────────────────
        void TrackAdEvent(string eventName, string adType, string placement);
        void TrackAdRewardGranted(string placement, string rewardType);

        // ── IAP ───────────────────────────────────────────────────────────────
        void TrackPurchaseEvent(string eventName, string productId, string productType);

        // ── Network / Cloud ───────────────────────────────────────────────────
        void TrackCloudEvent(string eventName, string operation, bool isOnline,
                             string errorCategory = "");

        // ── Retention ─────────────────────────────────────────────────────────
        void TrackRetentionEvent(string eventName);

        // ── Config ────────────────────────────────────────────────────────────
        void SetEnvironment(string environment);
        void SetConfigVersion(string version);
    }
}
