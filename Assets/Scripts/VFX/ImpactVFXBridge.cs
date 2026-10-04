using UnityEngine;
using KingSmash.Core;
using KingSmash.Characters;
using KingSmash.Services;
using KingSmash.Audio;
using KingSmash.Camera;
using KingSmash.Gameplay;

namespace KingSmash.VFX
{
    public class ImpactVFXBridge : MonoBehaviour
    {
        private const float SpeedThresholdMedium = 5f;
        private const float SpeedThresholdHeavy  = 15f;

        private const float HitStopLight  = 0.04f;
        private const float HitStopMedium = 0.08f;

        [Header("Camera Shake Profiles")]
        [SerializeField] private CameraShakeProfile _subtleProfile;
        [SerializeField] private CameraShakeProfile _smallProfile;
        [SerializeField] private CameraShakeProfile _mediumProfile;

        private void OnEnable()  => KingProjectile.OnKingCollision += HandleKingCollision;
        private void OnDisable() => KingProjectile.OnKingCollision -= HandleKingCollision;

        private void HandleKingCollision(KingProjectile king, Collision2D collision)
        {
            float speed = collision.relativeVelocity.magnitude;

            Vector3 contactPoint = collision.contacts.Length > 0
                ? (Vector3)collision.contacts[0].point
                : king.transform.position;

            if (speed >= SpeedThresholdHeavy)
                FireImpact(contactPoint, ImpactTier.Heavy);
            else if (speed >= SpeedThresholdMedium)
                FireImpact(contactPoint, ImpactTier.Medium);
            else
                FireImpact(contactPoint, ImpactTier.Small);
        }

        private enum ImpactTier { Small, Medium, Heavy }

        private void FireImpact(Vector3 position, ImpactTier tier)
        {
            VFXId vfxId = tier switch
            {
                ImpactTier.Heavy  => VFXId.KingImpactHeavy,
                ImpactTier.Medium => VFXId.KingImpactMedium,
                _                 => VFXId.KingImpactSmall
            };

            SoundId soundId = tier switch
            {
                ImpactTier.Heavy  => SoundId.KingImpactHeavy,
                ImpactTier.Medium => SoundId.KingImpactMedium,
                _                 => SoundId.KingImpactSmall
            };

            CameraShakeProfile shakeProfile = tier switch
            {
                ImpactTier.Heavy  => _mediumProfile,
                ImpactTier.Medium => _smallProfile,
                _                 => _subtleProfile
            };

            PlayVFX(vfxId, position);
            PlaySound(soundId);
            TriggerShake(shakeProfile);

            if (tier == ImpactTier.Medium)
                TriggerHitStop(HitStopLight);
            else if (tier == ImpactTier.Heavy)
                TriggerHitStop(HitStopMedium);
        }

        private static void PlayVFX(VFXId id, Vector3 position)
        {
            if (ServiceLocator.TryGet<IVFXService>(out var vfx))
                vfx.Play(id, position);
        }

        private static void PlaySound(SoundId id)
        {
            if (ServiceLocator.TryGet<IAudioService>(out var audio))
                audio.Play(id);
        }

        private void TriggerShake(CameraShakeProfile profile)
        {
            if (profile == null) return;
            if (ServiceLocator.TryGet<ICameraEffectService>(out var cam))
                cam.Shake(profile);
        }

        private static void TriggerHitStop(float duration)
        {
            if (ServiceLocator.TryGet<IHitStopService>(out var hs))
                hs.Trigger(duration);
        }
    }
}
