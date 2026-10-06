// LevelAnalyticsBridge.cs — M14 Analytics
// Decoupled bridge: listens to static events from LevelController, QueenController,
// PowerUpController and routes them to IAnalyticsService / ICrashReportingService.
// Lives in the scene — does NOT embed analytics logic in gameplay classes.

using UnityEngine;
using KingSmash.Characters;
using KingSmash.Core;
using KingSmash.Levels;
using KingSmash.PowerUps;
using KingSmash.Services;

namespace KingSmash.Analytics
{
    /// <summary>
    /// Scene MonoBehaviour that bridges level-lifecycle static events to analytics.
    /// Subscribe this GameObject to the scene; call SetCurrentLevel from
    /// LevelController or LevelSceneInitializer before the level starts.
    /// </summary>
    public sealed class LevelAnalyticsBridge : MonoBehaviour
    {
        private const string Tag = "LevelAnalyticsBridge";

        // ── State set externally ──────────────────────────────────────────────
        private int   _levelId;
        private int   _worldId;
        private int   _attemptNumber;

        // ── Timing ───────────────────────────────────────────────────────────
        private float _levelStartTime = -1f;

        // ── Lifecycle ────────────────────────────────────────────────────────

        private void OnEnable()
        {
            LevelController.OnLevelStateChanged += HandleLevelStateChanged;
            LevelController.OnLevelEnded         += HandleLevelEnded;
            QueenController.OnQueenRescued       += HandleQueenRescued;
            PowerUpController.OnEffectStarted    += HandlePowerUpEffectStarted;
        }

        private void OnDisable()
        {
            LevelController.OnLevelStateChanged -= HandleLevelStateChanged;
            LevelController.OnLevelEnded         -= HandleLevelEnded;
            QueenController.OnQueenRescued       -= HandleQueenRescued;
            PowerUpController.OnEffectStarted    -= HandlePowerUpEffectStarted;
        }

        // ── Public API ───────────────────────────────────────────────────────

        /// <summary>
        /// Called by LevelController or LevelSceneInitializer before the level starts.
        /// </summary>
        public void SetCurrentLevel(int levelId, int worldId, int attemptNumber)
        {
            _levelId       = levelId;
            _worldId       = worldId;
            _attemptNumber = attemptNumber;
        }

        // ── Handlers ─────────────────────────────────────────────────────────

        private void HandleLevelStateChanged(LevelState previous, LevelState next)
        {
            if (next != LevelState.Playing) return;

            _levelStartTime = Time.realtimeSinceStartup;

            // Update crash-reporting player context on level start
            if (ServiceLocator.TryGet<ICrashReportingService>(out var crash))
                crash.SetPlayerContext(0, _levelId, _worldId);

            if (!ServiceLocator.TryGet<IAnalyticsService>(out var analytics)) return;
            if (!AnalyticsRateGuard.Allow(AnalyticsEvents.LevelStart)) return;

            analytics.TrackLevelStarted(_levelId, _worldId, _attemptNumber);
        }

        private void HandleLevelEnded(int score, int stars, float destructionRatio)
        {
            float completionTime = _levelStartTime >= 0f
                ? Time.realtimeSinceStartup - _levelStartTime
                : 0f;

            bool isVictory = stars > 0;

            if (!ServiceLocator.TryGet<IAnalyticsService>(out var analytics)) return;

            if (isVictory)
            {
                if (!AnalyticsRateGuard.Allow(AnalyticsEvents.LevelComplete)) return;

                // remainingKings — score is tracked by LaunchController analytics; pass 0
                // when not available from this bridge.
                analytics.TrackLevelCompleted(
                    levelId:        _levelId,
                    worldId:        _worldId,
                    stars:          stars,
                    attemptNumber:  _attemptNumber,
                    completionTime: completionTime,
                    remainingKings: 0);
            }
            else
            {
                if (!AnalyticsRateGuard.Allow(AnalyticsEvents.LevelFailed)) return;

                analytics.TrackLevelFailed(
                    levelId:       _levelId,
                    worldId:       _worldId,
                    attemptNumber: _attemptNumber,
                    reason:        "out_of_attempts");
            }
        }

        private void HandleQueenRescued(QueenController queen)
        {
            if (!ServiceLocator.TryGet<IAnalyticsService>(out var analytics)) return;
            if (!AnalyticsRateGuard.Allow(AnalyticsEvents.QueenRescued)) return;

            analytics.LogEvent(AnalyticsEvents.QueenRescued,
                (AnalyticsParameters.LevelId, _levelId),
                (AnalyticsParameters.WorldId,  _worldId));
        }

        private void HandlePowerUpEffectStarted(PowerUpType type)
        {
            if (!ServiceLocator.TryGet<IAnalyticsService>(out var analytics)) return;
            if (!AnalyticsRateGuard.Allow(AnalyticsEvents.PowerUpActivated)) return;

            analytics.TrackPowerUpActivated(
                powerupId:      type.ToString(),
                levelId:        _levelId,
                worldId:        _worldId,
                quantityBefore: 0);   // quantity before not available from this event source
        }
    }
}
