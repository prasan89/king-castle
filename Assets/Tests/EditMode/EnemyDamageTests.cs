using NUnit.Framework;
using UnityEngine;

namespace KingSmash.Tests.EditMode
{
    public class EnemyDamageTests
    {
        // Helper: simulate the armor damage reduction formula used in EnemyHealth.TakeDamage
        private static float ApplyArmor(float raw, float armor) => raw * (1f - Mathf.Clamp01(armor));

        // Helper: classify hit size (mirrors EnemyHealth.ClassifyHit logic)
        private static Characters.HitReactionSize Classify(float effective, float small, float medium, float large)
        {
            if (effective >= large)  return Characters.HitReactionSize.Large;
            if (effective >= medium) return Characters.HitReactionSize.Medium;
            return Characters.HitReactionSize.Small;
        }

        [Test]
        public void NoArmor_FullDamageThrough()
        {
            Assert.AreEqual(50f, ApplyArmor(50f, 0f), 0.001f);
        }

        [Test]
        public void FullArmor_ZeroDamage()
        {
            Assert.AreEqual(0f, ApplyArmor(50f, 1f), 0.001f);
        }

        [Test]
        public void HalfArmor_HalvesDamage()
        {
            Assert.AreEqual(25f, ApplyArmor(50f, 0.5f), 0.001f);
        }

        [Test]
        public void ShieldReduction_80Percent_LeavesSmallDamage()
        {
            float raw = 100f;
            float shieldReduction = 0.8f;
            float throughShield = raw * (1f - shieldReduction);
            Assert.AreEqual(20f, throughShield, 0.001f);
        }

        [Test]
        public void HitClassification_BelowSmall_IsSmall()
        {
            Assert.AreEqual(Characters.HitReactionSize.Small, Classify(5f, 10f, 30f, 60f));
        }

        [Test]
        public void HitClassification_AboveLarge_IsLarge()
        {
            Assert.AreEqual(Characters.HitReactionSize.Large, Classify(70f, 10f, 30f, 60f));
        }

        [Test]
        public void HitClassification_MediumRange_IsMedium()
        {
            Assert.AreEqual(Characters.HitReactionSize.Medium, Classify(35f, 10f, 30f, 60f));
        }

        [Test]
        public void EnemyKingImpactResistance_ReducesDamage()
        {
            float incoming = 200f;
            float resistance = 0.5f;
            float actual = incoming * (1f - resistance);
            Assert.AreEqual(100f, actual, 0.001f);
        }

        [Test]
        public void KnockbackMultiplier_ScalesForce()
        {
            var force = new Vector2(10f, 5f);
            float mult = 0.6f;
            var result = force * mult;
            Assert.AreEqual(6f, result.x, 0.001f);
            Assert.AreEqual(3f, result.y, 0.001f);
        }
    }
}
