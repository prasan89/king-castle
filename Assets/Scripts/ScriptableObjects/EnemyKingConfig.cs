using UnityEngine;
namespace KingSmash.Characters
{
    [CreateAssetMenu(fileName = "EnemyKingConfig", menuName = "KingSmash/Config/Enemies/EnemyKingConfig")]
    public class EnemyKingConfig : ScriptableObject
    {
        [Header("Boss Health")]
        public float maxHP = 400f;
        public float armor = 2f;
        public float impactResistance = 0.5f;   // multiply incoming impact by (1 - impactResistance)
        [Header("Hit Reaction")]
        public float smallHitThreshold  = 30f;
        public float mediumHitThreshold = 100f;
        public float largeHitThreshold  = 200f;
        public float knockbackMultiplier = 0.3f;
        public float mass = 4f;
        [Header("Rewards")]
        public int scoreOnDeath = 1000;
        public int coinsOnDeath = 50;
        [Header("Audio / VFX")]
        public string hitSoundKey    = "boss_hit";
        public string deathSoundKey  = "boss_death";
        public string deathVFXKey    = "boss_death_vfx";
        public string tauntSoundKey  = "boss_taunt";
    }
}
