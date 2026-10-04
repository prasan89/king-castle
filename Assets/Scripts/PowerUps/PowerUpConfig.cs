using UnityEngine;

namespace KingSmash.PowerUps
{
    [CreateAssetMenu(fileName = "PowerUpConfig", menuName = "KingSmash/Config/PowerUpConfig")]
    public class PowerUpConfig : ScriptableObject
    {
        [Header("Identity")]
        public PowerUpType powerUpType;
        public string displayName;
        public Sprite icon;

        [Header("Economy")]
        public long purchaseCost;
        public int starterGrantCount = 1;

        [Header("Damage")]
        [Tooltip("Base damage applied at activation")]
        public float baseDamage = 50f;
        [Tooltip("Damage multiplier for Wood material")]
        public float woodMultiplier = 1.0f;
        [Tooltip("Damage multiplier for Stone material")]
        public float stoneMultiplier = 0.8f;
        [Tooltip("Damage multiplier for Metal material")]
        public float metalMultiplier = 0.6f;

        [Header("Effect")]
        [Tooltip("Duration of the ongoing effect in seconds (0 = instant)")]
        public float effectDuration = 0f;
        [Tooltip("Blast radius for area effects (0 = single target)")]
        public float blastRadius = 0f;
        [Tooltip("Damage-over-time per second for fire effect")]
        public float dotDamagePerSecond = 0f;
        [Tooltip("Number of chain targets for lightning effect")]
        public int chainTargets = 0;
        [Tooltip("King mass multiplier for MegaKing (1 = no change)")]
        public float kingMassMultiplier = 1f;
        [Tooltip("King launch power multiplier for MegaKing (1 = no change)")]
        public float kingPowerMultiplier = 1f;

        [Header("Visual")]
        public string activateParticleKey = "";
        public string activateSoundKey = "";
    }
}
