using System;
using System.Collections.Generic;
using NUnit.Framework;
using KingSmash.Save;
using KingSmash.Services;
using KingSmash.Progression;
using KingSmash.Economy;
using KingSmash.PowerUps;
using UnityEngine;

namespace KingSmash.Tests.EditMode
{
    // =========================================================================
    // M7 local ISaveService mock — avoids clash with InMemorySaveService
    // =========================================================================

    internal class M7SaveService : ISaveService
    {
        public SaveData Current { get; private set; }

        public M7SaveService()
        {
            Current = new SaveData
            {
                playerId         = "m7-test",
                kingProgression  = new KingProgression(),
                powerUpInventory = new PowerUpInventory(),
            };
        }

        public void Load()   { }
        public void Save()   { }
        public void Delete() { Current = new SaveData { playerId = "m7-test", kingProgression = new KingProgression(), powerUpInventory = new PowerUpInventory() }; }

        public void AddCoins(long amount)    { if (amount > 0) Current.coins += amount; }
        public void SpendCoins(long amount)  { if (Current.coins < amount) throw new InvalidOperationException("Insufficient coins."); Current.coins -= amount; }
        public bool TrySpendCoins(long amount) { if (Current.coins < amount) return false; SpendCoins(amount); return true; }
        public void AddGems(int amount)      { Current.gems += amount; }
        public void RecordLevelComplete(int levelIndex, int stars)
        {
            if (!Current.completedLevels.Contains(levelIndex)) Current.completedLevels.Add(levelIndex);
            int prev = Current.GetStarsForLevel(levelIndex);
            if (stars > prev) Current.SetStarsForLevel(levelIndex, stars);
        }
    }

    // =========================================================================
    // CurrencyFormatterTests
    // =========================================================================

    [TestFixture]
    public class CurrencyFormatterTests
    {
        [Test] public void Format_Zero()              => Assert.AreEqual("0",     CurrencyFormatter.Format(0));
        [Test] public void Format_999()               => Assert.AreEqual("999",   CurrencyFormatter.Format(999));
        [Test] public void Format_1000_Is1Point0K()   => Assert.AreEqual("1.0K",  CurrencyFormatter.Format(1000));
        [Test] public void Format_1200_Is1Point2K()   => Assert.AreEqual("1.2K",  CurrencyFormatter.Format(1200));
        [Test] public void Format_1500_Is1Point5K()   => Assert.AreEqual("1.5K",  CurrencyFormatter.Format(1500));
        [Test] public void Format_10500_Is10Point5K() => Assert.AreEqual("10.5K", CurrencyFormatter.Format(10500));
        [Test] public void Format_125000_Is125K()     => Assert.AreEqual("125K",  CurrencyFormatter.Format(125000));
        [Test] public void Format_1200000_Is1Point2M() => Assert.AreEqual("1.2M", CurrencyFormatter.Format(1200000));
        [Test] public void Format_2000000_Is2Point0M() => Assert.AreEqual("2.0M", CurrencyFormatter.Format(2000000));

        [Test]
        public void FormatExact_1200_HasComma()
        {
            Assert.AreEqual("1,200", CurrencyFormatter.FormatExact(1200));
        }

        [Test]
        public void FormatExact_Zero_ReturnsZeroString()
        {
            Assert.AreEqual("0", CurrencyFormatter.FormatExact(0));
        }

        [Test]
        public void FormatGems_5_HasGPrefix()
        {
            string result = CurrencyFormatter.FormatGems(5);
            Assert.IsTrue(result.StartsWith("G"), $"Expected prefix 'G', got: {result}");
            Assert.IsTrue(result.Contains("5"),   $"Expected '5' in result, got: {result}");
        }

        [Test]
        public void Format_NegativeValue_DoesNotCrash()
        {
            Assert.DoesNotThrow(() => CurrencyFormatter.Format(-1));
        }

        [Test]
        public void Format_9999_EndsWith_K()
        {
            string result = CurrencyFormatter.Format(9999);
            Assert.IsTrue(result.EndsWith("K"), $"Expected K suffix, got: {result}");
        }

