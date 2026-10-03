using UnityEngine;
namespace KingSmash.Characters
{
    [CreateAssetMenu(fileName = "ShieldGuardConfig", menuName = "KingSmash/Config/Enemies/ShieldGuardConfig")]
    public class ShieldGuardConfig : ScriptableObject
    {
        [Header("Body Health")]
        public float bodyMaxHP = 60f;
        public float bodyArmor = 0.5f;
        [Header("Shield Health")]
        public float shieldMaxHP = 100f;
        public float shieldDamageReduction = 0.8f;   // 0 = full damage, 1 = immune
        [Header("Hit Reaction")]
        public float smallHitThreshold  = 15f;
        public float mediumHitThreshold = 40f;
        public float largeHitThreshold  = 80f;
        public float knockbackMultiplier = 0.6f;
        [Header("Rewards")]
        public int scoreOnDeath = 250;
        public int coinsOnDeath = 15;
        [Header("Audio / VFX")]
        public string shieldHitSoundKey   = "shield_hit";
        public string shieldBreakSoundKey = "shield_break";
        public string hitSoundKey         = "enemy_hit";
        public string deathSoundKey       = "enemy_death";
        public string shieldBreakVFXKey   = "shield_break_vfx";
        public string deathVFXKey         = "enemy_death_vfx";
    }
}
