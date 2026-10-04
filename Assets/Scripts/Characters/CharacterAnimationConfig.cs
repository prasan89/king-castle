using UnityEngine;

namespace KingSmash.Characters
{
    [CreateAssetMenu(
        fileName = "CharacterAnimationConfig",
        menuName  = "KingSmash/Characters/CharacterAnimationConfig")]
    public class CharacterAnimationConfig : ScriptableObject
    {
        [Header("King — Squash / Stretch")]
        public float squashOnLaunch = 0.7f;
        public float stretchOnFlight = 1.3f;
        public float squashOnImpact = 0.5f;
        public float anticipationDuration = 0.15f;

        [Header("Enemy")]
        public float hitRecoilDuration = 0.12f;
        public float deathFallDuration = 0.4f;

        [Header("Queen")]
        public float rescueCelebrationDuration = 1.2f;
    }
}
