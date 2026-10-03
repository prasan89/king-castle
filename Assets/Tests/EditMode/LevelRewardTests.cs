using NUnit.Framework;
using KingSmash.Levels;
using KingSmash.Economy;
namespace KingSmash.Tests.EditMode
{
    public class LevelRewardTests
    {
        private EconomyConfig _economy;
        private StarThresholds _defaultThresholds;

        [SetUp]
        public void SetUp()
        {
            // EconomyConfig is a ScriptableObject — use default values inline for testing
            _economy = UnityEngine.ScriptableObject.CreateInstance<EconomyConfig>();
            _economy.coinsPerStar = 25;
            _economy.bonusCoinsThreeStars = 100;
            _economy.coinsPerStructureDestroyed = 5;
            _economy.coinsPerEnemyKilled = 10;

            _defaultThresholds = new StarThresholds
            {
                oneStar_destructionMin      = 0.30f,
                twoStar_destructionMin      = 0.60f,
                twoStar_enemyRatioMin       = 0.50f,
                threeStar_destructionMin    = 0.90f,
                threeStar_enemyRatioMin     = 1.00f,
                threeStar_attemptsRemaining = 1,
            };
        }

        [TearDown]
        public void TearDown() => UnityEngine.Object.DestroyImmediate(_economy);

        [Test]
        public void Build_Victory3Stars_CoinsIncludeBonus()
        {
            var result = LevelRewardCalculator.Build(
                levelIndex: 0, score: 999, destructionRatio: 1f,
                enemiesDefeated: 1, totalEnemies: 1, queenRescued: true,
                attemptsRemaining: 2, isVictory: true, failReason: null,
                starThresholds: _defaultThresholds, economy: _economy);

            Assert.AreEqual(3, result.Stars);
            Assert.IsTrue(result.CoinsEarned > 0);
            // Base: 3×25 + 100 = 175, + destruction bonus + enemy bonus
            Assert.GreaterOrEqual(result.CoinsEarned, 175L);
        }

        [Test]
        public void Build_Defeat_Stars0_Coins0()
        {
            var result = LevelRewardCalculator.Build(
                levelIndex: 0, score: 100, destructionRatio: 0.1f,
                enemiesDefeated: 0, totalEnemies: 3, queenRescued: false,
                attemptsRemaining: 0, isVictory: false, failReason: "No launches",
                starThresholds: _defaultThresholds, economy: _economy);

            Assert.AreEqual(0, result.Stars);
            Assert.AreEqual(0L, result.CoinsEarned);
            Assert.IsFalse(result.IsVictory);
            Assert.AreEqual("No launches", result.FailReason);
        }

        [Test]
        public void Build_NullEconomy_StillBuilds()
        {
            var result = LevelRewardCalculator.Build(
                levelIndex: 2, score: 500, destructionRatio: 0.7f,
                enemiesDefeated: 2, totalEnemies: 2, queenRescued: true,
                attemptsRemaining: 1, isVictory: true, failReason: null,
                starThresholds: _defaultThresholds, economy: null);
            Assert.IsNotNull(result);
            Assert.AreEqual(0L, result.CoinsEarned); // null economy = no coins
        }

        [Test]
        public void Build_LevelIndex_Propagates()
        {
            var result = LevelRewardCalculator.Build(
                levelIndex: 7, score: 0, destructionRatio: 0f,
                enemiesDefeated: 0, totalEnemies: 0, queenRescued: false,
                attemptsRemaining: 0, isVictory: false, failReason: "test",
                starThresholds: null, economy: null);
            Assert.AreEqual(7, result.LevelIndex);
        }

        // ─── LevelConfigFactory data tests ─────────────────────────────

        [Test]
        public void Factory_Has10Levels()
            => Assert.AreEqual(10, LevelConfigFactory.All.Count);

        [Test]
        public void Factory_Level1_IsTutorial()
        {
            var l = LevelConfigFactory.All[0];
            Assert.IsTrue(l.HasTutorialHints);
            Assert.AreEqual(1, l.DifficultyRating);
            Assert.AreEqual(1, l.GuardCount);
            Assert.IsFalse(l.HasEnemyKing);
        }

        [Test]
        public void Factory_Level10_HasAllEnemyTypes()
        {
            var l = LevelConfigFactory.All[9];
            Assert.Greater(l.GuardCount, 0);
            Assert.Greater(l.ShieldGuardCount, 0);
            Assert.Greater(l.ArcherCount, 0);
            Assert.IsTrue(l.HasEnemyKing);
            Assert.IsTrue(l.HasExplosiveBarrel);
        }

        [Test]
        public void Factory_DifficultyIncreases()
        {
            // Difficulty should not decrease between consecutive levels
            for (int i = 1; i < LevelConfigFactory.All.Count; i++)
                Assert.GreaterOrEqual(LevelConfigFactory.All[i].DifficultyRating,
                                      LevelConfigFactory.All[i - 1].DifficultyRating);
        }

        [Test]
        public void Factory_AllLevelsHaveQueen()
        {
            foreach (var l in LevelConfigFactory.All)
                Assert.IsTrue(l.HasQueen, $"Level {l.LevelIndex + 1} is missing Queen");
        }

        [Test]
        public void Factory_ThreeStarScoreGtTwoStarGtOne()
        {
            foreach (var l in LevelConfigFactory.All)
            {
                Assert.Greater(l.ThreeStarScore, l.TwoStarScore,   $"Level {l.LevelIndex + 1}");
                Assert.Greater(l.TwoStarScore,   l.RequiredScore,  $"Level {l.LevelIndex + 1}");
                Assert.Greater(l.RequiredScore,  0,                $"Level {l.LevelIndex + 1}");
            }
        }

        [Test]
        public void Factory_KingLaunches_AtLeast3()
        {
            foreach (var l in LevelConfigFactory.All)
                Assert.GreaterOrEqual(l.KingLaunches, 3, $"Level {l.LevelIndex + 1}");
        }

        [Test]
        public void Factory_ExplosiveBarrel_OnlyInCorrectLevels()
        {
            // Levels 5 (idx 4), 8 (idx 7), 10 (idx 9) have barrels
            Assert.IsTrue(LevelConfigFactory.All[4].HasExplosiveBarrel, "L5 should have barrel");
            Assert.IsTrue(LevelConfigFactory.All[7].HasExplosiveBarrel, "L8 should have barrel");
            Assert.IsTrue(LevelConfigFactory.All[9].HasExplosiveBarrel, "L10 should have barrel");
            Assert.IsFalse(LevelConfigFactory.All[0].HasExplosiveBarrel, "L1 should NOT have barrel");
        }

        [Test]
        public void Factory_EnemyKing_OnlyInLast2()
        {
            Assert.IsTrue(LevelConfigFactory.All[8].HasEnemyKing, "L9 should have Enemy King");
            Assert.IsTrue(LevelConfigFactory.All[9].HasEnemyKing, "L10 should have Enemy King");
            for (int i = 0; i < 8; i++)
                Assert.IsFalse(LevelConfigFactory.All[i].HasEnemyKing, $"L{i+1} should NOT have Enemy King");
        }
    }
}
