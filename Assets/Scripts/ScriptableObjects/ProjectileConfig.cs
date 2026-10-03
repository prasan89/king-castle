using UnityEngine;

namespace KingSmash.Physics
{
    [CreateAssetMenu(fileName = "ProjectileConfig", menuName = "KingSmash/Config/ProjectileConfig")]
    public class ProjectileConfig : ScriptableObject
    {
        [Header("Launch")]
        public float minLaunchPower = 4f;
        public float maxLaunchPower = 20f;
        public float launchAngleMin = 5f;
        public float launchAngleMax = 85f;

        [Header("Flight")]
        public float gravityScale = 1.5f;
        public float airDrag = 0.02f;
        public int maxBounces = 3;
        public float bounceEnergyLoss = 0.35f;

        [Header("Impact")]
        public float baseImpactRadius = 1.5f;
        public float impactForce = 800f;
        public float minSpeedForDamage = 2f;

        [Header("Trail")]
        public float trailTime = 0.4f;
        public bool showAimLine = true;
        public int aimLineDots = 20;
        public float aimLineDotSpacing = 0.2f;
    }
}
