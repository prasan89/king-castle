using NUnit.Framework;
using KingSmash.Levels;
namespace KingSmash.Tests.EditMode
{
    public class LevelValidationSystemTests
    {
        [Test]
        public void LevelValidator_AllLevels_Pass()
        {
            var reports = LevelValidator.ValidateAll();
            foreach (var r in reports)
                Assert.IsTrue(r.IsValid, r.ToString());
        }

        [Test]
        public void WorldValidator_AllWorlds_Pass()
        {
            // WorldValidator.ValidateAll() returns bool and logs; suppress expected passing
            bool result = WorldValidator.ValidateAll();
            Assert.IsTrue(result, "World validation failed — check Console for details.");
        }

        [Test]
        public void LevelValidator_InvalidDef_Catches_BadScore()
        {
            var bad = new LevelDefinition
            {
                LevelIndex = 0, WorldIndex = 0, DisplayName = "Test",
                KingLaunches = 3, DifficultyRating = 1, HasQueen = true,
                RequiredScore = 100, TwoStarScore = 50, ThreeStarScore = 200, // TwoStar < Required
                OneStar_DestructionMin = 0.3f, TwoStar_DestructionMin = 0.6f,
                ThreeStar_DestructionMin = 0.9f
            };
            var report = LevelValidator.Validate(bad);
            Assert.IsFalse(report.IsValid);
            Assert.Greater(report.Errors.Count, 0);
        }

        [Test]
        public void LevelValidator_InvalidDef_Catches_NegativeIndex()
        {
            var bad = new LevelDefinition { LevelIndex = -1, WorldIndex = 0,
                HasQueen = true, DisplayName = "X", KingLaunches = 3, DifficultyRating = 1,
                RequiredScore = 100, TwoStarScore = 200, ThreeStarScore = 300,
                OneStar_DestructionMin = 0.3f, TwoStar_DestructionMin = 0.6f, ThreeStar_DestructionMin = 0.9f };
            var report = LevelValidator.Validate(bad);
            Assert.IsFalse(report.IsValid);
        }

        [Test]
        public void LevelValidator_ValidDef_Passes()
        {
            var good = new LevelDefinition
            {
                LevelIndex = 5, WorldIndex = 0, DisplayName = "Good Level",
                KingLaunches = 4, DifficultyRating = 3, HasQueen = true,
                RequiredScore = 400, TwoStarScore = 700, ThreeStarScore = 1000,
                OneStar_DestructionMin = 0.30f, TwoStar_DestructionMin = 0.60f,
                ThreeStar_DestructionMin = 0.90f, IsBossLevel = false,
            };
            var report = LevelValidator.Validate(good);
            Assert.IsTrue(report.IsValid, report.ToString());
        }

        [Test]
        public void LevelValidator_Reports100Entries()
        {
            var reports = LevelValidator.ValidateAll();
            // Should have exactly 100 (one per level) + possibly 1 synthetic count check
            Assert.GreaterOrEqual(reports.Count, 100);
        }

        [Test]
        public void WorldProgression_UnlockStarRequirements_Ascending()
        {
            for (int i = 1; i < WorldRegistry.All.Count; i++)
                Assert.GreaterOrEqual(
                    WorldRegistry.All[i].RequiredStarsToUnlock,
                    WorldRegistry.All[i-1].RequiredStarsToUnlock,
                    $"World {i} unlock stars should be >= World {i-1}");
        }

        [Test]
        public void WorldProgression_World0_RequiresZeroStars()
            => Assert.AreEqual(0, WorldRegistry.GetWorld(0).RequiredStarsToUnlock);

        [Test]
        public void WorldProgression_FinalWorld_RequiresMostStars()
        {
            int finalReq = WorldRegistry.GetWorld(4).RequiredStarsToUnlock;
            for (int i = 0; i < 4; i++)
                Assert.LessOrEqual(WorldRegistry.GetWorld(i).RequiredStarsToUnlock, finalReq);
        }
    }
}
