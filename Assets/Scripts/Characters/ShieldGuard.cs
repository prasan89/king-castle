using System;
using UnityEngine;
using KingSmash.Core;
using KingSmash.Gameplay;
using KingSmash.Services;

namespace KingSmash.Characters
{
    public class ShieldGuard : EnemyController
    {
        [SerializeField] private ShieldGuardConfig _config;
        [SerializeField] private GameObject _shieldVisual;

        private float _shieldCurrentHP;
        private bool  _shieldBroken;

        public static event Action<ShieldGuard> OnShieldBroken;

        public bool ShieldActive => !_shieldBroken && _shieldCurrentHP > 0f;
        public float ShieldNormalized => _config != null ? _shieldCurrentHP / _config.shieldMaxHP : 0f;

        protected override void ConfigureFromScriptableObject()
        {
            if (_config == null) return;
            _health.Initialize(_config.bodyMaxHP, _config.bodyArmor);
            _health.SmallThreshold  = _config.smallHitThreshold;
            _health.MediumThreshold = _config.mediumHitThreshold;
            _health.LargeThreshold  = _config.largeHitThreshold;
            _shieldCurrentHP = _config.shieldMaxHP;
        }

        public override void ReceiveDamage(IDamageSource source)
        {
            if (!IsAlive) return;
            if (ShieldActive)
            {
                float reduced = source.DamageAmount * (1f - (_config?.shieldDamageReduction ?? 0.8f));
                _shieldCurrentHP -= source.DamageAmount - reduced; // shield absorbs the reduction
                _health.TakeDamage(reduced);
                ApplyKnockback(source.ImpactForce);
                PlayShieldHitAudio();

                if (_shieldCurrentHP <= 0f) BreakShield();
            }
            else
            {
                base.ReceiveDamage(source);
            }
        }

        private void BreakShield()
        {
            _shieldBroken = true;
            if (_shieldVisual != null) _shieldVisual.SetActive(false);
            GameLogger.Info("ShieldGuard", $"{name} shield broken!");
            if (ServiceLocator.TryGet<IAudioService>(out var audio) && _config != null)
                audio.PlaySfx(_config.shieldBreakSoundKey);
            OnShieldBroken?.Invoke(this);
        }

        private void PlayShieldHitAudio()
        {
            if (ServiceLocator.TryGet<IAudioService>(out var audio) && _config != null)
                audio.PlaySfx(_config.shieldHitSoundKey);
        }

        protected override void ApplyKnockback(Vector2 force)
        {
            float mult = _config != null ? _config.knockbackMultiplier : 1f;
            base.ApplyKnockback(force * mult);
        }

        protected override void OnDeath()
        {
            if (ServiceLocator.TryGet<IAudioService>(out var audio) && _config != null)
                audio.PlaySfx(_config.deathSoundKey);
        }
    }
}
