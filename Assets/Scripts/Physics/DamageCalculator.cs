using UnityEngine;

namespace KingSmash.Physics
{
    /// Pure-static damage math — no MonoBehaviour, fully testable.
    public static class DamageCalculator
    {
        /// Calculate damage delivered from a collision given impact velocity and material.
        /// impactSpeed: magnitude of relative velocity at contact point (m/s Unity units)
        /// material: the receiving block's MaterialConfig (may be null — uses defaults)
        /// attackerMass: the projectile's Rigidbody2D.mass
        /// Returns raw damage value before HP subtraction.
        public static float Calculate(float impactSpeed, MaterialConfig material, float attackerMass = 1f)
        {
            if (material == null)
                return impactSpeed * attackerMass;

            // Below minDamageSpeed the hit is too gentle to damage this material
            if (impactSpeed < material.minDamageSpeed)
                return 0f;

            // Kinetic energy proxy: 0.5 * m * v^2, scaled to game units
            float rawDamage = 0.5f * attackerMass * impactSpeed * impactSpeed;
            return rawDamage * material.damageMultiplier;
        }

        /// Impulse force vector to apply at the impact point on a Rigidbody2D.
        public static Vector2 ImpulseForce(Vector2 contactVelocity, float attackerMass, float materialBounciness)
        {
            // Impulse = m * deltaV.  deltaV ≈ (1 + e) * v_approach for a rigid body.
            float restitution = Mathf.Clamp01(materialBounciness);
            return contactVelocity * attackerMass * (1f + restitution);
        }

        /// Convert a 0-1 destruction ratio to a star count using configurable thresholds.
        public static int DestructionToStars(float destructionRatio, float twoStarThreshold = 0.5f, float threeStarThreshold = 0.9f)
        {
            if (destructionRatio >= threeStarThreshold) return 3;
            if (destructionRatio >= twoStarThreshold) return 2;
            if (destructionRatio > 0f) return 1;
            return 0;
        }
    }
}
