using System;
using System.Collections.Generic;
using NUnit.Framework;
using KingSmash.Save;
using KingSmash.Services;
using KingSmash.Progression;
using KingSmash.Economy;
using KingSmash.UI;

namespace KingSmash.Tests.EditMode
{
    // =========================================================================
    // Minimal in-memory ISaveService mock
    // =========================================================================

    internal class InMemorySaveService : ISaveService
    {
        public SaveData Current { get; private set; }
        public int SaveCallCount { get; private set; }

        public InMemorySaveService()
        {
            Current = new SaveData
            {
                playerId        = "test-player",
                kingProgression = new KingProgression()
            };
        }

        public void Load()   { }
        public void Delete() { Current = new SaveData { playerId = "test-player", kingProgression = new KingProgression() }; }
        public void Save()   { SaveCallCount++; }

        public void AddCoins(long amount)
        {
            if (amount <= 0) return;
            Current.coins += amount;
        }

        public void SpendCoins(long amount)
        {
            if (Current.coins < amount) throw new InvalidOperationException("Insufficient coins.");
            Current.coins -= amount;
        }

        public bool TrySpendCoins(long amount)
        {
            if (Current.coins < amount) return false;
            SpendCoins(amount);
            return true;
        }

        public void AddGems(int amount) { Current.gems += amount; }

        public void RecordLevelComplete(int levelIndex, int stars)
        {
            if (!Current.completedLevels.Contains(levelIndex))
                Current.completedLevels.Add(levelIndex);
            var existing = Current.GetStarsForLevel(levelIndex);
            if (stars > existing)
                Current.SetStarsForLevel(levelIndex, stars);
            if (levelIndex >= Current.currentLevel)
                Current.currentLevel = levelIndex + 1;
        }
    }

    // =========================================================================
    // Factory helpers
    // =========================================================================

    internal static class TestFactory
    {
        public static KingUpgradeConfig MakeKingUpgradeConfig(int maxLevel = 5, long xpPerRow = 100)
        {
            var cfg = UnityEngine.ScriptableObject.CreateInstance<KingUpgradeConfig>();
            cfg.maxKingLevel = maxLevel;
            cfg.maxStatLevel = 10;
            cfg.xpPerLevelComplete = 50;
            cfg.xpPerEnemyKilled   = 5;
            cfg.xpPerQueenRescued  = 30;
            cfg.xpBonusThreeStar   = 20;
            cfg.xpDestructionPercentBonus = 1;

            cfg.kingLevels = new List<KingUpgradeConfig.KingLevelRow>();
            for (int i = 1; i <= maxLevel; i++)
            {
                cfg.kingLevels.Add(new KingUpgradeConfig.KingLevelRow
                {
                    kingLevel   = i,
                    xpRequired  = i * xpPerRow,
                    power       = i * 0.5f,
                    speed       = i * 0.3f,
                    smashRadius = i * 0.1f,
                    armor       = i * 0.2f,
                });
            }

            // Add stat upgrade rows (up to maxStatLevel)
            cfg.powerUpgrades       = MakeStatRows(10, 200, 0.5f);
            cfg.speedUpgrades       = MakeStatRows(10, 150, 0.3f);
            cfg.smashRadiusUpgrades = MakeStatRows(10, 100, 0.1f);
            cfg.armorUpgrades       = MakeStatRows(10, 100, 0.2f);

            return cfg;
        }

        private static List<KingUpgradeConfig.StatUpgradeRow> MakeStatRows(int count, long baseCost, float valuePerLevel)
        {
            var list = new List<KingUpgradeConfig.StatUpgradeRow>();
            for (int i = 1; i <= count; i++)
                list.Add(new KingUpgradeConfig.StatUpgradeRow { statLevel = i, coinCost = baseCost * i, valueIncrease = valuePerLevel });
            return list;
        }

        public static LevelResult MakeVictory(int levelIndex = 0, int enemiesDefeated = 0,
            bool queenRescued = false, float destructionRatio = 0f, int stars = 1, long coinsEarned = 100)
        {
            return new LevelResult
            {
                LevelIndex       = levelIndex,
                IsVictory        = true,
                EnemiesDefeated  = enemiesDefeated,
                QueenRescued     = queenRescued,
                DestructionRatio = destructionRatio,
                Stars            = stars,
                CoinsEarned      = coinsEarned,
            };
        }

        public static LevelResult MakeDefeat(int levelIndex = 0)
        {
            return new LevelResult { LevelIndex = levelIndex, IsVictory = false, Stars = 0 };
        }
    }

    // =========================================================================
    // XPRewardCalculatorTests
    // =========================================================================

    [TestFixture]
    public class XPRewardCalculatorTests
    {
        private KingUpgradeConfig _config;

