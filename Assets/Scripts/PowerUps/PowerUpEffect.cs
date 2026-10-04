using UnityEngine;
using KingSmash.Characters;
using KingSmash.Physics;

namespace KingSmash.PowerUps
{
    public abstract class PowerUpEffect : MonoBehaviour
    {
        protected PowerUpConfig  Config  { get; private set; }
        protected KingProjectile King    { get; private set; }

        public bool IsRunning { get; private set; }

        public void Initialize(PowerUpConfig config, KingProjectile king)
        {
            Config = config;
            King   = king;
        }

        public void Begin()
        {
            if (IsRunning) return;
            IsRunning = true;
            OnBegin();
        }

        public void End()
        {
            if (!IsRunning) return;
            IsRunning = false;
            OnEnd();
        }

        protected abstract void OnBegin();
        protected abstract void OnEnd();

        protected float GetDamageMultiplier(DestructibleObject target)
        {
            if (Config == null || target == null || target.Material == null)
                return 1f;

            return target.Material.materialType switch
            {
                StructureMaterialType.Wood  => Config.woodMultiplier,
                StructureMaterialType.Stone => Config.stoneMultiplier,
                StructureMaterialType.Metal => Config.metalMultiplier,
                _                           => 1f
            };
        }
    }
}
