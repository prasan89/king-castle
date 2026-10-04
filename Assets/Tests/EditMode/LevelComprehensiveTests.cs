// LevelComprehensiveTests.cs
// 100-level regression test suite for King Smash.
// EditMode only — no physics, no scene loading, no Random, no Time.

using NUnit.Framework;
using KingSmash.Levels;

namespace KingSmash.Tests.EditMode
{
    [TestFixture]
    public class LevelComprehensiveTests
    {
        // ── Parameterized source ─────────────────────────────────────────────────

        private static int[] AllLevelIds()
        {
            var ids = new int[100];
            for (int i = 0; i < 100; i++)
                ids[i] = i;
            return ids;
        }

        // ── Parameterized: structural validation ────────────────────────────────

        [TestCaseSource(nameof(AllLevelIds))]
        public void Level_HasValidLevelIndex(int levelIndex)
        {
            var all = LevelConfigFactory.All;
            Assert.IsTrue(levelIndex >= 0 && levelIndex < all.Count,
                $"levelIndex {levelIndex} is out of range [0, {all.Count})");
            var def = all[levelIndex];
            Assert.AreEqual(levelIndex, def.LevelIndex,
                $"Level at position {levelIndex} has LevelIndex={def.LevelIndex}");
        }

        [TestCaseSource(nameof(AllLevelIds))]
        public void Level_HasValidWorldAssignment(int levelIndex)
        {
            var def   = LevelConfigFactory.All[levelIndex];
            var world = WorldRegistry.GetWorldForLevel(levelIndex);
            Assert.IsNotNull(world,
                $"Level {levelIndex} returned null world from WorldRegistry.GetWorldForLevel");
            Assert.IsTrue(levelIndex >= world.LevelStart && levelIndex <= world.LevelEnd,
                $"Level {levelIndex} is outside world range [{world.LevelStart},{world.LevelEnd}]");
        }

        [TestCaseSource(nameof(AllLevelIds))]
        public void Level_HasValidScoreProgression(int levelIndex)
        {
            var def = LevelConfigFactory.All[levelIndex];
            Assert.Greater(def.RequiredScore, 0,
                $"Level {levelIndex}: RequiredScore must be > 0");
            Assert.Greater(def.TwoStarScore, def.RequiredScore,
                $"Level {levelIndex}: TwoStarScore ({def.TwoStarScore}) must exceed RequiredScore ({def.RequiredScore})");
            Assert.Greater(def.ThreeStarScore, def.TwoStarScore,
                $"Level {levelIndex}: ThreeStarScore ({def.ThreeStarScore}) must exceed TwoStarScore ({def.TwoStarScore})");
        }

        [TestCaseSource(nameof(AllLevelIds))]
        public void Level_HasValidDestructionThresholds(int levelIndex)
        {
            var def = LevelConfigFactory.All[levelIndex];
            Assert.Greater(def.OneStar_DestructionMin, 0f,
                $"Level {levelIndex}: OneStar_DestructionMin must be > 0");
            Assert.LessOrEqual(def.OneStar_DestructionMin, 1f,
                $"Level {levelIndex}: OneStar_DestructionMin must be <= 1");
            Assert.GreaterOrEqual(def.TwoStar_DestructionMin, def.OneStar_DestructionMin,
                $"Level {levelIndex}: TwoStar_DestructionMin must be >= OneStar_DestructionMin");
            Assert.GreaterOrEqual(def.ThreeStar_DestructionMin, def.TwoStar_DestructionMin,
                $"Level {levelIndex}: ThreeStar_DestructionMin must be >= TwoStar_DestructionMin");
        }

        [TestCaseSource(nameof(AllLevelIds))]
        public void Level_HasValidKingLaunches(int levelIndex)
        {
            var def = LevelConfigFactory.All[levelIndex];
            Assert.GreaterOrEqual(def.KingLaunches, 1,
                $"Level {levelIndex}: KingLaunches must be >= 1");
            Assert.LessOrEqual(def.KingLaunches, 10,
                $"Level {levelIndex}: KingLaunches must be <= 10");
        }

        [TestCaseSource(nameof(AllLevelIds))]
        public void Level_HasPrimaryMaterial(int levelIndex)
        {
            var def = LevelConfigFactory.All[levelIndex];
            Assert.IsNotNull(def.PrimaryMaterial,
                $"Level {levelIndex}: PrimaryMaterial is null");
            Assert.IsNotEmpty(def.PrimaryMaterial,
                $"Level {levelIndex}: PrimaryMaterial is empty");

            bool valid = def.PrimaryMaterial == "Wood"
                      || def.PrimaryMaterial == "Stone"
                      || def.PrimaryMaterial == "Metal"
                      || def.PrimaryMaterial == "Sandstone"
                      || def.PrimaryMaterial == "Ice";
            Assert.IsTrue(valid,
                $"Level {levelIndex}: PrimaryMaterial '{def.PrimaryMaterial}' is not one of Wood/Stone/Metal/Sandstone/Ice");
        }

