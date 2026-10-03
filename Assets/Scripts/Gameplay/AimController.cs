using System;
using UnityEngine;
using KingSmash.Physics;
using KingSmash.Core;

namespace KingSmash.Gameplay
{
    /// Handles touch/mouse drag → aim direction + power.
    /// Fires events consumed by TrajectoryPreview and LaunchController.
    public class AimController : MonoBehaviour
    {
        [SerializeField] private ProjectileConfig _projectileConfig;
        [SerializeField] private Transform _launchPoint;
        [SerializeField] private bool _requiresLaunchZoneTap = false;
        [SerializeField] private float _launchZoneScreenFraction = 0.4f;

        private Vector2 _touchStart;
        private Vector2 _currentAimDirection;
        private float _currentPower;
        private bool _isAiming;
        private bool _inputEnabled = true;

        // direction, power, launchWorldPos, gravityScale
        public static event Action<Vector2, float, Vector2, float> OnAimUpdated;
        public static event Action<Vector2, float> OnLaunchReleased;
        public static event Action OnAimCancelled;
        public static event Action OnAimStarted;

        public bool IsAiming => _isAiming;
        public Vector2 AimDirection => _currentAimDirection;
        public float AimPower => _currentPower;

        public void SetInputEnabled(bool enabled)
        {
            _inputEnabled = enabled;
            if (!enabled) CancelAim();
        }

        private void Update()
        {
            if (!_inputEnabled) return;

            if (Input.touchCount > 0)
                ProcessTouch(Input.GetTouch(0));
#if UNITY_EDITOR
            else
                ProcessMouseInput();
#endif
        }

        private void ProcessTouch(Touch touch)
        {
            switch (touch.phase)
            {
                case TouchPhase.Began:      StartAim(touch.position);   break;
                case TouchPhase.Moved:
                case TouchPhase.Stationary: UpdateAim(touch.position);   break;
                case TouchPhase.Ended:      ReleaseAim(touch.position);  break;
                case TouchPhase.Canceled:   CancelAim();                 break;
            }
        }

#if UNITY_EDITOR
        private void ProcessMouseInput()
        {
            if (Input.GetMouseButtonDown(0))  StartAim(Input.mousePosition);
            else if (Input.GetMouseButton(0)) UpdateAim(Input.mousePosition);
            else if (Input.GetMouseButtonUp(0)) ReleaseAim(Input.mousePosition);
        }
#endif

        private bool IsInLaunchZone(Vector2 screenPos)
        {
            if (!_requiresLaunchZoneTap) return true;
            return screenPos.y < Screen.height * _launchZoneScreenFraction;
        }

        private void StartAim(Vector2 screenPos)
        {
            if (!IsInLaunchZone(screenPos)) return;
            _touchStart = screenPos;
            _isAiming = true;
            OnAimStarted?.Invoke();
        }

        private void UpdateAim(Vector2 screenPos)
        {
            if (!_isAiming) return;

            var delta = _touchStart - screenPos;
            _currentAimDirection = delta.normalized;

            // Power scales with drag distance — normalize to screen height for resolution independence
            float maxDragPx = Screen.height * 0.28f;
            float t = Mathf.Clamp01(delta.magnitude / maxDragPx);
            _currentPower = Mathf.Lerp(_projectileConfig.minLaunchPower, _projectileConfig.maxLaunchPower, t);

            Vector2 launchWorldPos = _launchPoint != null ? (Vector2)_launchPoint.position : Vector2.zero;
            float gravityScale = _projectileConfig.gravityScale;
            OnAimUpdated?.Invoke(_currentAimDirection, _currentPower, launchWorldPos, gravityScale);
        }

        private void ReleaseAim(Vector2 screenPos)
        {
            if (!_isAiming) return;
            UpdateAim(screenPos);
            _isAiming = false;

            if (_currentPower > _projectileConfig.minLaunchPower)
                OnLaunchReleased?.Invoke(_currentAimDirection, _currentPower);
            else
                OnAimCancelled?.Invoke();
        }

        private void CancelAim()
        {
            _isAiming = false;
            OnAimCancelled?.Invoke();
            GameLogger.Debug("AimController", "Aim cancelled.");
        }
    }
}
