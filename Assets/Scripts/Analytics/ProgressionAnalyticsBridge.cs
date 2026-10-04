// ProgressionAnalyticsBridge.cs — M14 Analytics
// Bridges king level-up events and world progression events to IAnalyticsService.
// Also exposes a public TrackStatUpgraded method for UpgradeScreen / KingUpgradeService.

using UnityEngine;
using KingSmash.Core;
using KingSmash.Levels;
using KingSmash.Progression;
using KingSmash.Save;
using KingSmash.Services;

namespace KingSmash.Analytics
{
    /// <summary>
    /// Scene MonoBehaviour that bridges progression static events to analytics.
    /// Subscribe in the scene. TrackStatUpgraded is called directly from the
    /// upgrade UI or from a KingUpgradeService bridge.
    /// </summary>
    public sealed class ProgressionAnalyticsBridge : MonoBehaviour
    {
        private const string Tag = "ProgressionAnalyticsBridge";

        // ── Lifecycle ─────────────────────────────────────────────────────────

        private void OnEnable()
        {
            KingProgressionService.OnKingLevelUp  += HandleKingLevelUp;
            KingProgressionService.OnXPChanged    += HandleXPChanged;
            WorldProgressionService.OnWorldUnlocked  += HandleWorldUnlocked;
            WorldProgressionService.OnWorldCompleted += HandleWorldCompleted;
        }

        private void OnDisable()
        {
            KingProgressionService.OnKingLevelUp  -= HandleKingLevelUp;
            KingProgressionService.OnXPChanged    -= HandleXPChanged;
            WorldProgressionService.OnWorldUnlocked  -= HandleWorldUnlocked;
            WorldProgressionService.OnWorldCompleted -= HandleWorldCompleted;
        }

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// Call from UpgradeScreen or KingUpgradeService when a stat upgrade completes.
        /// </summary>
        public void TrackStatUpgraded(string statName, float oldValue, float newValue,
                                      long cost, int playerLevel)
        {
            if (!ServiceLocator.TryGet<IAnalyticsService>(out var analytics)) return;
            if (!AnalyticsRateGuard.Allow(AnalyticsEvents.StatUpgraded)) return;

            analytics.TrackKingUpgrade(statName, oldValue, newValue, cost, playerLevel);
        }

        // ── Handlers ─────────────────────────────────────────────────────────

        private void HandleKingLevelUp(int fromLevel, int toLevel, KingStats newStats)
        {
            if (!ServiceLocator.TryGet<IAnalyticsService>(out var analytics)) return;
            if (!AnalyticsRateGuard.Allow(AnalyticsEvents.KingLevelUp)) return;

            // Map to TrackKingUpgrade: stat="king_level", old=fromLevel, new=toLevel,
            // cost=0 (level-up is not purchased), playerLevel=toLevel.
            analytics.TrackKingUpgrade(
                statName:    "king_level",
                oldValue:    fromLevel,
                newValue:    toLevel,
                cost:        0L,
                playerLevel: toLevel);
        }

        private void HandleXPChanged(long currentXp, long requiredXp)
        {
            // XP changes are very frequent; log only if rate guard allows.
            // Use LogEvent (not a typed method) as IAnalyticsService has no TrackXpChanged.
            if (!ServiceLocator.TryGet<IAnalyticsService>(out var analytics)) return;
            if (!AnalyticsRateGuard.Allow(AnalyticsEvents.XpGained)) return;

            analytics.LogEvent(AnalyticsEvents.XpGained,
                (AnalyticsParameters.Amount,      currentXp),
                (AnalyticsParameters.NewValue,    requiredXp));
        }

        private void HandleWorldUnlocked(int worldIndex)
        {
            if (!ServiceLocator.TryGet<IAnalyticsService>(out var analytics)) return;
            if (!AnalyticsRateGuard.Allow(AnalyticsEvents.WorldUnlocked)) return;

            analytics.LogEvent(AnalyticsEvents.WorldUnlocked,
                (AnalyticsParameters.WorldId, worldIndex));
        }

        private void HandleWorldCompleted(int worldIndex)
        {
            if (!ServiceLocator.TryGet<IAnalyticsService>(out var analytics)) return;
            if (!AnalyticsRateGuard.Allow(AnalyticsEvents.WorldCompleted)) return;

            analytics.LogEvent(AnalyticsEvents.WorldCompleted,
                (AnalyticsParameters.WorldId, worldIndex));
        }
    }
}
