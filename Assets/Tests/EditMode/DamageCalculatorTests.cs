using NUnit.Framework;
using UnityEngine;
using KingSmash.Physics;

namespace KingSmash.Tests.EditMode
{
    public class DamageCalculatorTests
    {
        private MaterialConfig MakeMaterial(StructureMaterialType type, float maxHp, float minSpeed,
            float multiplier, float density = 1f, float bounciness = 0.3f)
        {
            var m = ScriptableObject.CreateInstance<MaterialConfig>();
            m.materialType       = type;
            m.maxHealth          = maxHp;
            m.minDamageSpeed     = minSpeed;
            m.damageMultiplier   = multiplier;
            m.density            = density;
            m.bounciness         = bounciness;
            return m;
        }

        [Test]
        public void BelowMinSpeed_ReturnsZero()
        {
            var mat = MakeMaterial(StructureMaterialType.Stone, 200f, 2.5f, 0.7f);
            float damage = DamageCalculator.Calculate(impactSpeed: 1f, material: mat, attackerMass: 1f);
            Assert.AreEqual(0f, damage);
            Object.DestroyImmediate(mat);
        }

        [Test]
        public void AtMinSpeed_ReturnsPositive()
        {
            var mat = MakeMaterial(StructureMaterialType.Wood, 80f, 1.5f, 1.2f);
            float damage = DamageCalculator.Calculate(impactSpeed: 1.5f, material: mat, attackerMass: 1f);
            Assert.Greater(damage, 0f);
            Object.DestroyImmediate(mat);
        }

        [Test]
        public void HigherSpeed_MoreDamage()
        {
            var mat = MakeMaterial(StructureMaterialType.Wood, 80f, 1.0f, 1.0f);
            float low  = DamageCalculator.Calculate(3f,  mat, 1f);
            float high = DamageCalculator.Calculate(10f, mat, 1f);
            Assert.Greater(high, low);
            Object.DestroyImmediate(mat);
        }

        [Test]
        public void MetalMultiplier_LessDamageThanWood()
        {
            var wood  = MakeMaterial(StructureMaterialType.Wood,  80f,  1.0f, 1.2f);
            var metal = MakeMaterial(StructureMaterialType.Metal, 400f, 1.0f, 0.4f);
            float woodDmg  = DamageCalculator.Calculate(8f, wood,  1f);
            float metalDmg = DamageCalculator.Calculate(8f, metal, 1f);
            Assert.Greater(woodDmg, metalDmg, "Wood should take more damage than Metal at same speed.");
            Object.DestroyImmediate(wood);
            Object.DestroyImmediate(metal);
        }

        [Test]
        public void HeavierAttacker_MoreDamage()
        {
            var mat   = MakeMaterial(StructureMaterialType.Wood, 80f, 1f, 1f);
            float d1  = DamageCalculator.Calculate(5f, mat, attackerMass: 1f);
            float d2  = DamageCalculator.Calculate(5f, mat, attackerMass: 3f);
            Assert.Greater(d2, d1);
            Object.DestroyImmediate(mat);
        }

        [Test]
        public void NullMaterial_FallsBackToDefault()
        {
            float damage = DamageCalculator.Calculate(5f, null, 1f);
            Assert.AreEqual(5f, damage, 0.001f);
        }

        [Test]
        public void DestructionToStars_CorrectThresholds()
        {
            Assert.AreEqual(0, DamageCalculator.DestructionToStars(0f));
            Assert.AreEqual(1, DamageCalculator.DestructionToStars(0.2f));
            Assert.AreEqual(2, DamageCalculator.DestructionToStars(0.7f));
            Assert.AreEqual(3, DamageCalculator.DestructionToStars(0.95f));
        }

        [Test]
        public void ImpulseForce_ScalesWithMassAndRestitution()
        {
            var vel    = new Vector2(10f, 0f);
            var low    = DamageCalculator.ImpulseForce(vel, 1f, 0f);
            var high   = DamageCalculator.ImpulseForce(vel, 1f, 1f);
            Assert.Greater(high.magnitude, low.magnitude);
        }
    }
}
