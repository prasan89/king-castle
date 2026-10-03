using UnityEngine;
using KingSmash.Gameplay;
using KingSmash.Services;
using KingSmash.Core;

namespace KingSmash.Characters
{
    public class EnemyKing : EnemyController
    {
        [SerializeField] private EnemyKingConfig _config;

        protected override void ConfigureFromScriptableObject()
        {
            if (_config == null) return;
            _health.Initialize(_config.maxHP, _config.armor);
            _health.SmallThreshold  = _config.smallHitThreshold;
            _health.MediumThreshold = _config.mediumHitThreshold;
            _health.LargeThreshold  = _config.largeHitThreshold;
            if (_rb != null) _rb.mass = _config.mass;
        }

        public override void ReceiveDamage(IDamageSource source)
        {
            if (!IsAlive) return;
            // Boss reduces incoming damage by impactResistance fraction
            float resistance = _config != null ? _config.impactResistance : 0f;
            float reduced = source.DamageAmount * (1f - resistance);
            _health.TakeDamage(reduced);
            ApplyKnockback(source.ImpactForce);
            PlayBossHitAudio();
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
            GameLogger.Info("EnemyKing", "Enemy King defeated!");
        }

        private void PlayBossHitAudio()
        {
            if (ServiceLocator.TryGet<IAudioService>(out var audio) && _config != null)
                audio.PlaySfx(_config.hitSoundKey);
        }
    }
}
