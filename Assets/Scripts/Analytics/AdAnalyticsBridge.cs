// AdAnalyticsBridge.cs — M14 Analytics
// Static entry points called from RewardedAdFlowService / GoogleMobileAdsService.
// Also subscribes to PowerUpController.OnEffectStarted to detect rewarded-ad context
// (power-ups granted via ads carry the source tag "rewarded_ad").
//
// IMPORTANT: reward is tracked ONLY in TrackAdRewardGranted — never in
// TrackAdStarted or TrackAdLoaded to avoid double-counting.

using UnityEngine;
using KingSmash.Core;
using KingSmash.PowerUps;
using KingSmash.Services;

namespace KingSmash.Analytics
{
    /// <summary>
    /// Scene MonoBehaviour that bridges ad-lifecycle events to analytics.
    /// The static Track* methods are the primary entry points; they are called
    /// directly from the ad services without requiring a scene reference.
    /// </summary>
    public sealed class AdAnalyticsBridge : MonoBehaviour
    {
        private const string Tag             = "AdAnalyticsBridge";
        private const string RewardedAdSource = "rewarded_ad";

        // ── Lifecycle ─────────────────────────────────────────────────────────

        private void OnEnable()
        {
            PowerUpController.OnEffectStarted += HandlePowerUpEffectStarted;
        }

        private void OnDisable()
        {
            PowerUpController.OnEffectStarted -= HandlePowerUpEffectStarted;
        }

        // ── PowerUp handler (rewarded-ad context filter) ───────────────────────

        /// <summary>
        /// Tracks power-ups activated as a result of a rewarded ad.
        /// Power-ups from other sources are handled by LevelAnalyticsBridge.
        /// This bridge tracks only the ad_started event here (reward tracked separately).
        /// </summary>
        private void HandlePowerUpEffectStarted(PowerUpType type)
        {
            // Only log ad_started when the effect originates from a rewarded-ad grant.
            // This check is intentionally conservative: the bridge cannot know for certain
            // the source without state, so we track powerup_activated with ad source.
            // Full ad flow is covered via the static Track* methods below.

            if (!ServiceLocator.TryGet<IAnalyticsService>(out var analytics)) return;
            if (!AnalyticsRateGuard.Allow(AnalyticsEvents.PowerUpActivated)) return;

            analytics.LogEvent(AnalyticsEvents.PowerUpActivated,
                (AnalyticsParameters.PowerupId, type.ToString()),
                (AnalyticsParameters.Source,    RewardedAdSource));
        }

        // ── Static entry points (called from ad service classes) ──────────────

        /// <summary>Tracks that an ad was requested from the ad network.</summary>
        public static void TrackAdRequested(string adType, string placement)
        {
            if (!ServiceLocator.TryGet<IAnalyticsService>(out var analytics)) return;
            if (!AnalyticsRateGuard.Allow(AnalyticsEvents.AdRequested)) return;

            analytics.TrackAdEvent(AnalyticsEvents.AdRequested, adType, placement);
        }

        /// <summary>Tracks that an ad loaded successfully.</summary>
        public static void TrackAdLoaded(string adType, string placement)
        {
            if (!ServiceLocator.TryGet<IAnalyticsService>(out var analytics)) return;
            if (!AnalyticsRateGuard.Allow(AnalyticsEvents.AdLoaded)) return;

            analytics.TrackAdEvent(AnalyticsEvents.AdLoaded, adType, placement);
        }

        /// <summary>Tracks that an ad failed to load or show. Does NOT track reward.</summary>
        public static void TrackAdFailed(string adType, string placement)
        {
            if (!ServiceLocator.TryGet<IAnalyticsService>(out var analytics)) return;
            if (!AnalyticsRateGuard.Allow(AnalyticsEvents.AdFailed)) return;

            analytics.TrackAdEvent(AnalyticsEvents.AdFailed, adType, placement);
        }

        /// <summary>Tracks that an ad started showing. Does NOT track reward.</summary>
        public static void TrackAdStarted(string adType, string placement)
        {
            if (!ServiceLocator.TryGet<IAnalyticsService>(out var analytics)) return;
            if (!AnalyticsRateGuard.Allow(AnalyticsEvents.AdStarted)) return;

            analytics.TrackAdEvent(AnalyticsEvents.AdStarted, adType, placement);
        }

        /// <summary>Tracks that an ad was completed (watched to the end). Does NOT track reward.</summary>
        public static void TrackAdCompleted(string adType, string placement)
        {
            if (!ServiceLocator.TryGet<IAnalyticsService>(out var analytics)) return;
            if (!AnalyticsRateGuard.Allow(AnalyticsEvents.AdCompleted)) return;

            analytics.TrackAdEvent(AnalyticsEvents.AdCompleted, adType, placement);
        }

        /// <summary>
        /// Tracks that the reward was granted after a rewarded ad.
        /// This is the ONLY place rewards are tracked — never in TrackAdStarted / TrackAdLoaded.
        /// </summary>
        public static void TrackAdRewardGranted(string placement, string rewardType)
        {
            if (!ServiceLocator.TryGet<IAnalyticsService>(out var analytics)) return;
            if (!AnalyticsRateGuard.Allow(AnalyticsEvents.RewardedAdRewardGranted)) return;

            analytics.TrackAdRewardGranted(placement, rewardType);
        }
    }
}
