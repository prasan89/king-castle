using System;
using System.Collections;
using UnityEngine;
using KingSmash.Audio;
using KingSmash.Camera;
using KingSmash.Characters;
using KingSmash.Core;
using KingSmash.Services;
using KingSmash.VFX;

namespace KingSmash.World
{
    public class QueenRescueSequencer : MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField] private ScreenFlashEffect   _screenFlash;
        [SerializeField] private CameraShakeProfile  _bossShakeProfile;

        public static event Action OnSequenceComplete;

        private void OnEnable()  => QueenController.OnQueenRescued += HandleQueenRescued;
        private void OnDisable() => QueenController.OnQueenRescued -= HandleQueenRescued;

        private void HandleQueenRescued(QueenController queen)
        {
            StartCoroutine(PlayRescueSequence(queen.transform));
        }

        private IEnumerator PlayRescueSequence(Transform queenTransform)
        {
            if (ServiceLocator.TryGet<ICameraEffectService>(out var cameraFx))
            {
                if (_bossShakeProfile != null)
                    cameraFx.Shake(_bossShakeProfile);
                else
                    cameraFx.ShakeImmediate(0.6f, 0.25f);
            }
            yield return new WaitForSeconds(0.3f);

            if (_screenFlash != null)
                _screenFlash.Flash(new Color(1f, 0.85f, 0.10f, 1f), 0.5f);
            yield return new WaitForSeconds(0.5f);

            if (ServiceLocator.TryGet<IAudioService>(out var audio))
                audio.Play(SoundId.QueenRescue);

            if (ServiceLocator.TryGet<IVFXService>(out var vfx) && queenTransform != null)
                vfx.Play(VFXId.QueenRescue, queenTransform.position);

            yield return new WaitForSeconds(0.8f);

            yield return StartCoroutine(ZoomCamera(queenTransform, 0.8f));
            yield return new WaitForSeconds(0.2f);

            if (ServiceLocator.TryGet<IAudioService>(out var audioStep5))
                audioStep5.PlayMusic(SoundId.Victory);
            yield return new WaitForSeconds(1.5f);

            OnSequenceComplete?.Invoke();
        }

        private IEnumerator ZoomCamera(Transform target, float duration)
        {
            var cam = Camera.main;
            if (cam == null || !cam.orthographic) yield break;

            float startSize = cam.orthographicSize;
            float endSize   = Mathf.Max(1f, startSize - 1.5f);

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
                cam.orthographicSize = Mathf.Lerp(startSize, endSize, t);
                yield return null;
            }

            cam.orthographicSize = endSize;
        }
    }
}
