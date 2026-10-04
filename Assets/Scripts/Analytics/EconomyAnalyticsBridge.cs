// EconomyAnalyticsBridge.cs — M14 Analytics
// Bridges DailyRewardService and MissionService claimed events to IAnalyticsService.
// Exposes static helpers for currency and retention events so CurrencyService and
// app-open code can call them without a direct reference to the MonoBehaviour.

using UnityEngine;
using KingSmash.Core;
using KingSmash.Retention;
using KingSmash.Services;

namespace KingSmash.Analytics
{
    /// <summary>
    /// Scene MonoBehaviour that bridges economy static events to analytics.
    /// Static helpers (TrackCurrencyEarned, TrackCurrencySpent, TrackRetentionOpen)
    /// can be called directly from CurrencyService or app-open code.
    /// </summary>
    public sealed class EconomyAnalyticsBridge : MonoBehaviour
    {
        private const string Tag = "EconomyAnalyticsBridge";

        // ── Lifecycle ─────────────────────────────────────────────────────────

        private void OnEnable()
        {
            DailyRewardService.OnDailyClaimed  += HandleDailyClaimed;
            MissionService.OnMissionClaimed    += HandleMissionClaimed;
        }

        private void OnDisable()
        {
            DailyRewardService.OnDailyClaimed  -= HandleDailyClaimed;
            MissionService.OnMissionClaimed    -= HandleMissionClaimed;
        }

        // ── Handlers ─────────────────────────────────────────────────────────

        private void HandleDailyClaimed(DailyRewardResult result)
        {
            if (!result.Success) return;
            if (!ServiceLocator.TryGet<IAnalyticsService>(out var analytics)) return;
            if (!AnalyticsRateGuard.Allow(AnalyticsEvents.DailyRewardClaimed)) return;

            long amount = result.CoinsGranted > 0 ? result.CoinsGranted : result.GemsGranted;
            analytics.TrackRewardClaimed("daily_reward", amount, "daily");
        }

        private void HandleMissionClaimed(string missionId, MissionClaimResult result)
        {
            if (!result.Success) return;
            if (!ServiceLocator.TryGet<IAnalyticsService>(out var analytics)) return;
            if (!AnalyticsRateGuard.Allow(AnalyticsEvents.MissionClaimed)) return;

            long amount = result.Reward != null
                ? (result.Reward.coins > 0 ? result.Reward.coins : result.Reward.gems)
                : 0L;
            analytics.TrackRewardClaimed("mission", amount, "mission");
        }

        // ── Static helpers — call from CurrencyService / app code ─────────────

        /// <summary>
        /// Tracks a currency earn event (e.g. level reward, daily bonus).
        /// Safe to call from any context; never throws if service is missing.
        /// </summary>
        public static void TrackCurrencyEarned(string currency, long amount, string source)
        {
            if (!ServiceLocator.TryGet<IAnalyticsService>(out var analytics)) return;
            if (!AnalyticsRateGuard.Allow(AnalyticsEvents.CurrencyEarned)) return;

            analytics.TrackCurrencyEvent(AnalyticsEvents.CurrencyEarned, currency, amount, source);
        }

        /// <summary>
        /// Tracks a currency spend event (e.g. powerup purchase, upgrade).
        /// Safe to call from any context; never throws if service is missing.
        /// </summary>
        public static void TrackCurrencySpent(string currency, long amount, string source)
        {
            if (!ServiceLocator.TryGet<IAnalyticsService>(out var analytics)) return;
            if (!AnalyticsRateGuard.Allow(AnalyticsEvents.CurrencySpent)) return;

            analytics.TrackCurrencyEvent(AnalyticsEvents.CurrencySpent, currency, amount, source);
        }

        /// <summary>
        /// Tracks a retention day open event. Call from app-open flow.
        /// daysSinceInstall is bucketed (1, 2, 3, 7, 14, 30, 60, 90+).
        /// </summary>
        public static void TrackRetentionOpen(int daysSinceInstall)
        {
            if (!ServiceLocator.TryGet<IAnalyticsService>(out var analytics)) return;
            if (!AnalyticsRateGuard.Allow(AnalyticsEvents.RetentionDayOpen)) return;

            int bucket = BucketDay(daysSinceInstall);
            analytics.LogEvent(AnalyticsEvents.RetentionDayOpen,
                (AnalyticsParameters.Amount, bucket));
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static int BucketDay(int d)
        {
            if (d <= 1)  return 1;
            if (d <= 2)  return 2;
            if (d <= 3)  return 3;
            if (d <= 7)  return 7;
            if (d <= 14) return 14;
            if (d <= 30) return 30;
            if (d <= 60) return 60;
            return 90;
        }
    }
}
