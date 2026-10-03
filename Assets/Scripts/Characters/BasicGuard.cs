using UnityEngine;
using KingSmash.Services;

namespace KingSmash.Characters
{
    public class BasicGuard : EnemyController
    {
        [SerializeField] private BasicGuardConfig _config;

        protected override void ConfigureFromScriptableObject()
        {
            if (_config == null) return;
            _health.Initialize(_config.maxHP, _config.armor);
            _health.SmallThreshold  = _config.smallHitThreshold;
            _health.MediumThreshold = _config.mediumHitThreshold;
            _health.LargeThreshold  = _config.largeHitThreshold;
        }

        protected override void ApplyKnockback(UnityEngine.Vector2 force)
        {
            float mult = _config != null ? _config.knockbackMultiplier : 1f;
            base.ApplyKnockback(force * mult);
        }

        protected override void OnDeath()
        {
            if (ServiceLocator.TryGet<IAudioService>(out var audio) && _config != null)
                audio.PlaySfx(_config.deathSoundKey);
        }

        protected override void PlayHitAudio(HitReactionSize size)
        {
            if (ServiceLocator.TryGet<IAudioService>(out var audio) && _config != null)
                audio.PlaySfx(_config.hitSoundKey);
        }
    }
}
