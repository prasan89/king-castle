using System;
using UnityEngine;
using KingSmash.Core;

namespace KingSmash.PowerUps
{
    public static class PowerUpFactory
    {
        public static PowerUpConfig LoadConfig(PowerUpType type)
        {
            string path   = $"PowerUps/PowerUpConfig_{type}";
            var    config = Resources.Load<PowerUpConfig>(path);
            if (config == null)
                GameLogger.Warning("PowerUpFactory", $"PowerUpConfig not found at Resources/{path}");
            return config;
        }

        public static PowerUpConfig[] LoadAllConfigs()
        {
            var types   = (PowerUpType[])Enum.GetValues(typeof(PowerUpType));
            var configs = new PowerUpConfig[types.Length];
            for (int i = 0; i < types.Length; i++)
                configs[i] = LoadConfig(types[i]);
            return configs;
        }

        public static Type GetEffectType(PowerUpType type) => type switch
        {
            PowerUpType.Bomb      => typeof(BombSmashEffect),
            PowerUpType.Fire      => typeof(FireSmashEffect),
            PowerUpType.Ice       => typeof(IceSmashEffect),
            PowerUpType.Lightning => typeof(LightningSmashEffect),
            PowerUpType.MegaKing  => typeof(MegaKingEffect),
            _                     => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        public static PowerUpEffect CreateEffect(PowerUpType type, GameObject host) =>
            (PowerUpEffect)host.AddComponent(GetEffectType(type));
    }
}
