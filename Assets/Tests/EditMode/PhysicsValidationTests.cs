// PhysicsValidationTests.cs
// Physics configuration and boundary validation tests (EditMode — no runtime physics).
// Validates config data, material coverage, boss integrity, and star calculation.

using NUnit.Framework;
using KingSmash.Levels;

namespace KingSmash.Tests.EditMode
{
    [TestFixture]
    public class PhysicsValidationTests
    {
        // ── Material coverage ────────────────────────────────────────────────────

        [Test]
        public void MaterialConfigs_AllMaterialsHavePositiveDensity()
        {
            // Validates that all 5 materials are actually referenced across the 100 levels.
            // A material that appears nowhere is effectively unconfigured / dead code.
            var all  = LevelConfigFactory.All;
            bool hasWood      = false;
            bool hasStone     = false;
            bool hasMetal     = false;
            bool hasSandstone = false;
            bool hasIce       = false;

            foreach (var def in all)
            {
                if (def.PrimaryMaterial == "Wood"      || def.SecondaryMaterial == "Wood")      hasWood      = true;
                if (def.PrimaryMaterial == "Stone"     || def.SecondaryMaterial == "Stone")     hasStone     = true;
                if (def.PrimaryMaterial == "Metal"     || def.SecondaryMaterial == "Metal")     hasMetal     = true;
                if (def.PrimaryMaterial == "Sandstone" || def.SecondaryMaterial == "Sandstone") hasSandstone = true;
                if (def.PrimaryMaterial == "Ice"       || def.SecondaryMaterial == "Ice")       hasIce       = true;
            }

            Assert.IsTrue(hasWood,      "No level references material 'Wood'");
            Assert.IsTrue(hasStone,     "No level references material 'Stone'");
            Assert.IsTrue(hasMetal,     "No level references material 'Metal'");
            Assert.IsTrue(hasSandstone, "No level references material 'Sandstone'");
            Assert.IsTrue(hasIce,       "No level references material 'Ice'");
        }

        // ── KingLaunches bounds ──────────────────────────────────────────────────

        [Test]
        public void Physics_AllLevelsHaveBoundedKingLaunches()
        {
            var all = LevelConfigFactory.All;
            foreach (var def in all)
            {
                Assert.LessOrEqual(def.KingLaunches, 10,
                    $"Level {def.LevelIndex} has KingLaunches={def.KingLaunches} (max 10)");
                Assert.GreaterOrEqual(def.KingLaunches, 1,
                    $"Level {def.LevelIndex} has KingLaunches={def.KingLaunches} (min 1)");
            }
        }

        // ── Boss level integrity ──────────────────────────────────────────────────

        [Test]
        public void Physics_BossLevelsHaveEnemyKing()
        {
            var levels = LevelConfigFactory.All;
            foreach (var world in WorldRegistry.All)
            {
                var boss = levels[world.BossLevelIndex];
                Assert.IsTrue(boss.HasEnemyKing,
                    $"World {world.WorldIndex} boss (level {world.BossLevelIndex}) must have HasEnemyKing=true");
            }
        }

        [Test]
        public void Physics_NoBossLevelIsFirstLevel()
        {
            foreach (var world in WorldRegistry.All)
            {
                Assert.AreNotEqual(world.LevelStart, world.BossLevelIndex,
                    $"World {world.WorldIndex}: BossLevelIndex ({world.BossLevelIndex}) must not equal LevelStart ({world.LevelStart})");
            }
        }

        // ── Global difficulty progression ─────────────────────────────────────────

        [Test]
        public void Physics_DifficultyProgression()
        {
            var levels = LevelConfigFactory.All;
            int diffLevel0  = levels[0].DifficultyRating;
            int diffLevel99 = levels[99].DifficultyRating;
            Assert.Greater(diffLevel99, diffLevel0,
                $"Level 99 DifficultyRating ({diffLevel99}) should be greater than level 0 ({diffLevel0})");
        }

        // ── Destruction objectives ───────────────────────────────────────────────

