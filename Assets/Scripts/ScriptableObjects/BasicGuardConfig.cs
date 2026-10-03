using UnityEngine;
namespace KingSmash.Characters
{
    [CreateAssetMenu(fileName = "BasicGuardConfig", menuName = "KingSmash/Config/Enemies/BasicGuardConfig")]
    public class BasicGuardConfig : ScriptableObject
    {
        [Header("Health")]
        public float maxHP = 60f;
        public float armor = 0.5f;
        [Header("Hit Reaction")]
        public float smallHitThreshold  = 10f;
        public float mediumHitThreshold = 30f;
        public float largeHitThreshold  = 60f;
        public float knockbackMultiplier = 1f;
        [Header("Rewards")]
        public int scoreOnDeath = 150;
        public int coinsOnDeath = 8;
        [Header("Audio / VFX")]
        public string hitSoundKey   = "enemy_hit";
        public string deathSoundKey = "enemy_death";
        public string deathVFXKey   = "enemy_death_vfx";
    }
}
