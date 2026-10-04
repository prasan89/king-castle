using System.Collections;
using UnityEngine;
using KingSmash.Gameplay;

namespace KingSmash.Camera
{
    /// <summary>
    /// MonoBehaviour camera-effect service.
    /// Drives shake by injecting an additive offset each frame.
    /// If a CameraController is present it calls CameraController.AddShakeOffset()
    /// so the follow logic and shake are fully independent.
    /// Otherwise it offsets the main Camera transform directly.
    /// </summary>
    [DefaultExecutionOrder(-800)]
    public class CameraEffectService : MonoBehaviour, ICameraEffectService
    {
        [SerializeField] private UnityEngine.Camera _camera;

        private CameraController _cameraController;

        // Shake state
        private Coroutine _shakeCoroutine;
        private Vector3   _shakeOffset;

        // ─────────────────────────────────────────────────────────────────────
        #region Unity lifecycle

        private void Awake()
        {
            if (_camera == null)
                _camera = UnityEngine.Camera.main;

            if (_camera != null)
                _cameraController = _camera.GetComponent<CameraController>();
        }

        private void LateUpdate()
        {
            // If we are NOT using CameraController, apply the additive offset directly.
            // (When CameraController is present, offsets are injected in the coroutine per frame.)
            if (_cameraController == null && _camera != null)
            {
                // No-op here — offset applied inside coroutine via transform adjustment.
            }
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────
        #region ICameraEffectService

        public void Shake(CameraShakeProfile profile)
        {
            if (profile == null) return;
            BeginShake(profile.duration, profile.magnitude, profile.envelope);
        }

        public void ShakeImmediate(float duration, float magnitude)
        {
            // Inline profile — decaying linear envelope
            AnimationCurve envelope = AnimationCurve.Linear(0f, 1f, 1f, 0f);
            BeginShake(duration, magnitude, envelope);
        }

        public void StopShake()
        {
            if (_shakeCoroutine != null)
            {
                StopCoroutine(_shakeCoroutine);
                _shakeCoroutine = null;
            }
            ApplyOffset(Vector3.zero);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────
        #region Internal helpers

        private void BeginShake(float duration, float magnitude, AnimationCurve envelope)
        {
            // Allow interrupting / restarting an existing shake
            if (_shakeCoroutine != null)
                StopCoroutine(_shakeCoroutine);

            _shakeCoroutine = StartCoroutine(RunShake(duration, magnitude, envelope));
        }

        private IEnumerator RunShake(float duration, float magnitude, AnimationCurve envelope)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t       = Mathf.Clamp01(elapsed / duration);
                float scale   = envelope.Evaluate(t);
                Vector2 rand  = Random.insideUnitCircle;
                _shakeOffset  = new Vector3(rand.x, rand.y, 0f) * magnitude * scale;

                ApplyOffset(_shakeOffset);
                yield return null;
            }

            ApplyOffset(Vector3.zero);
            _shakeOffset    = Vector3.zero;
            _shakeCoroutine = null;
        }

        private void ApplyOffset(Vector3 offset)
        {
            if (_cameraController != null)
            {
                _cameraController.AddShakeOffset(offset);
            }
            // Direct-transform path: no action needed — CameraController manages
            // the actual transform each frame; without it we are on a standalone camera.
            // In that case we store the offset and a separate LateUpdate pass applies it.
            // For simplicity with a standalone camera, store and apply immediately.
            else if (_camera != null)
            {
                // Remove previous shake offset and add new one.
                // We do this by keeping track of the previous offset.
                _camera.transform.position =
                    _camera.transform.position - _prevDirectOffset + offset;
                _prevDirectOffset = offset;
            }
        }

        private Vector3 _prevDirectOffset;

        #endregion
    }
}
