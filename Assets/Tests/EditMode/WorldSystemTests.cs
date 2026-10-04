using NUnit.Framework;
using System.Collections.Generic;
using KingSmash.Levels;
namespace KingSmash.Tests.EditMode
{
    public class WorldSystemTests
    {
        // ─── WorldRegistry ──────────────────────────────────────────────
        [Test] public void WorldRegistry_Has5Worlds()
            => Assert.AreEqual(5, WorldRegistry.All.Count);

        [Test] public void WorldRegistry_WorldIndicesAre0To4()
        {
            for (int i = 0; i < 5; i++)
                Assert.IsNotNull(WorldRegistry.GetWorld(i), $"World {i} missing");
        }

        [Test] public void WorldRegistry_EachWorldHas20Levels()
        {
            foreach (var w in WorldRegistry.All)
                Assert.AreEqual(20, w.LevelsPerWorld, $"World {w.WorldIndex} ({w.DisplayName}) has {w.LevelsPerWorld} levels");
        }

        [Test] public void WorldRegistry_LevelRangesCoverAll100()
        {
            var covered = new bool[100];
            foreach (var w in WorldRegistry.All)
                for (int i = w.LevelStart; i <= w.LevelEnd; i++)
                    covered[i] = true;
            for (int i = 0; i < 100; i++)
                Assert.IsTrue(covered[i], $"Level {i+1} not covered by any world");
        }

        [Test] public void WorldRegistry_BossIsAtLevelEnd()
        {
            foreach (var w in WorldRegistry.All)
                Assert.AreEqual(w.LevelEnd, w.BossLevelIndex,
                    $"World {w.WorldIndex}: BossLevelIndex={w.BossLevelIndex} != LevelEnd={w.LevelEnd}");
        }

        [Test] public void WorldRegistry_Exactly5BossLevels()
        {
            int count = 0;
            for (int i = 0; i < 100; i++)
                if (WorldRegistry.IsBossLevel(i)) count++;
            Assert.AreEqual(5, count);
        }

        [Test] public void WorldRegistry_BossLevelsAre19_39_59_79_99()
        {
            Assert.IsTrue(WorldRegistry.IsBossLevel(19), "L20 should be boss");
            Assert.IsTrue(WorldRegistry.IsBossLevel(39), "L40 should be boss");
            Assert.IsTrue(WorldRegistry.IsBossLevel(59), "L60 should be boss");
            Assert.IsTrue(WorldRegistry.IsBossLevel(79), "L80 should be boss");
            Assert.IsTrue(WorldRegistry.IsBossLevel(99), "L100 should be boss");
        }

        [Test] public void WorldRegistry_NonBossLevels_NotBoss()
        {
            Assert.IsFalse(WorldRegistry.IsBossLevel(0),  "L1 should not be boss");
            Assert.IsFalse(WorldRegistry.IsBossLevel(9),  "L10 should not be boss");
            Assert.IsFalse(WorldRegistry.IsBossLevel(50), "L51 should not be boss");
        }

        [Test] public void WorldRegistry_GetWorldForLevel_Correct()
        {
            Assert.AreEqual(0, WorldRegistry.GetWorldForLevel(0)?.WorldIndex);
            Assert.AreEqual(0, WorldRegistry.GetWorldForLevel(19)?.WorldIndex);
            Assert.AreEqual(1, WorldRegistry.GetWorldForLevel(20)?.WorldIndex);
            Assert.AreEqual(1, WorldRegistry.GetWorldForLevel(39)?.WorldIndex);
            Assert.AreEqual(4, WorldRegistry.GetWorldForLevel(99)?.WorldIndex);
        }

        [Test] public void WorldRegistry_GetWorldForLevel_OutOfRange_ReturnsNull()
            => Assert.IsNull(WorldRegistry.GetWorldForLevel(100));

        // ─── WorldDefinition.GetStatus ───────────────────────────────────
        [Test] public void WorldDef_Status_Locked_WhenNotEnoughStars()
        {
            var w = WorldRegistry.GetWorld(1); // needs 30 stars
            var status = w.GetStatus(0, 0);
            Assert.AreEqual(WorldStatus.Locked, status);
        }