        [SetUp]
        public void SetUp() => _config = TestFactory.MakeKingUpgradeConfig();

        [Test]
        public void XP_IsZero_OnFailure()
        {
            Assert.AreEqual(0L, XPRewardCalculator.Calculate(_config, TestFactory.MakeDefeat()));
        }

        [Test]
        public void XP_IncludesBase_OnVictory()
        {
            var result = TestFactory.MakeVictory(enemiesDefeated: 0, queenRescued: false,
                destructionRatio: 0f, stars: 1);
            long xp = XPRewardCalculator.Calculate(_config, result);
            Assert.AreEqual(_config.xpPerLevelComplete, xp);
        }

        [Test]
        public void XP_Includes_EnemyXP()
        {
            var result = TestFactory.MakeVictory(enemiesDefeated: 4, stars: 1);
            long xp = XPRewardCalculator.Calculate(_config, result);
            long expected = _config.xpPerLevelComplete + 4 * _config.xpPerEnemyKilled;
            Assert.AreEqual(expected, xp);
        }

        [Test]
        public void XP_Includes_QueenXP()
        {
            var result = TestFactory.MakeVictory(queenRescued: true, stars: 1);
            long xp = XPRewardCalculator.Calculate(_config, result);
            long expected = _config.xpPerLevelComplete + _config.xpPerQueenRescued;
            Assert.AreEqual(expected, xp);
        }

        [Test]
        public void XP_Includes_StarBonus_ThreeStar()
        {
            var result = TestFactory.MakeVictory(stars: 3);
            long xp = XPRewardCalculator.Calculate(_config, result);
            long expected = _config.xpPerLevelComplete + _config.xpBonusThreeStar;
            Assert.AreEqual(expected, xp);
        }

        [Test]
        public void XP_Includes_DestructionBonus()
        {
            var result = TestFactory.MakeVictory(destructionRatio: 0.8f, stars: 1);
            long xp = XPRewardCalculator.Calculate(_config, result);
            long expected = _config.xpPerLevelComplete + 80L * _config.xpDestructionPercentBonus;
            Assert.AreEqual(expected, xp);
        }

        [Test]
        public void XP_NullConfig_ReturnsZero()
        {
            Assert.AreEqual(0L, XPRewardCalculator.Calculate(null, TestFactory.MakeVictory()));
        }
    }

    // =========================================================================
    // KingProgressionServiceTests
    // =========================================================================

    [TestFixture]
    public class KingProgressionServiceTests
    {
        private InMemorySaveService    _save;
        private KingUpgradeConfig      _config;
        private KingProgressionService _svc;

        private int  _levelUpOldLevel;
        private int  _levelUpNewLevel;
        private bool _levelUpFired;

        [SetUp]
        public void SetUp()
        {
            KingProgressionService.OnKingLevelUp = null;
            KingProgressionService.OnXPChanged   = null;

            _save   = new InMemorySaveService();
            _config = TestFactory.MakeKingUpgradeConfig(maxLevel: 5, xpPerRow: 100);
            _svc    = new KingProgressionService(_save, _config);

            _levelUpFired    = false;
            _levelUpOldLevel = 0;
            _levelUpNewLevel = 0;

            KingProgressionService.OnKingLevelUp += (old, next, _) =>
            {
                _levelUpFired    = true;
                _levelUpOldLevel = old;
                _levelUpNewLevel = next;
            };
        }

        [Test]
        public void AddXP_IncreasesXP()
        {
            _svc.AddXP(50);
            Assert.AreEqual(50L, _svc.KingXP);
        }

        [Test]
        public void AddXP_DoesNotLevelUp_BelowThreshold()
        {
            long required = _svc.XPRequiredForNextLevel(1);
            _svc.AddXP(required - 1);
            Assert.AreEqual(1, _svc.KingLevel);
            Assert.IsFalse(_levelUpFired);
        }

        [Test]
        public void AddXP_LevelsUp_AtThreshold()
        {
            long required = _svc.XPRequiredForNextLevel(1);
            _svc.AddXP(required);
            Assert.AreEqual(2, _svc.KingLevel);
            Assert.IsTrue(_levelUpFired);
        }

        [Test]
        public void LevelUp_FiresEvent_WithCorrectLevels()
        {
            long required = _svc.XPRequiredForNextLevel(1);
            _svc.AddXP(required);
            Assert.IsTrue(_levelUpFired);
            Assert.AreEqual(1, _levelUpOldLevel);
            Assert.AreEqual(2, _levelUpNewLevel);
        }

        [Test]
        public void LevelUp_DoesNotExceedMaxLevel()
        {
            _svc.AddXP(long.MaxValue / 2);
            Assert.AreEqual(_config.maxKingLevel, _svc.KingLevel);
            Assert.AreEqual(0L, _svc.KingXP);
        }