        // ── Critical level tests ────────────────────────────────────────────────

        [Test]
        public void Level_0_IsFirstSmash()
        {
            var def = LevelConfigFactory.All[0];
            Assert.AreEqual(0,  def.LevelIndex,  "Level 0: LevelIndex should be 0");
            Assert.AreEqual(0,  def.WorldIndex,  "Level 0: WorldIndex should be 0");
            Assert.IsTrue(def.HasTutorialHints,  "Level 0: HasTutorialHints should be true");
            Assert.IsTrue(def.HasQueen,          "Level 0: HasQueen should be true");
        }

        [Test]
        public void Level_19_IsForestBoss()
        {
            var def = LevelConfigFactory.All[19];
            Assert.AreEqual(19, def.LevelIndex, "Level 19: LevelIndex should be 19");
            Assert.IsTrue(def.HasEnemyKing,     "Level 19 (Forest boss): HasEnemyKing should be true");
        }

        [Test]
        public void Level_20_StartsDesertWorld()
        {
            var def = LevelConfigFactory.All[20];
            Assert.AreEqual(20, def.LevelIndex,  "Level 20: LevelIndex should be 20");
            Assert.AreEqual(1,  def.WorldIndex,  "Level 20: WorldIndex should be 1 (Desert)");
        }

        [Test]
        public void Level_39_IsDesertBoss()
        {
            var def = LevelConfigFactory.All[39];
            Assert.AreEqual(39, def.LevelIndex, "Level 39: LevelIndex should be 39");
            Assert.IsTrue(def.HasEnemyKing,     "Level 39 (Desert boss): HasEnemyKing should be true");
        }

        [Test]
        public void Level_40_StartsIceWorld()
        {
            var def = LevelConfigFactory.All[40];
            Assert.AreEqual(40, def.LevelIndex, "Level 40: LevelIndex should be 40");
            Assert.AreEqual(2,  def.WorldIndex, "Level 40: WorldIndex should be 2 (Ice)");
        }

        [Test]
        public void Level_99_IsLastLevel()
        {
            var def = LevelConfigFactory.All[99];
            Assert.AreEqual(99, def.LevelIndex, "Level 99: LevelIndex should be 99");
            Assert.AreEqual(4,  def.WorldIndex, "Level 99: WorldIndex should be 4 (Final)");
        }

        // ── World regression tests ───────────────────────────────────────────────

        [Test]
        public void World_HasExactly5Worlds()
        {
            Assert.AreEqual(5, WorldRegistry.All.Count, "There should be exactly 5 worlds");
        }

        [Test]
        public void World_Each20LevelsNoBoss()
        {
            foreach (var world in WorldRegistry.All)
            {
                int count = world.LevelEnd - world.LevelStart + 1;
                Assert.AreEqual(20, count,
                    $"World {world.WorldIndex} should cover exactly 20 levels, got {count} ({world.LevelStart}-{world.LevelEnd})");
            }
        }

        [Test]
        public void World_NoBossLevelOutOfRange()
        {
            foreach (var world in WorldRegistry.All)
            {
                Assert.GreaterOrEqual(world.BossLevelIndex, world.LevelStart,
                    $"World {world.WorldIndex}: BossLevelIndex {world.BossLevelIndex} < LevelStart {world.LevelStart}");
                Assert.LessOrEqual(world.BossLevelIndex, world.LevelEnd,
                    $"World {world.WorldIndex}: BossLevelIndex {world.BossLevelIndex} > LevelEnd {world.LevelEnd}");
            }
        }

        [Test]
        public void World_UnlockStarsProgression()
        {
            var all = WorldRegistry.All;
            int prevStars = -1;
            foreach (var world in all)
            {
                Assert.GreaterOrEqual(world.RequiredStarsToUnlock, prevStars,
                    $"World {world.WorldIndex} RequiredStarsToUnlock ({world.RequiredStarsToUnlock}) is less than previous world ({prevStars})");
                prevStars = world.RequiredStarsToUnlock;
            }
        }

        [Test]
        public void AllLevels_TotalCount100()
        {
            Assert.AreEqual(100, LevelConfigFactory.All.Count, "Total level count should be exactly 100");
        }
    }
}
