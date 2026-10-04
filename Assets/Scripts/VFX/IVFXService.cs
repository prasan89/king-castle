using UnityEngine;

namespace KingSmash.VFX
{
    public interface IVFXService
    {
        /// <summary>Spawn VFX at a world position with an optional rotation.</summary>
        void Play(VFXId id, Vector3 position, Quaternion rotation = default);

        /// <summary>Spawn VFX parented to a transform.</summary>
        void Play(VFXId id, Transform parent, bool worldPositionStays = true);

        /// <summary>Immediately deactivate all pooled VFX instances.</summary>
        void StopAll();
    }
}
