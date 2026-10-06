using UnityEngine;
using KingSmash.Core;
using KingSmash.Services;

namespace KingSmash.Characters
{
    // M2: Archer is a placeholder — ranged attack AI is NOT implemented this milestone.
    // Config fields (attackRange, attackInterval, etc.) are defined in ArcherConfig for M3.
    public class Archer : EnemyController
    {
        [SerializeField] private ArcherConfig _config;

        protected override void ConfigureFromScriptableObject()
        {
            if (_config == null) return;
            _health.Initialize(_config.maxHP, _config.armor);
            _health.SmallThreshold  = _config.smallHitThreshold;
            _health.MediumThreshold = _config.mediumHitThreshold;
            _health.LargeThreshold  = _config.largeHitThreshold;
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

        protected override void PlayHitAudio(HitReactionSize size)
        {
            if (ServiceLocator.TryGet<IAudioService>(out var audio) && _config != null)
                audio.PlaySfx(_config.hitSoundKey);
        }
    }
}
