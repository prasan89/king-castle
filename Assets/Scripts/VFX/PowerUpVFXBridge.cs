using UnityEngine;
using KingSmash.Core;
using KingSmash.PowerUps;
using KingSmash.Services;
using KingSmash.Audio;
using KingSmash.Camera;
using KingSmash.Gameplay;

namespace KingSmash.VFX
{
    public class PowerUpVFXBridge : MonoBehaviour
    {
        [Header("Camera Shake Profiles")]
        [SerializeField] private CameraShakeProfile _bossProfile;

        private static readonly Vector3 WorldOrigin = Vector3.zero;

        private void OnEnable()
        {
            PowerUpController.OnEffectStarted += HandleEffectStarted;
            PowerUpController.OnEffectEnded   += HandleEffectEnded;
            PowerUpController.OnEffectTick    += HandleEffectTick;
        }

        private void OnDisable()
        {
            PowerUpController.OnEffectStarted -= HandleEffectStarted;
            PowerUpController.OnEffectEnded   -= HandleEffectEnded;
            PowerUpController.OnEffectTick    -= HandleEffectTick;
        }

        private void HandleEffectStarted(PowerUpType type)
        {
            PlayVFX(MapTypeToVFX(type), WorldOrigin);
            PlaySound(MapTypeToSound(type));

            if (type == PowerUpType.MegaKing)
                TriggerShake(_bossProfile);
        }

        private void HandleEffectEnded(PowerUpType type) { }

        private void HandleEffectTick(PowerUpType type, float normalizedProgress)
        {
            if (type != PowerUpType.MegaKing) return;
            PlayVFX(VFXId.MegaKingAura, WorldOrigin);
        }

        private static VFXId MapTypeToVFX(PowerUpType type) => type switch
        {
            PowerUpType.Bomb      => VFXId.BombExplosion,
            PowerUpType.Fire      => VFXId.FireImpact,
            PowerUpType.Ice       => VFXId.IceImpact,
            PowerUpType.Lightning => VFXId.LightningImpact,
            PowerUpType.MegaKing  => VFXId.MegaKingAura,
            _                     => VFXId.DustPuff
        };

        private static SoundId MapTypeToSound(PowerUpType type) => type switch
        {
            PowerUpType.Bomb      => SoundId.PowerUpBomb,
            PowerUpType.Fire      => SoundId.PowerUpFire,
            PowerUpType.Ice       => SoundId.PowerUpIce,
            PowerUpType.Lightning => SoundId.PowerUpLightning,
            PowerUpType.MegaKing  => SoundId.PowerUpMega,
            _                     => SoundId.PowerUpActivate
        };

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
    }
}