        [Test]
        public void GetCurrentStats_ReturnsZero_WhenNoRows()
        {
            var emptyCfg = UnityEngine.ScriptableObject.CreateInstance<KingUpgradeConfig>();
            emptyCfg.maxKingLevel = 10;
            emptyCfg.maxStatLevel = 10;
            var svc   = new KingProgressionService(_save, emptyCfg);
            var stats = svc.GetCurrentStats();
            Assert.AreEqual(0f, stats.Power);
            Assert.AreEqual(0f, stats.Speed);
        }
    }

    // =========================================================================
    // CurrencyServiceTests
    // =========================================================================

    [TestFixture]
    public class CurrencyServiceTests
    {
        private InMemorySaveService _save;
        private CurrencyService     _svc;

        [SetUp]
        public void SetUp()
        {
            _save = new InMemorySaveService();
            _svc  = new CurrencyService(_save);
        }

        [Test]
        public void TryAdd_IncreasesBalance()
        {
            bool ok = _svc.TryAdd(200);
            Assert.IsTrue(ok);
            Assert.AreEqual(200L, _svc.Balance);
        }

        [Test]
        public void TrySpend_DecreasesBalance()
        {
            _svc.TryAdd(500);
            bool ok = _svc.TrySpend(200, "test");
            Assert.IsTrue(ok);
            Assert.AreEqual(300L, _svc.Balance);
        }

        [Test]
        public void TrySpend_Fails_WhenInsufficient()
        {
            _svc.TryAdd(100);
            bool ok = _svc.TrySpend(200, "test");
            Assert.IsFalse(ok);
            Assert.AreEqual(100L, _svc.Balance);
        }

        [Test]
        public void TrySpend_NeverGoesNegative()
        {
            _svc.TryAdd(50);
            _svc.TrySpend(9999, "test");
            Assert.GreaterOrEqual(_svc.Balance, 0L);
        }

        [Test]
        public void TryAdd_ZeroAmount_Fails()
        {
            bool ok = _svc.TryAdd(0);
            Assert.IsFalse(ok);
            Assert.AreEqual(0L, _svc.Balance);
        }

        [Test]
        public void GetHistory_ReturnsTransactions()
        {
            _svc.TryAdd(100, "reward", out _);
            _svc.TrySpend(30, "upgrade", out _);
            var history = _svc.GetHistory();
            Assert.GreaterOrEqual(history.Length, 2);
        }

        [Test]
        public void History_Capped_At50()
        {
            for (int i = 0; i < 60; i++)
                _svc.TryAdd(10, "loop", out _);
            var history = _svc.GetHistory();
            Assert.LessOrEqual(history.Length, 50);
        }
    }

    // =========================================================================
    // KingUpgradeServiceTests
    // =========================================================================

    [TestFixture]
    public class KingUpgradeServiceTests
    {
        private InMemorySaveService _save;
        private KingUpgradeConfig   _config;
        private CurrencyService     _currency;
        private KingUpgradeService  _svc;

        [SetUp]
        public void SetUp()
        {
            KingUpgradeService.OnStatUpgraded = null;

            _save     = new InMemorySaveService();
            _config   = TestFactory.MakeKingUpgradeConfig(maxLevel: 10, xpPerRow: 100);
            _currency = new CurrencyService(_save);
            _svc      = new KingUpgradeService(_save, _config, _currency);
        }

        [Test]
        public void TryUpgrade_Power_Success()
        {
            _currency.TryAdd(10000);
            int levelBefore = _save.Current.kingProgression.PowerLevel;
            var result = _svc.TryUpgradeStat(KingStat.Power);
            Assert.AreEqual(UpgradeResult.Success, result);
            Assert.AreEqual(levelBefore + 1, _save.Current.kingProgression.PowerLevel);
        }

        [Test]
        public void TryUpgrade_Fails_InsufficientCoins()
        {
            _save.Current.coins = 0;
            int levelBefore = _save.Current.kingProgression.PowerLevel;
            var result = _svc.TryUpgradeStat(KingStat.Power);
            Assert.AreEqual(UpgradeResult.InsufficientCoins, result);
            Assert.AreEqual(levelBefore, _save.Current.kingProgression.PowerLevel);
        }

        [Test]
        public void TryUpgrade_Fails_AtMaxLevel()
        {
            _save.Current.kingProgression.PowerLevel = 10;
            _currency.TryAdd(999999);
            var result = _svc.TryUpgradeStat(KingStat.Power);
            Assert.AreEqual(UpgradeResult.AlreadyMaxLevel, result);
            Assert.AreEqual(10, _save.Current.kingProgression.PowerLevel);
        }

        [Test]
        public void GetUpgradePreview_ShowsCorrectValues()
        {
            var preview = _svc.GetUpgradePreview(KingStat.Power);
            Assert.AreEqual(1, preview.currentLevel);
            Assert.IsFalse(preview.isMax);
            Assert.Greater(preview.cost, 0L);
        }
    }

