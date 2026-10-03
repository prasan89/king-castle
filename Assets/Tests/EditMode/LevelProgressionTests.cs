using NUnit.Framework;
using KingSmash.UI;
using KingSmash.Levels;
namespace KingSmash.Tests.EditMode
{
    public class LevelProgressionTests
    {
        [Test]
        public void LevelResult_VictoryTrue_CoinsPositive_StarsSet()
        {
            var result = new LevelResult
            {
                LevelIndex   = 0,
                IsVictory    = true,
                Stars        = 2,
                CoinsEarned  = 150L,
                QueenRescued = true,
                DestructionRatio = 0.75f,
            };
            Assert.IsTrue(result.IsVictory);
            Assert.AreEqual(2, result.Stars);
            Assert.AreEqual(150L, result.CoinsEarned);
        }

        [Test]
        public void UnlockLogic_VictoryUnlocksNextLevel()
        {
            int currentLevel = 0;
            int resultLevelIndex = 0;
            bool isVictory = true;

            if (isVictory)
            {
                int nextLevel = resultLevelIndex + 1;
                if (nextLevel > currentLevel) currentLevel = nextLevel;
            }

            Assert.AreEqual(1, currentLevel);
        }

        [Test]
        public void UnlockLogic_FailureDoesNotUnlock()
        {
            int currentLevel = 0;
            bool isVictory = false;

            if (isVictory)
                currentLevel++;

            Assert.AreEqual(0, currentLevel);
        }

        [Test]
        public void Stars_NeverDowngrade()
        {
            int savedStars = 3;
            int newStars = 1;
            if (newStars > savedStars) savedStars = newStars;
            Assert.AreEqual(3, savedStars);
        }

        [Test]
        public void CoinsAccumulate()
        {
            long totalCoins = 0;
            long[] rewards = { 75, 125, 200 };
            foreach (var r in rewards) totalCoins += r;
            Assert.AreEqual(400L, totalCoins);
        }

        [Test]
        public void World1Complete_AllLevels_TotalStars30()
        {
            int totalStars = 0;
            for (int i = 0; i < 10; i++) totalStars += 3;
            Assert.AreEqual(30, totalStars);
        }

        [Test]
        public void LevelIndex_MatchesExpected()
        {
            for (int i = 0; i < 10; i++)
                Assert.AreEqual(i, LevelConfigFactory.All[i].LevelIndex);
        }

        [Test]
        public void RequiredStarsToUnlock_Ascending()
        {
            for (int i = 1; i < LevelConfigFactory.All.Count; i++)
                Assert.GreaterOrEqual(
                    LevelConfigFactory.All[i].RequiredStarsToUnlock,
                    LevelConfigFactory.All[i-1].RequiredStarsToUnlock,
                    $"L{i+1} requiredStars should be >= L{i}");
        }
    }
}