        [Test]
        public void Destruction_AllLevelsHaveDestructionObjective()
        {
            var all = LevelConfigFactory.All;
            foreach (var def in all)
            {
                Assert.Greater(def.OneStar_DestructionMin, 0f,
                    $"Level {def.LevelIndex}: OneStar_DestructionMin must be > 0 (all levels require some destruction)");
            }
        }

        [Test]
        public void Destruction_ThreeStarRequiresHighDestruction()
        {
            const float minimumThreeStarDestruction = 0.70f;
            var all = LevelConfigFactory.All;
            foreach (var def in all)
            {
                if (def.HasEnemyKing) continue;
                Assert.GreaterOrEqual(def.ThreeStar_DestructionMin, minimumThreeStarDestruction,
                    $"Level {def.LevelIndex}: ThreeStar_DestructionMin ({def.ThreeStar_DestructionMin}) is < {minimumThreeStarDestruction} for a non-boss level");
            }
        }

        // ── Star calculator edge cases ────────────────────────────────────────────

        [Test]
        public void StarCalc_ZeroDestructionGivesZeroStars()
        {
            var thresholds = new StarThresholds
            {
                oneStar_destructionMin       = 0.30f,
                oneStar_mustRescueQueen      = true,
                twoStar_destructionMin       = 0.60f,
                twoStar_mustRescueQueen      = true,
                twoStar_enemyRatioMin        = 0.50f,
                threeStar_destructionMin     = 0.90f,
                threeStar_mustRescueQueen    = true,
                threeStar_enemyRatioMin      = 1.00f,
                threeStar_attemptsRemaining  = 1,
            };
            int stars = StarCalculator.Calculate(thresholds,
                destructionRatio:  0f,
                enemyRatio:        0f,
                queenRescued:      false,
                attemptsRemaining: 0);
            Assert.AreEqual(0, stars, "Zero destruction / zero enemy ratio should yield 0 stars");
        }

        [Test]
        public void StarCalc_MaxScoreGivesThreeStars()
        {
            // Use level 0 thresholds explicitly.
            var def = LevelConfigFactory.All[0];
            var thresholds = new StarThresholds
            {
                oneStar_destructionMin       = def.OneStar_DestructionMin,
                oneStar_mustRescueQueen      = def.MustRescueQueen,
                twoStar_destructionMin       = def.TwoStar_DestructionMin,
                twoStar_mustRescueQueen      = def.MustRescueQueen,
                twoStar_enemyRatioMin        = def.TwoStar_EnemyRatioMin,
                threeStar_destructionMin     = def.ThreeStar_DestructionMin,
                threeStar_mustRescueQueen    = def.MustRescueQueen,
                threeStar_enemyRatioMin      = def.ThreeStar_EnemyRatioMin,
                threeStar_attemptsRemaining  = def.ThreeStar_AttemptsRemaining,
            };
            int stars = StarCalculator.Calculate(thresholds,
                destructionRatio:  1.0f,
                enemyRatio:        1.0f,
                queenRescued:      true,
                attemptsRemaining: 2);
            Assert.AreEqual(3, stars, "Max performance on level 0 should yield 3 stars");
        }

        // ── Boundary / scene name tests ───────────────────────────────────────────

        [Test]
        public void Boundaries_AllLevelsShareSameSceneName()
        {
            const string expectedScene = "Level";
            var all = LevelConfigFactory.All;
            foreach (var def in all)
            {
                Assert.AreEqual(expectedScene, def.SceneName,
                    $"Level {def.LevelIndex}: SceneName should be '{expectedScene}', got '{def.SceneName}'");
            }
        }

        [Test]
        public void Boundaries_RequiredStarsIncreases()
        {
            // Within each world, RequiredStarsToUnlock should be non-decreasing across consecutive levels.
            var levels = LevelConfigFactory.All;
            foreach (var world in WorldRegistry.All)
            {
                int prev = -1;
                for (int i = world.LevelStart; i <= world.LevelEnd; i++)
                {
                    var def = levels[i];
                    Assert.GreaterOrEqual(def.RequiredStarsToUnlock, prev,
                        $"World {world.WorldIndex}, level {i}: RequiredStarsToUnlock ({def.RequiredStarsToUnlock}) is less than level {i - 1} ({prev})");
                    prev = def.RequiredStarsToUnlock;
                }
            }
        }
    }
}
