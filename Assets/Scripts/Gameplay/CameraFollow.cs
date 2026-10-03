using UnityEngine;
using KingSmash.Characters;

namespace KingSmash.Gameplay
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private float _smoothSpeed = 5f;
        [SerializeField] private Vector2 _followOffset = new(2f, 1f);
        [SerializeField] private bool _followOnlyWhenLaunched = true;

        [Header("Bounds")]
        [SerializeField] private float _minX = -5f;
        [SerializeField] private float _maxX = 50f;
        [SerializeField] private float _minY = -2f;
        [SerializeField] private float _maxY = 8f;

        private bool _shouldFollow;
        private Vector3 _defaultPosition;

        private void Awake() => _defaultPosition = transform.position;

        private void OnEnable()
        {
            LaunchController.OnLaunchesRemainingChanged += HandleLaunchStarted;
        }

        private void OnDisable()
        {
            LaunchController.OnLaunchesRemainingChanged -= HandleLaunchStarted;
        }

        private void HandleLaunchStarted(int _) => _shouldFollow = true;

        private void LateUpdate()
        {
            if (_target == null || (!_shouldFollow && _followOnlyWhenLaunched)) return;

            var desired = new Vector3(
                Mathf.Clamp(_target.position.x + _followOffset.x, _minX, _maxX),
                Mathf.Clamp(_target.position.y + _followOffset.y, _minY, _maxY),
                transform.position.z);

            transform.position = Vector3.Lerp(transform.position, desired, _smoothSpeed * Time.deltaTime);
        }

        public void ResetToDefault()
        {
            _shouldFollow = false;
            transform.position = _defaultPosition;
        }

        public void SetTarget(Transform target) => _target = target;
    }
}
