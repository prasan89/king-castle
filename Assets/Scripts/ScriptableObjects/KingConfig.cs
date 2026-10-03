using UnityEngine;

namespace KingSmash.Characters
{
    [CreateAssetMenu(fileName = "KingConfig", menuName = "KingSmash/Config/KingConfig")]
    public class KingConfig : ScriptableObject
    {
        [Header("Base Stats")]
        public float baseLaunchPower = 12f;
        public float baseLaunchSpeed = 15f;
        public float baseSmashRadius = 1.5f;
        public float baseArmor = 1f;

        [Header("Upgrade Scaling")]
        public float powerPerLevel = 0.5f;
        public float speedPerLevel = 0.3f;
        public float smashRadiusPerLevel = 0.1f;
        public float armorPerLevel = 0.2f;

        [Header("Physics")]
        public float mass = 1f;
        public float drag = 0.1f;
        public float angularDrag = 0.5f;
        public float bounceMultiplier = 0.4f;

        [Header("Visual")]
        public float launchTrailDuration = 0.5f;
        public int maxBounces = 3;

        public float GetPowerAtLevel(int level) => baseLaunchPower + powerPerLevel * (level - 1);
        public float GetSpeedAtLevel(int level) => baseLaunchSpeed + speedPerLevel * (level - 1);
        public float GetSmashRadiusAtLevel(int level) => baseSmashRadius + smashRadiusPerLevel * (level - 1);
        public float GetArmorAtLevel(int level) => baseArmor + armorPerLevel * (level - 1);

        [Header("Combat")]
        public float maxHP = 200f;
        public float hpPerLevel = 20f;
        public float impactDamage = 30f;
        public float impactDamagePerLevel = 5f;
        public float stunDurationOnHeavyHit = 1.5f;
        public float GetMaxHPAtLevel(int level) => maxHP + hpPerLevel * (level - 1);
        public float GetImpactDamageAtLevel(int level) => impactDamage + impactDamagePerLevel * (level - 1);
    }
}
