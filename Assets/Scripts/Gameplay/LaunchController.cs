using System;
using UnityEngine;
using KingSmash.Characters;
using KingSmash.Core;
using KingSmash.Services;

namespace KingSmash.Gameplay
{
    /// Spawns King projectiles, counts launches, routes analytics.
    public class LaunchController : MonoBehaviour
    {
        [SerializeField] private KingProjectile _kingPrefab;
        [SerializeField] private Transform _launchPoint;
        [SerializeField] private AimController _aimController;
        [SerializeField] private CameraController _cameraController;

        private int _launchesRemaining;
        private KingProjectile _activeKing;

        public static event Action<int> OnLaunchesRemainingChanged;
        public static event Action OnAllLaunchesExpended;
        public static event Action<Vector2, float> OnKingLaunched;

        public int LaunchesRemaining => _launchesRemaining;
        public bool CanLaunch => _launchesRemaining > 0 && (_activeKing == null || _activeKing.HasLanded);

        private void OnEnable()
        {
            AimController.OnLaunchReleased += HandleLaunchReleased;
            KingProjectile.OnKingLanded    += HandleKingLanded;
        }

        private void OnDisable()
        {
            AimController.OnLaunchReleased -= HandleLaunchReleased;
            KingProjectile.OnKingLanded    -= HandleKingLanded;
        }

        public void Initialize(int launches)
        {
            _launchesRemaining = launches;
            OnLaunchesRemainingChanged?.Invoke(_launchesRemaining);
        }

        private void HandleLaunchReleased(Vector2 direction, float power)
        {
            if (!CanLaunch) return;
            PerformLaunch(direction, power);
        }

        private void HandleKingLanded(KingProjectile king)
        {
            // Re-enable aiming once King has landed if launches remain
            if (_launchesRemaining > 0)
                _aimController?.SetInputEnabled(true);
        }

        private void PerformLaunch(Vector2 direction, float power)
        {
            if (_kingPrefab == null)
            {
                GameLogger.Warning("LaunchController", "KingPrefab not assigned.");
                return;
            }
            _aimController?.SetInputEnabled(false);

            _activeKing = Instantiate(_kingPrefab, _launchPoint.position, Quaternion.identity);
            _activeKing.Launch(direction, power);
            _cameraController?.SetTarget(_activeKing.transform);
            _launchesRemaining--;

            OnKingLaunched?.Invoke(direction, power);
            OnLaunchesRemainingChanged?.Invoke(_launchesRemaining);

            ServiceLocator.Get<IAnalyticsService>().LogEvent(
                AnalyticsEvents.KingLaunched,
                ("power", power),
                ("launches_remaining", _launchesRemaining));

            if (_launchesRemaining <= 0)
                OnAllLaunchesExpended?.Invoke();
        }
    }
}