        [Test]
        public void Format_50000_Is50K()
        {
            Assert.AreEqual("50K", CurrencyFormatter.Format(50000));
        }
    }

    // =========================================================================
    // EconomyConfigM7Tests
    // =========================================================================

    [TestFixture]
    public class EconomyConfigM7Tests
    {
        private EconomyConfig _config;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<EconomyConfig>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_config != null) UnityEngine.Object.DestroyImmediate(_config);
        }

        [Test]
        public void CalculateLevelReward_OneStar_ReturnsCoinPerStar()
        {
            _config.coinsPerStar       = 25;
            _config.bonusCoinsThreeStars = 100;
            Assert.AreEqual(25L, _config.CalculateLevelReward(1));
        }

        [Test]
        public void CalculateLevelReward_TwoStars_NoBonus()
        {
            _config.coinsPerStar       = 25;
            _config.bonusCoinsThreeStars = 100;
            Assert.AreEqual(50L, _config.CalculateLevelReward(2));
        }

        [Test]
        public void CalculateLevelReward_ThreeStars_IncludesBonus()
        {
            _config.coinsPerStar       = 25;
            _config.bonusCoinsThreeStars = 100;
            Assert.AreEqual(175L, _config.CalculateLevelReward(3));
        }

        [Test]
        public void CalculateLevelReward_ZeroStars_ReturnsZero()
        {
            _config.coinsPerStar       = 25;
            _config.bonusCoinsThreeStars = 100;
            Assert.AreEqual(0L, _config.CalculateLevelReward(0));
        }

        [Test]
        public void GetDestructionBonus_BelowFirstThreshold_ReturnsZero()
        {
            long bonus = _config.GetDestructionBonus(0.4f);
            Assert.AreEqual(0L, bonus);
        }

        [Test]
        public void GetDestructionBonus_AboveHalfThreshold_ReturnsNonZero()
        {
            long bonus = _config.GetDestructionBonus(0.6f);
            Assert.Greater(bonus, 0L);
        }

        [Test]
        public void GetDestructionBonus_NearFull_GreaterOrEqualToMid()
        {
            long bonus95 = _config.GetDestructionBonus(0.95f);
            long bonus60 = _config.GetDestructionBonus(0.6f);
            Assert.GreaterOrEqual(bonus95, bonus60);
        }

        [Test]
        public void GetWorldReward_World0_ReturnsEntry()
        {
            var wr = _config.GetWorldReward(0);
            Assert.IsNotNull(wr);
            Assert.Greater(wr.coins, 0L);
        }

        [Test]
        public void GetWorldReward_NonExistent_ReturnsNull()
        {
            var wr = _config.GetWorldReward(99);
            Assert.IsNull(wr);
        }

        [Test]
        public void GetPowerUpPrice_Bomb_ReturnsCost100()
        {
            var price = _config.GetPowerUpPrice("powerup_bomb");
            Assert.IsNotNull(price);
            Assert.AreEqual(100L, price.coinCost);
        }

        [Test]
        public void GetPowerUpPrice_NonExistent_ReturnsNull()
        {
            Assert.IsNull(_config.GetPowerUpPrice("powerup_nonexistent"));
        }
    }

    // =========================================================================
    // RewardResultTests
    // =========================================================================

    [TestFixture]
    public class RewardResultTests
    {
        [Test]
        public void Empty_HasZeroCoins()
        {
            Assert.AreEqual(0L, RewardResult.Empty.coins);
        }

        [Test]
        public void Empty_HasZeroGems()
        {
            Assert.AreEqual(0, RewardResult.Empty.gems);
        }

        [Test]
        public void Empty_HasZeroXP()
        {
            Assert.AreEqual(0L, RewardResult.Empty.xp);
        }

        [Test]
        public void HasPowerUp_False_WhenCountIsZero()
        {
            Assert.IsFalse(new RewardResult { powerUpCount = 0 }.HasPowerUp);
        }

        [Test]
        public void HasPowerUp_True_WhenCountAboveZero()
        {
            Assert.IsTrue(new RewardResult { powerUpCount = 1, powerUpType = PowerUpType.Bomb }.HasPowerUp);
        }

        [Test]
        public void HasPowerUp_False_WhenNullableTypeIsNull()
        {
            Assert.IsFalse(new RewardResult { powerUpType = null, powerUpCount = 0 }.HasPowerUp);
        }
    }

    // =========================================================================
    // RewardServiceTests
    // =========================================================================

    [TestFixture]
    public class RewardServiceTests
    {
        private M7SaveService          _save;
        private CurrencyService        _currency;
        private KingProgressionService _progression;
        private EconomyConfig          _economyConfig;
        private RewardService          _svc;

        [SetUp]
        public void SetUp()
        {
            CurrencyService.OnCoinsChanged    = null;
            CurrencyService.OnInsufficientFunds = null;

            _save          = new M7SaveService();
            _currency      = new CurrencyService(_save);
            _economyConfig = ScriptableObject.CreateInstance<EconomyConfig>();
            _economyConfig.coinsPerStar         = 25;
            _economyConfig.bonusCoinsThreeStars  = 100;
            _economyConfig.coinsPerEnemyKilled   = 10;
            _economyConfig.baseLevelCoinsMin     = 100;
            _economyConfig.baseLevelCoinsMax     = 500;
            _economyConfig.levelCoinScalePer10Levels = 50;
            _economyConfig.queenRescueBonus      = 100;
            _economyConfig.oneStarBonus          = 0;
            _economyConfig.twoStarBonus          = 50;
            _economyConfig.threeStarBonus        = 200;

            var upgradeConfig = ScriptableObject.CreateInstance<KingUpgradeConfig>();
            upgradeConfig.maxKingLevel             = 5;
            upgradeConfig.maxStatLevel             = 10;
            upgradeConfig.xpPerLevelComplete       = 50;
            upgradeConfig.xpPerEnemyKilled         = 5;
            upgradeConfig.xpPerQueenRescued        = 30;
            upgradeConfig.xpBonusThreeStar         = 20;
            upgradeConfig.xpDestructionPercentBonus = 1;
            upgradeConfig.kingLevels = new List<KingUpgradeConfig.KingLevelRow>
            {
                new KingUpgradeConfig.KingLevelRow { kingLevel = 1, xpRequired = 100 },
                new KingUpgradeConfig.KingLevelRow { kingLevel = 2, xpRequired = 200 },
            };

            _progression = new KingProgressionService(_save, upgradeConfig);
            _svc         = new RewardService(_save, _currency, _progression, _economyConfig);
        }

        [TearDown]
        public void TearDown()
        {
            if (_economyConfig != null) UnityEngine.Object.DestroyImmediate(_economyConfig);
        }

        private static KingSmash.UI.LevelResult MakeVictory(int levelIndex = 0, int stars = 1,
            int enemiesDefeated = 0, bool queenRescued = false, float destructionRatio = 0f)
        {
            return new KingSmash.UI.LevelResult
            {
                LevelIndex       = levelIndex,
                IsVictory        = true,
                Stars            = stars,
                EnemiesDefeated  = enemiesDefeated,
                QueenRescued     = queenRescued,
                DestructionRatio = destructionRatio,
            };
        }

        private static KingSmash.UI.LevelResult MakeDefeat(int levelIndex = 0)
        {
            return new KingSmash.UI.LevelResult { LevelIndex = levelIndex, IsVictory = false, Stars = 0 };
        }

        [Test]
        public void ClaimLevelReward_Victory_ReturnsNonEmptyResult()
        {
            var result = MakeVictory(levelIndex: 0, stars: 2, enemiesDefeated: 3);
            var reward = _svc.ClaimLevelReward(result);
            Assert.IsTrue(reward.coins > 0 || reward.xp > 0, "Expected non-empty reward on victory.");
        }

        [Test]
        public void ClaimLevelReward_Defeat_ReturnsEmpty()
        {
            var result = MakeDefeat(levelIndex: 0);
            var reward = _svc.ClaimLevelReward(result);
            Assert.AreEqual(0L, reward.coins, "Defeat should award 0 coins.");
            Assert.AreEqual(0L, reward.xp,    "Defeat should award 0 xp.");
        }

        [Test]
        public void ClaimLevelReward_Duplicate_SecondCallReturnsEmpty()
        {
            var result = MakeVictory(levelIndex: 5, stars: 1);
            var first  = _svc.ClaimLevelReward(result);
            var second = _svc.ClaimLevelReward(result);

            Assert.IsTrue(first.coins > 0 || first.xp > 0, "First claim should award something.");
            Assert.AreEqual(0L, second.coins, "Second claim should award 0 coins.");
            Assert.AreEqual(0L, second.xp,    "Second claim should award 0 xp.");
        }

        [Test]
        public void ClaimLevelReward_Victory_AddsCoinsToBalance()
        {
            long before = _save.Current.coins;
            _svc.ClaimLevelReward(MakeVictory(levelIndex: 1, stars: 3));
            Assert.Greater(_save.Current.coins, before);
        }

        [Test]
        public void ClaimLevelReward_MarksLevelAsProcessed()
        {
            _svc.ClaimLevelReward(MakeVictory(levelIndex: 7, stars: 1));
            Assert.IsTrue(_save.Current.IsRewardProcessed(7));
        }

        [Test]
        public void ClaimWorldReward_AwardsOnce()
        {
            var reward = _svc.ClaimWorldReward(0, _economyConfig);
            Assert.IsTrue(reward.coins > 0 || reward.gems > 0, "First world reward should award something.");
        }

        [Test]
        public void ClaimWorldReward_SecondCall_ReturnsEmpty()
        {
            _svc.ClaimWorldReward(0, _economyConfig);
            var second = _svc.ClaimWorldReward(0, _economyConfig);
            Assert.AreEqual(0L, second.coins);
            Assert.AreEqual(0,  second.gems);
        }

        [Test]
        public void ClaimLevelReward_NeverCausesNegativeBalance()
        {
            _save.Current.coins = 0;
            Assert.DoesNotThrow(() => _svc.ClaimLevelReward(MakeDefeat(levelIndex: 99)));
            Assert.GreaterOrEqual(_save.Current.coins, 0L);
        }

        [Test]
        public void ClaimLevelReward_QueenRescue_IncludesBonus()
        {
            var result    = MakeVictory(levelIndex: 0, stars: 1, queenRescued: true);
            var noQueen   = MakeVictory(levelIndex: 1, stars: 1, queenRescued: false);
            var withQueen = _svc.ClaimLevelReward(result);
            var without   = _svc.ClaimLevelReward(noQueen);
            Assert.GreaterOrEqual(withQueen.coins, without.coins);
        }
    }

    // =========================================================================
    // SaveDataV4Tests
    // =========================================================================

    [TestFixture]
    public class SaveDataV4Tests
    {
        private SaveData _data;

        [SetUp]
        public void SetUp()
        {
            _data = new SaveData
            {
                kingProgression  = new KingProgression(),
                powerUpInventory = new PowerUpInventory(),
            };
        }

        [Test]
        public void CurrentVersion_Is4()
        {
            Assert.AreEqual(4, SaveData.CurrentVersion);
        }

        [Test]
        public void EconomyVersion_DefaultsTo1()
        {
            Assert.AreEqual(1, _data.economyVersion);
        }

        [Test]
        public void Gems_DefaultsToZero()
        {
            Assert.AreEqual(0, _data.gems);
        }

        [Test]
        public void Coins_DefaultsToZero()
        {
            Assert.AreEqual(0L, _data.coins);
        }

        [Test]
        public void IsRewardProcessed_False_Initially()
        {
            Assert.IsFalse(_data.IsRewardProcessed(0));
        }

        [Test]
        public void MarkRewardProcessed_PersistsCorrectly()
        {
            _data.MarkRewardProcessed(3);
            Assert.IsTrue(_data.IsRewardProcessed(3));
            Assert.IsFalse(_data.IsRewardProcessed(4));
        }

        [Test]
        public void PowerUpInventory_NonNull_ByDefault()
        {
            Assert.IsNotNull(_data.powerUpInventory);
        }
    }
}
