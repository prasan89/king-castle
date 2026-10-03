using UnityEngine;

namespace KingSmash.Physics
{
    public enum StructureMaterialType { Wood, Stone, Metal, Ice, Glass, Rubber }

    [CreateAssetMenu(fileName = "MaterialConfig", menuName = "KingSmash/Config/MaterialConfig")]
    public class MaterialConfig : ScriptableObject
    {
        [Header("Identity")]
        public StructureMaterialType materialType;
        public string displayName;

        [Header("Health")]
        public float maxHealth = 100f;
        [Tooltip("Minimum impact speed (m/s) that registers as damage")]
        public float minDamageSpeed = 1f;
        [Tooltip("Multiplier applied to raw kinetic-energy damage")]
        public float damageMultiplier = 1f;

        [Header("Physics")]
        [Range(0.1f, 20f)] public float density = 1f;
        [Range(0f, 1f)] public float bounciness = 0.3f;
        [Range(0f, 1f)] public float friction = 0.4f;

        [Header("Debris")]
        [Tooltip("How many debris particles to spawn on break (capped at pool max)")]
        [Range(0, 12)] public int debrisCount = 4;
        public float breakParticleScale = 1f;

        [Header("Audio")]
        public string impactSoundKey = "";
        public string breakSoundKey = "";

        [Header("Debug")]
        public Color debugColor = Color.white;
    }
}
