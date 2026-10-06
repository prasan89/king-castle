using System.Collections;
using UnityEngine;
using KingSmash.Core;
using KingSmash.Services;

namespace KingSmash.World
{
    public class WorldThemeApplicator : MonoBehaviour
    {
        [SerializeField] private WorldTheme              _theme;
        [SerializeField] private UnityEngine.Camera      _mainCamera;
        [SerializeField] private SpriteRenderer          _skyBackground;
        [SerializeField] private ParticleSystem          _ambientParticles;

        private void OnEnable()  => ApplyTheme();
        private void OnDisable() => StopAmbientParticles();

        public void ApplyTheme()
        {
            if (_theme == null)
            {
                GameLogger.Warning("WorldThemeApplicator", "No WorldTheme assigned.");
                return;
            }

            ApplyCameraBackground();
            ApplySkyBackground();
            ApplyAmbientParticles();
            StartAmbientParticles();
            PlayWorldMusic();
        }

        public void SetTheme(WorldTheme theme)
        {
            _theme = theme;
            ApplyTheme();
        }

        private void ApplyCameraBackground()
        {
            if (_mainCamera == null)
                _mainCamera = UnityEngine.Camera.main;

            if (_mainCamera == null) return;
            _mainCamera.backgroundColor = Color.Lerp(_theme.skyColorBottom, _theme.skyColorTop, 0.5f);
        }

        private void ApplySkyBackground()
        {
            if (_skyBackground == null) return;
            _skyBackground.color = _theme.skyColorBottom;
        }

        private void ApplyAmbientParticles()
        {
            if (_ambientParticles == null || _theme.particleColors == null) return;

            var main     = _ambientParticles.main;
            var emission = _ambientParticles.emission;
            float baseRate      = emission.rateOverTime.constant;
            float effectiveBase = baseRate > 0f ? baseRate : 20f;
            emission.rateOverTime = new ParticleSystem.MinMaxCurve(effectiveBase * _theme.particleDensity);

            if (_theme.particleColors.Length >= 3)
            {
                var gradient = new Gradient();
                gradient.SetKeys(
                    new GradientColorKey[]
                    {
                        new GradientColorKey(_theme.particleColors[0], 0.0f),
                        new GradientColorKey(_theme.particleColors[1], 0.5f),
                        new GradientColorKey(_theme.particleColors[2], 1.0f)
                    },
                    new GradientAlphaKey[]
                    {
                        new GradientAlphaKey(_theme.particleColors[0].a, 0.0f),
                        new GradientAlphaKey(_theme.particleColors[1].a, 0.5f),
                        new GradientAlphaKey(_theme.particleColors[2].a, 1.0f)
                    }
                );
                main.startColor = new ParticleSystem.MinMaxGradient(gradient);
            }
            else if (_theme.particleColors.Length > 0)
            {
                main.startColor = new ParticleSystem.MinMaxGradient(_theme.particleColors[0]);
            }
        }

        private void StartAmbientParticles()
        {
            if (_ambientParticles == null) return;
            if (!_ambientParticles.isPlaying)
                _ambientParticles.Play();
        }

        private void StopAmbientParticles()
        {
            if (_ambientParticles == null) return;
            _ambientParticles.Stop();
        }

        private void PlayWorldMusic()
        {
            if (_theme == null) return;
            if (ServiceLocator.TryGet<IAudioService>(out var audio))
                audio.PlayMusic(_theme.worldMusic);
        }
    }
}
