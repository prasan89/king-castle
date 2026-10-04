// GameplayAnalyticsBridge.cs — M14 Analytics
// Bridges gameplay-moment static events (launches, collisions, destructions, enemy deaths)
// to IAnalyticsService. Heavy throttling is applied: collision events are skipped below a
// speed threshold, and all events pass through AnalyticsRateGuard.

using UnityEngine;
using KingSmash.Characters;
using KingSmash.Core;
using KingSmash.Gameplay;
using KingSmash.Physics;
using KingSmash.Services;

namespace KingSmash.Analytics
{
    /// <summary>
    /// Scene MonoBehaviour that bridges gameplay static events to analytics.
    /// Does not track per-frame or per-physics-tick events.
    /// The current level context must be provided by LevelAnalyticsBridge or
    /// set via SetCurrentLevel.
    /// </summary>
    public sealed class GameplayAnalyticsBridge : MonoBehaviour
    {
        private const string Tag             = "GameplayAnalyticsBridge";
        private const float  MinImpactSpeed  = 3f;   // m/s threshold for king_impact events

        // ── Level context ─────────────────────────────────────────────────────
        private int _levelId;
        private int _worldId;

        // ── Lifecycle ─────────────────────────────────────────────────────────

        private void OnEnable()
        {
            LaunchController.OnKingLaunched        += HandleKingLaunched;
            KingProjectile.OnKingCollision         += HandleKingCollision;
            DestructibleObject.OnDestroyed         += HandleDestructibleObjectDestroyed;
            EnemyController.OnEnemyDied            += HandleEnemyDied;
        }

        private void OnDisable()
        {
            LaunchController.OnKingLaunched        -= HandleKingLaunched;
            KingProjectile.OnKingCollision         -= HandleKingCollision;
            DestructibleObject.OnDestroyed         -= HandleDestructibleObjectDestroyed;
            EnemyController.OnEnemyDied            -= HandleEnemyDied;
        }

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// Keep in sync with LevelAnalyticsBridge.SetCurrentLevel so all bridges
        /// report the same level/world context.
        /// </summary>
        public void SetCurrentLevel(int levelId, int worldId)
        {
            _levelId = levelId;
            _worldId = worldId;
        }

        // ── Handlers ──────────────────────────────────────────────────────────

        private void HandleKingLaunched(Vector2 direction, float power)
        {
            if (!ServiceLocator.TryGet<IAnalyticsService>(out var analytics)) return;
            if (!AnalyticsRateGuard.Allow(AnalyticsEvents.KingLaunched)) return;

            analytics.LogEvent(AnalyticsEvents.KingLaunched,
                (AnalyticsParameters.LevelId, _levelId),
                (AnalyticsParameters.Power,    power));
        }

        private void HandleKingCollision(KingProjectile king, Collision2D collision)
        {
            float speed = collision.relativeVelocity.magnitude;
            if (speed <= MinImpactSpeed) return;    // below threshold — skip

            if (!ServiceLocator.TryGet<IAnalyticsService>(out var analytics)) return;
            if (!AnalyticsRateGuard.Allow(AnalyticsEvents.KingImpact)) return;

            analytics.LogEvent(AnalyticsEvents.KingImpact,
                (AnalyticsParameters.LevelId, _levelId),
                (AnalyticsParameters.Power,    speed));
        }

        private void HandleDestructibleObjectDestroyed(DestructibleObject obj)
        {
            if (!ServiceLocator.TryGet<IAnalyticsService>(out var analytics)) return;
            if (!AnalyticsRateGuard.Allow(AnalyticsEvents.StructureDestroyed)) return;

            string material = obj.Material != null ? obj.Material.name : "unknown";

            analytics.LogEvent(AnalyticsEvents.StructureDestroyed,
                (AnalyticsParameters.Material, material),
                (AnalyticsParameters.LevelId,  _levelId));
        }

        private void HandleEnemyDied(EnemyController controller)
        {
            if (!ServiceLocator.TryGet<IAnalyticsService>(out var analytics)) return;
            if (!AnalyticsRateGuard.Allow(AnalyticsEvents.EnemyDefeated)) return;

            string enemyType = controller.GetType().Name;

            analytics.LogEvent(AnalyticsEvents.EnemyDefeated,
                (AnalyticsParameters.EnemyType, enemyType),
                (AnalyticsParameters.LevelId,   _levelId));
        }
    }
}
