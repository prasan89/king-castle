using UnityEngine;
using KingSmash.Core;
using KingSmash.Physics;
using KingSmash.Services;
using KingSmash.Audio;
using KingSmash.Camera;

namespace KingSmash.VFX
{
    public class DestructionVFXBridge : MonoBehaviour
    {
        private const float DamageVFXThreshold = 20f;

        [Header("Camera Shake Profiles")]
        [SerializeField] private CameraShakeProfile _smallProfile;
        [SerializeField] private CameraShakeProfile _mediumProfile;

        private void OnEnable()
        {
            DestructibleObject.OnDestroyed += HandleDestroyed;
            DestructibleObject.OnDamaged   += HandleDamaged;
            CastleStructure.OnCastleDestroyed += HandleCastleDestroyed;
        }

        private void OnDisable()
        {
            DestructibleObject.OnDestroyed -= HandleDestroyed;
            DestructibleObject.OnDamaged   -= HandleDamaged;
            CastleStructure.OnCastleDestroyed -= HandleCastleDestroyed;
        }

        private void HandleDamaged(DestructibleObject obj, float damage)
        {
            if (damage < DamageVFXThreshold) return;
            if (obj == null) return;

            var matType = obj.Material != null ? obj.Material.materialType : StructureMaterialType.Wood;
            Vector3 pos = obj.transform.position;

            PlayVFX(MapMaterialToVFX(matType), pos);
            PlaySound(MapMaterialToImpactSound(matType));
        }

        private void HandleDestroyed(DestructibleObject obj)
        {
            if (obj == null) return;

            var matType = obj.Material != null ? obj.Material.materialType : StructureMaterialType.Wood;
            Vector3 pos = obj.transform.position;

            PlayVFX(MapMaterialToBreakVFX(matType), pos);
            PlaySound(MapMaterialToBreakSound(matType));
            TriggerShake(_smallProfile);
        }

        private void HandleCastleDestroyed(CastleStructure castle)
        {
            PlaySound(SoundId.CastleCollapse);
            TriggerShake(_mediumProfile);
        }

        private static VFXId MapMaterialToVFX(StructureMaterialType mat) => mat switch
        {
            StructureMaterialType.Wood   => VFXId.WoodBreak,
            StructureMaterialType.Stone  => VFXId.StoneBreak,
            StructureMaterialType.Metal  => VFXId.MetalImpact,
            StructureMaterialType.Ice    => VFXId.IceBreak,
            StructureMaterialType.Glass  => VFXId.SparkFlash,
            StructureMaterialType.Rubber => VFXId.DustPuff,
            _                            => VFXId.DustPuff
        };

        private static VFXId MapMaterialToBreakVFX(StructureMaterialType mat) => mat switch
        {
            StructureMaterialType.Wood   => VFXId.WoodBreak,
            StructureMaterialType.Stone  => VFXId.StoneBreak,
            StructureMaterialType.Metal  => VFXId.MetalImpact,
            StructureMaterialType.Ice    => VFXId.IceBreak,
            StructureMaterialType.Glass  => VFXId.SparkFlash,
            StructureMaterialType.Rubber => VFXId.DustPuff,
            _                            => VFXId.DustPuff
        };

        private static SoundId MapMaterialToImpactSound(StructureMaterialType mat) => mat switch
        {
            StructureMaterialType.Wood   => SoundId.WoodBreak,
            StructureMaterialType.Stone  => SoundId.StoneBreak,
            StructureMaterialType.Metal  => SoundId.MetalImpact,
            StructureMaterialType.Ice    => SoundId.IceBreak,
            _                            => SoundId.WoodBreak
        };

        private static SoundId MapMaterialToBreakSound(StructureMaterialType mat) => mat switch
        {
            StructureMaterialType.Wood   => SoundId.WoodBreak,
            StructureMaterialType.Stone  => SoundId.StoneBreak,
            StructureMaterialType.Metal  => SoundId.MetalImpact,
            StructureMaterialType.Ice    => SoundId.IceBreak,
            _                            => SoundId.WoodBreak
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
