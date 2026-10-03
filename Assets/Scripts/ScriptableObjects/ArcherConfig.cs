using UnityEngine;
namespace KingSmash.Characters
{
    [CreateAssetMenu(fileName = "ArcherConfig", menuName = "KingSmash/Config/Enemies/ArcherConfig")]
    public class ArcherConfig : ScriptableObject
    {
        [Header("Health")]
        public float maxHP = 40f;
        public float armor = 0.3f;
        [Header("Hit Reaction")]
        public float smallHitThreshold  = 8f;
        public float mediumHitThreshold = 20f;
        public float largeHitThreshold  = 40f;
        public float knockbackMultiplier = 1.4f;
        [Header("Ranged Attack (placeholder — not implemented M2)")]
        public float attackRange    = 8f;
        public float attackInterval = 3f;
        public float arrowSpeed     = 6f;
        public float arrowDamage    = 15f;
        [Header("Rewards")]
        public int scoreOnDeath = 200;
        public int coinsOnDeath = 12;
        [Header("Audio / VFX")]
        public string hitSoundKey   = "enemy_hit";
        public string deathSoundKey = "enemy_death";
        public string deathVFXKey   = "enemy_death_vfx";
    }
}