        [Test] public void WorldDef_Status_World0_AlwaysAvailableOrBetter()
        {
            var w = WorldRegistry.GetWorld(0);
            var status = w.GetStatus(0, 0);
            Assert.AreNotEqual(WorldStatus.Locked, status);
        }

        [Test] public void WorldDef_Status_Completed_WhenPastEnd()
        {
            var w = WorldRegistry.GetWorld(0);
            var status = w.GetStatus(20, 60); // highest unlocked past end
            Assert.AreEqual(WorldStatus.Completed, status);
        }

        [Test] public void WorldDef_Status_InProgress_WhenInsideRange()
        {
            var w = WorldRegistry.GetWorld(0);
            var status = w.GetStatus(5, 60); // in range
            Assert.AreEqual(WorldStatus.InProgress, status);
        }

        // ─── LevelConfigFactory 100-level checks ─────────────────────────
        [Test] public void LevelFactory_Has100Levels()
            => Assert.AreEqual(100, LevelConfigFactory.All.Count);

        [Test] public void LevelFactory_AllIDsUnique()
        {
            var seen = new HashSet<int>();
            foreach (var d in LevelConfigFactory.All)
                Assert.IsTrue(seen.Add(d.LevelIndex), $"Duplicate LevelIndex {d.LevelIndex}");
        }

        [Test] public void LevelFactory_AllIDsInRange0To99()
        {
            foreach (var d in LevelConfigFactory.All)
                Assert.IsTrue(d.LevelIndex >= 0 && d.LevelIndex < 100,
                    $"LevelIndex {d.LevelIndex} out of range");
        }

        [Test] public void LevelFactory_AllLevelsHaveQueen()
        {
            foreach (var d in LevelConfigFactory.All)
                Assert.IsTrue(d.HasQueen, $"Level {d.LevelIndex+1} missing queen");
        }

        [Test] public void LevelFactory_Exactly5BossLevels()
        {
            int count = 0;
            foreach (var d in LevelConfigFactory.All) if (d.IsBossLevel) count++;
            Assert.AreEqual(5, count, "Expected exactly 5 boss levels");
        }

        [Test] public void LevelFactory_BossLevelsMatchWorldEnds()
        {
            var bossIndices = new HashSet<int>();
            foreach (var d in LevelConfigFactory.All) if (d.IsBossLevel) bossIndices.Add(d.LevelIndex);
            foreach (var w in WorldRegistry.All)
                Assert.IsTrue(bossIndices.Contains(w.BossLevelIndex),
                    $"World {w.WorldIndex} boss level {w.BossLevelIndex} not marked IsBossLevel");
        }

        [Test] public void LevelFactory_Level1_IsTutorial()
        {
            var l = LevelConfigFactory.All[0];
            Assert.IsTrue(l.HasTutorialHints);
            Assert.AreEqual(0, l.WorldIndex);
        }

        [Test] public void LevelFactory_Level100_IsBoss_HasAllEnemyTypes()
        {
            var l = LevelConfigFactory.All[99];
            Assert.IsTrue(l.IsBossLevel, "L100 should be boss");
            Assert.IsTrue(l.HasEnemyKing, "L100 should have Enemy King");
            Assert.Greater(l.GuardCount, 0);
            Assert.Greater(l.ArcherCount, 0);
            Assert.Greater(l.ShieldGuardCount, 0);
            Assert.IsTrue(l.HasExplosiveBarrel);
        }

        [Test] public void LevelFactory_DifficultyRatingsInRange()
        {
            foreach (var d in LevelConfigFactory.All)
                Assert.IsTrue(d.DifficultyRating >= 1 && d.DifficultyRating <= 10,
                    $"Level {d.LevelIndex+1} difficulty {d.DifficultyRating} out of range");
        }

        [Test] public void LevelFactory_ScoreHierarchyValid()
        {
            foreach (var d in LevelConfigFactory.All)
            {
                Assert.Greater(d.RequiredScore, 0, $"L{d.LevelIndex+1} RequiredScore");
                Assert.Greater(d.TwoStarScore, d.RequiredScore, $"L{d.LevelIndex+1} TwoStar");
                Assert.Greater(d.ThreeStarScore, d.TwoStarScore, $"L{d.LevelIndex+1} ThreeStar");
            }
        }
    }
}
