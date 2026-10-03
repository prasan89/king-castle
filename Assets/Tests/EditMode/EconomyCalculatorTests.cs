using NUnit.Framework;
using UnityEngine;
using KingSmash.Economy;
using KingSmash.Save;

namespace KingSmash.Tests.EditMode
{
    public class EconomyCalculatorTests
    {
        private EconomyConfig _config;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<EconomyConfig>();
            _config.coinsPerStar = 25;
            _config.bonusCoinsThreeStars = 100;
            _config.coinsPerStructureDestroyed = 5;
            _config.coinsPerEnemyKilled = 10;
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(_config);

        [Test] public void OneStar_Reward_IsCorrect() => Assert.AreEqual(25, _config.CalculateLevelReward(1));
        [Test] public void TwoStar_Reward_IsCorrect() => Assert.AreEqual(50, _config.CalculateLevelReward(2));
        [Test] public void ThreeStar_Reward_IncludesBonus() => Assert.AreEqual(175, _config.CalculateLevelReward(3));
        [Test] public void ZeroStars_ReturnsZero() => Assert.AreEqual(0, _config.CalculateLevelReward(0));

        [Test]
        public void CanAfford_WithSufficientCoins_ReturnsTrue()
        {
            var data = SaveData.CreateNew();
            data.coins = 1000;
            Assert.IsTrue(EconomyCalculator.CanAfford(data, 500));
        }

        [Test]
        public void CanAfford_WithInsufficientCoins_ReturnsFalse()
        {
            var data = SaveData.CreateNew();
            data.coins = 100;
            Assert.IsFalse(EconomyCalculator.CanAfford(data, 500));
        }

        [Test]
        public void TotalReward_CombinesAllSources()
        {
            var total = EconomyCalculator.CalculateTotalLevelReward(_config, 3, 4, 2);
            // 3-star=175 + 4*5=20 + 2*10=20 = 215
            Assert.AreEqual(215, total);
        }
    }
}
