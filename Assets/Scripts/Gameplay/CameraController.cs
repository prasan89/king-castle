using System.Collections;
using UnityEngine;
using KingSmash.Characters;

namespace KingSmash.Gameplay
{
    /// Smooth camera controller for the Level scene.
    /// Follows launched King, returns to default on landing/expend.
    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour
    {
        [Header("Follow")]
        [SerializeField] private float _followSpeed = 5f;
        [SerializeField] private Vector3 _followOffset = new(1.5f, 1f, -10f);

        [Header("Bounds")]
        [SerializeField] private float _minX = -2f;
        [SerializeField] private float _maxX  = 24f;
        [SerializeField] private float _minY = -1f;
        [SerializeField] private float _maxY  = 10f;

        [Header("Return")]
        [SerializeField] private float _returnSpeed = 3f;
        [SerializeField] private float _returnDelay = 1.2f;

        [Header("Shake")]
        [SerializeField] private float _shakeMagnitude = 0.12f;
        [SerializeField] private float _shakeDuration = 0.18f;

        private Transform _target;
        private Vector3 _defaultPosition;
        private bool _following;
        private bool _shaking;

        private void Awake()
        {
            _defaultPosition = transform.position;
        }

        private void OnEnable()
        {
            KingProjectile.OnKingCollision += HandleKingCollision;
            KingProjectile.OnKingLanded    += HandleKingLanded;
            LaunchController.OnKingLaunched += HandleKingLaunched;
        }

        private void OnDisable()
        {
            KingProjectile.OnKingCollision -= HandleKingCollision;
            KingProjectile.OnKingLanded    -= HandleKingLanded;
            LaunchController.OnKingLaunched -= HandleKingLaunched;
        }

        private void HandleKingLaunched(Vector2 _, float __)
        {
            // Target assigned by LaunchController via SetTarget
            _following = true;
        }

        private void HandleKingCollision(KingProjectile king, UnityEngine.Collision2D col)
        {
            if (!_shaking) StartCoroutine(Shake());
        }

        private void HandleKingLanded(KingProjectile _)
        {
            StartCoroutine(ReturnToDefault());
        }

        public void SetTarget(Transform target)
        {
            _target = target;
        }

        private void LateUpdate()
        {
            if (!_following || _target == null) return;

            var desired = new Vector3(
                Mathf.Clamp(_target.position.x + _followOffset.x, _minX, _maxX),
                Mathf.Clamp(_target.position.y + _followOffset.y, _minY, _maxY),
                _followOffset.z);

            transform.position = Vector3.Lerp(transform.position, desired, _followSpeed * Time.deltaTime);
        }

        private IEnumerator ReturnToDefault()
        {
            yield return new WaitForSeconds(_returnDelay);
            _following = false;
            float elapsed = 0f;
            var start = transform.position;
            var end   = new Vector3(_defaultPosition.x, _defaultPosition.y, _followOffset.z);
            float duration = 0.8f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                transform.position = Vector3.Lerp(start, end, elapsed / duration);
                yield return null;
            }
            transform.position = end;
        }

        private IEnumerator Shake()
        {
            _shaking = true;
            var origin = transform.position;
            float elapsed = 0f;
            while (elapsed < _shakeDuration)
            {
                elapsed += Time.deltaTime;
                float t = 1f - elapsed / _shakeDuration;
                transform.position = origin + (Vector3)UnityEngine.Random.insideUnitCircle * _shakeMagnitude * t;
                yield return null;
            }
            transform.position = origin;
            _shaking = false;
        }

        public void ResetToDefault()
        {
            StopAllCoroutines();
            _following = false;
            _shaking   = false;
            transform.position = new Vector3(_defaultPosition.x, _defaultPosition.y, _followOffset.z);
        }
    }
}