    // =========================================================================
    // SaveDataV2Tests
    // =========================================================================

    [TestFixture]
    public class SaveDataV2Tests
    {
        private SaveData _data;

        [SetUp]
        public void SetUp()
        {
            _data = new SaveData { kingProgression = new KingProgression() };
        }

        [Test]
        public void IsRewardProcessed_False_Initially()
        {
            Assert.IsFalse(_data.IsRewardProcessed(0));
        }

        [Test]
        public void MarkRewardProcessed_Then_IsProcessed_True()
        {
            _data.MarkRewardProcessed(0);
            Assert.IsTrue(_data.IsRewardProcessed(0));
        }

        [Test]
        public void RewardProcessed_DoesNotAffectStarTracking()
        {
            _data.MarkRewardProcessed(0);
            _data.SetStarsForLevel(0, 3);
            Assert.AreEqual(3, _data.GetStarsForLevel(0));
        }

        [Test]
        public void KingProgression_DefaultValues()
        {
            var prog = _data.kingProgression;
            Assert.AreEqual(1, prog.KingLevel);
            Assert.AreEqual(0L, prog.KingXP);
            Assert.AreEqual(1, prog.PowerLevel);
            Assert.AreEqual(1, prog.SpeedLevel);
            Assert.AreEqual(1, prog.SmashRadiusLevel);
            Assert.AreEqual(1, prog.ArmorLevel);
        }
    }

    // =========================================================================
    // KingProgressionIntegrationTests
    // =========================================================================

    [TestFixture]
    public class KingProgressionIntegrationTests
    {
        private InMemorySaveService    _save;
        private KingUpgradeConfig      _config;
        private KingProgressionService _progressionSvc;
        private CurrencyService        _currencySvc;

        [SetUp]
        public void SetUp()
        {
            KingProgressionService.OnKingLevelUp = null;
            KingProgressionService.OnXPChanged   = null;

            _save           = new InMemorySaveService();
            _config         = TestFactory.MakeKingUpgradeConfig(maxLevel: 5, xpPerRow: 100);
            _progressionSvc = new KingProgressionService(_save, _config);
            _currencySvc    = new CurrencyService(_save);
        }

        private void CommitResult(LevelResult result)
        {
            if (result.IsVictory)
            {
                bool alreadyProcessed = _save.Current.IsRewardProcessed(result.LevelIndex);

                // Always update stars
                int prev = _save.Current.GetStarsForLevel(result.LevelIndex);
                if (result.Stars > prev)
                    _save.Current.SetStarsForLevel(result.LevelIndex, result.Stars);

                if (!alreadyProcessed)
                {
                    long xp = XPRewardCalculator.Calculate(_config, result);
                    _progressionSvc.AddXP(xp);
                    _currencySvc.TryAdd(result.CoinsEarned);
                    _save.Current.MarkRewardProcessed(result.LevelIndex);
                    _save.Save();
                }
            }
        }

        [Test]
        public void LevelComplete_Awards_CoinsAndXP()
        {
            var result = TestFactory.MakeVictory(levelIndex: 0, enemiesDefeated: 2,
                queenRescued: false, destructionRatio: 0f, stars: 1, coinsEarned: 150);
            CommitResult(result);

            long expectedXP = XPRewardCalculator.Calculate(_config, result);
            Assert.AreEqual(expectedXP, _save.Current.kingProgression.KingXP);
            Assert.AreEqual(150L, _save.Current.coins);
        }

        [Test]
        public void LevelComplete_Duplicate_SkipsRewards()
        {
            var result = TestFactory.MakeVictory(levelIndex: 0, coinsEarned: 200, stars: 1);
            CommitResult(result);
            long coinsAfterFirst = _save.Current.coins;
            long xpAfterFirst    = _save.Current.kingProgression.KingXP;

            CommitResult(result);

            Assert.AreEqual(coinsAfterFirst, _save.Current.coins, "Coins must not increase on duplicate commit.");
            Assert.AreEqual(xpAfterFirst,    _save.Current.kingProgression.KingXP, "XP must not increase on duplicate commit.");
        }

        [Test]
        public void LevelComplete_AlwaysUpdates_Stars()
        {
            CommitResult(TestFactory.MakeVictory(levelIndex: 0, stars: 1, coinsEarned: 100));
            Assert.AreEqual(1, _save.Current.GetStarsForLevel(0));

            CommitResult(TestFactory.MakeVictory(levelIndex: 0, stars: 3, coinsEarned: 100));
            Assert.AreEqual(3, _save.Current.GetStarsForLevel(0),
                "Stars must update even on duplicate when new star count is higher.");
        }
    }
}
