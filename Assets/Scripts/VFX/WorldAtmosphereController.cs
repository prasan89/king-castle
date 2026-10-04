using System.Collections;
using UnityEngine;
using KingSmash.Core;
using KingSmash.Services;
using KingSmash.Audio;

namespace KingSmash.VFX
{
    public class WorldAtmosphereController : MonoBehaviour
    {
        public enum WorldId
        {
            Forest       = 1,
            Desert       = 2,
            Ice          = 3,
            DarkRealm    = 4,
            FinalCastle  = 5
        }

        [Header("World")]
        [SerializeField] private WorldId _worldId = WorldId.Forest;

        [Header("Ambient Particles")]
        [SerializeField] private ParticleSystem[] _ambientParticles;

        [Header("Ambient Audio")]
        [SerializeField] private AudioSource _ambientAudioSource;
        [SerializeField] private float _ambientVolume = 0.3f;

        [Header("Fade")]
        [SerializeField] private float _fadeInDuration = 1.5f;

        private Coroutine _fadeCoroutine;

        private void OnEnable()
        {
            PlayAmbientParticles();
            StartAmbientAudio();
            PlayWorldMusic();
        }

        private void OnDisable()
        {
            StopAmbientParticles();
            StopAmbientAudio();
        }

        private void PlayAmbientParticles()
        {
            if (_ambientParticles == null) return;
            for (int i = 0; i < _ambientParticles.Length; i++)
            {
                if (_ambientParticles[i] != null)
                    _ambientParticles[i].Play();
            }
        }

        private void StopAmbientParticles()
        {
            if (_ambientParticles == null) return;
            for (int i = 0; i < _ambientParticles.Length; i++)
            {
                if (_ambientParticles[i] != null)
                    _ambientParticles[i].Stop();
            }
        }

        private void StartAmbientAudio()
        {
            if (_ambientAudioSource == null) return;

            if (_fadeCoroutine != null)
                StopCoroutine(_fadeCoroutine);

            _ambientAudioSource.volume = 0f;
            _ambientAudioSource.loop   = true;
            _ambientAudioSource.Play();
            _fadeCoroutine = StartCoroutine(FadeAudioIn(_fadeInDuration));
        }

        private void StopAmbientAudio()
        {
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
                _fadeCoroutine = null;
            }

            if (_ambientAudioSource != null)
            {
                _ambientAudioSource.Stop();
                _ambientAudioSource.volume = 0f;
            }
        }

        private IEnumerator FadeAudioIn(float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                if (_ambientAudioSource != null)
                    _ambientAudioSource.volume = Mathf.Lerp(0f, _ambientVolume, elapsed / duration);
                yield return null;
            }

            if (_ambientAudioSource != null)
                _ambientAudioSource.volume = _ambientVolume;

            _fadeCoroutine = null;
        }

        private void PlayWorldMusic()
        {
            var musicId = MapWorldToMusic(_worldId);
            if (ServiceLocator.TryGet<IAudioService>(out var audio))
                audio.PlayMusic(musicId);
        }

        private static SoundId MapWorldToMusic(WorldId world) => world switch
        {
            WorldId.Forest      => SoundId.World1,
            WorldId.Desert      => SoundId.World2,
            WorldId.Ice         => SoundId.World3,
            WorldId.DarkRealm   => SoundId.World4,
            WorldId.FinalCastle => SoundId.World5,
            _                   => SoundId.World1
        };
    }
}
