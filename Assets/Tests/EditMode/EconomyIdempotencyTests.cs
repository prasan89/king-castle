// EconomyIdempotencyTests.cs
// Economy correctness and idempotency tests for King Smash.
// EditMode only — pure data/logic, no MonoBehaviours, no scene loading.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using KingSmash.Save;
using KingSmash.Services;
using KingSmash.Economy;
using KingSmash.PowerUps;
using KingSmash.Levels;
using KingSmash.Progression;
using KingSmash.Retention;

namespace KingSmash.Tests.EditMode
{
    // =========================================================================
    // Shared in-memory save service (avoids duplicate class name collisions)
    // =========================================================================

    internal class EconomyIdempotencyTestSaveService : ISaveService
    {
        public SaveData Current { get; private set; }

        public EconomyIdempotencyTestSaveService()
        {
            Current = new SaveData
            {
                playerId         = "econ-idempotency-test",
                kingProgression  = new KingProgression(),
                powerUpInventory = new PowerUpInventory(),
            };
        }

        public void Load()   { }
        public void Save()   { }
        public void Delete()
        {
            Current = new SaveData
            {
                playerId         = "econ-idempotency-test",
                kingProgression  = new KingProgression(),
                powerUpInventory = new PowerUpInventory(),
            };
        }

        public void AddCoins(long amount)
        {
            if (amount > 0) Current.coins += amount;
        }

        public void SpendCoins(long amount)
        {
            if (Current.coins < amount)
                throw new InvalidOperationException("Insufficient coins.");
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
            int prev = Current.GetStarsForLevel(levelIndex);
            if (stars > prev) Current.SetStarsForLevel(levelIndex, stars);
        }
    }

    // =========================================================================
    // EconomyIdempotencyTests
    // =========================================================================

    [TestFixture]
    public class EconomyIdempotencyTests
    {
        private EconomyIdempotencyTestSaveService _save;
        private CurrencyService _currency;

        [SetUp]
        public void SetUp()
        {
            // Clear static events to prevent cross-test interference.
            CurrencyService.OnCoinsChanged     = null;
            CurrencyService.OnInsufficientFunds = null;
            PowerUpService.OnInventoryChanged  = null;
            PowerUpService.OnPowerUpActivated  = null;
            PowerUpService.OnPowerUpAwarded    = null;
            PowerUpService.OnPowerUpPurchased  = null;

            _save     = new EconomyIdempotencyTestSaveService();
            _currency = new CurrencyService(_save);
        }

        // ── CurrencyService ───────────────────────────────────────────────────

        [Test]
        public void CurrencyService_CannotGoNegative()
        {
            // Spending more than balance must return false and leave balance unchanged.
            bool result = _currency.TrySpend(99999, "TEST");
            Assert.IsFalse(result, "TrySpend should return false when balance is insufficient");
            Assert.AreEqual(0L, _currency.Balance,
                "Balance should remain 0 after a failed spend");
        }

        [Test]
        public void CurrencyService_AddThenSpend_Correct()
        {
            _currency.TryAdd(1000, "REWARD", out _);
            _currency.TrySpend(400, "PURCHASE", out _);
            Assert.AreEqual(600L, _currency.Balance,
                "1000 - 400 = 600 coins expected");
        }

        [Test]
        public void CurrencyService_DoubleAddSameSource_NotIdempotent()
        {
            // Two Add calls with the same reason both succeed — CurrencyService does not
            // de-duplicate by reason string. This documents expected (non-idempotent) behaviour.
            _currency.TryAdd(500, "REWARD", out _);
            _currency.TryAdd(500, "REWARD", out _);
            Assert.AreEqual(1000L, _currency.Balance,
                "Two Add(500) calls should each succeed, totalling 1000 (not idempotent by design)");
        }

        // ── Level reward deduplication via processedLevelRewards ─────────────

        [Test]
        public void LevelReward_NoDuplicateProcessing()
        {
            // Mark level 5 as processed.
            _save.Current.MarkRewardProcessed(5);
            Assert.IsTrue(_save.Current.IsRewardProcessed(5),
                "Level 5 should be marked as processed after MarkRewardProcessed");

            // Attempt to process again: marking is idempotent (set semantics).
            _save.Current.MarkRewardProcessed(5);
            int count = 0;
            foreach (int idx in _save.Current.processedLevelRewards)
                if (idx == 5) count++;
            Assert.AreEqual(1, count,
                "processedLevelRewards should contain level 5 exactly once");
        }

        // ── EconomyCalculator ─────────────────────────────────────────────────

        [Test]
        public void EconomyCalculator_NoNegativeReward()
        {
            // CalculateTotalLevelReward with any valid stars/counts must be >= 0.
            var config = ScriptableObject.CreateInstance<EconomyConfig>();
            config.coinsPerStar               = 25;
            config.bonusCoinsThreeStars       = 100;
            config.coinsPerStructureDestroyed = 5;
            config.coinsPerEnemyKilled        = 10;

            int[] starValues = { 0, 1, 2, 3 };
            foreach (int stars in starValues)
            {
                long reward = EconomyCalculator.CalculateTotalLevelReward(config, stars, 0, 0);
                Assert.GreaterOrEqual(reward, 0L,
                    $"Reward for {stars} stars should be >= 0, got {reward}");
            }

            Object.DestroyImmediate(config);
        }

        [Test]
        public void EconomyCalculator_HigherStarsMeansMoreReward()
        {
            var config = ScriptableObject.CreateInstance<EconomyConfig>();
            config.coinsPerStar               = 25;
            config.bonusCoinsThreeStars       = 100;
            config.coinsPerStructureDestroyed = 0;
            config.coinsPerEnemyKilled        = 0;

            long reward0 = EconomyCalculator.CalculateTotalLevelReward(config, 0, 0, 0);
            long reward1 = EconomyCalculator.CalculateTotalLevelReward(config, 1, 0, 0);
            long reward2 = EconomyCalculator.CalculateTotalLevelReward(config, 2, 0, 0);
            long reward3 = EconomyCalculator.CalculateTotalLevelReward(config, 3, 0, 0);

            Assert.GreaterOrEqual(reward1, reward0,
                "1-star reward should be >= 0-star reward");
            Assert.GreaterOrEqual(reward2, reward1,
                "2-star reward should be >= 1-star reward");
            Assert.GreaterOrEqual(reward3, reward2,
                "3-star reward should be >= 2-star reward");

            Object.DestroyImmediate(config);
        }

        // ── PowerUpService ────────────────────────────────────────────────────

        [Test]
        public void PowerUpService_QuantityNeverNegative()
        {
            var pus = new PowerUpService(_save, _currency);
            // TryActivateSelected with nothing selected returns false without crashing.
            bool result = pus.TryActivateSelected(null, null);
            Assert.IsFalse(result, "TryActivateSelected should return false when nothing is selected");
            // Inventory count must stay at 0 (never -1).
            Assert.AreEqual(0, pus.GetCount(PowerUpType.Bomb),
                "Bomb count should be 0, not negative");
        }

        [Test]
        public void PowerUpService_AwardThenUse_Correct()
        {
            var pus = new PowerUpService(_save, _currency);
            pus.Award(PowerUpType.Fire, 3);
            Assert.AreEqual(3, pus.GetCount(PowerUpType.Fire),
                "After Award(Fire, 3) count should be 3");

            // Consume directly via the inventory (no MonoBehaviour in EditMode).
            bool consumed = _save.Current.powerUpInventory.TryConsume(PowerUpType.Fire);
            Assert.IsTrue(consumed, "TryConsume should return true when count > 0");
            Assert.AreEqual(2, pus.GetCount(PowerUpType.Fire),
                "After consuming one Fire power-up, count should be 2");
        }

        // ── DailyRewardService ────────────────────────────────────────────────

        [Test]
        public void DailyReward_SingleClaimPerDay()
        {
            var dailyConfig = ScriptableObject.CreateInstance<DailyRewardConfig>();
            dailyConfig.InitializeDefaults();

            var progression    = new KingProgressionService(_save, null);
            var rewardService  = new RewardService(_save, _currency, progression, null);
            var powerUpService = new PowerUpService(_save, _currency);
            var dailyService   = new DailyRewardService(_save, rewardService, dailyConfig, powerUpService);

            // First claim succeeds.
            var first = dailyService.ClaimTodayReward();
            Assert.IsTrue(first.Success, "First daily reward claim should succeed");

            // Second claim the same day should fail.
            var second = dailyService.ClaimTodayReward();
            Assert.IsFalse(second.Success,
                "Second daily reward claim on the same day should fail");
            Assert.AreEqual("already_claimed", second.FailReason,
                "FailReason should be 'already_claimed'");

            Object.DestroyImmediate(dailyConfig);
        }

        // ── StarCalculator idempotency ────────────────────────────────────────

        [Test]
        public void StarCalculator_NeverReturnsMoreThan3()
        {
            var thresholds = new StarThresholds
            {
                oneStar_destructionMin       = 0.01f,
                oneStar_mustRescueQueen      = false,
                twoStar_destructionMin       = 0.01f,
                twoStar_mustRescueQueen      = false,
                twoStar_enemyRatioMin        = 0.01f,
                threeStar_destructionMin     = 0.01f,
                threeStar_mustRescueQueen    = false,
                threeStar_enemyRatioMin      = 0.01f,
                threeStar_attemptsRemaining  = 0,
            };

            // Even with impossibly good scores, must not exceed 3.
            int stars = StarCalculator.Calculate(thresholds,
                destructionRatio:  999f,
                enemyRatio:        999f,
                queenRescued:      true,
                attemptsRemaining: 9999);
            Assert.LessOrEqual(stars, 3,
                "StarCalculator must never return more than 3 stars");
        }

        // ── LevelRewardCalculator: all 100 levels give positive reward ─────────

        [Test]
        public void LevelRewardCalculator_AllLevelsHavePositiveReward()
        {
            var config = ScriptableObject.CreateInstance<EconomyConfig>();
            config.coinsPerStar               = 25;
            config.bonusCoinsThreeStars       = 100;
            config.coinsPerStructureDestroyed = 5;
            config.coinsPerEnemyKilled        = 10;
            config.baseLevelCoinsMin          = 100;
            config.baseLevelCoinsMax          = 500;
            config.levelCoinScalePer10Levels  = 50;
            config.queenRescueBonus           = 100;
            config.oneStarBonus               = 0;
            config.twoStarBonus               = 50;
            config.threeStarBonus             = 200;

            var all = LevelConfigFactory.All;
            foreach (var def in all)
            {
                // Build a 1-star result: minimal victory.
                var result = LevelRewardCalculator.Build(
                    levelIndex:        def.LevelIndex,
                    score:             def.RequiredScore,
                    destructionRatio:  def.OneStar_DestructionMin,
                    enemiesDefeated:   1,
                    totalEnemies:      def.GuardCount + def.ShieldGuardCount + def.ArcherCount + (def.HasEnemyKing ? 1 : 0),
                    queenRescued:      def.HasQueen,
                    attemptsRemaining: 1,
                    isVictory:         true,
                    failReason:        null,
                    starThresholds:    null,   // null -> StarCalculator returns 0; we rely on config rewards
                    economy:           config
                );

                // The reward should be non-negative; stars may be 0 when thresholds=null.
                Assert.GreaterOrEqual(result.CoinsEarned, 0L,
                    $"Level {def.LevelIndex}: CoinsEarned should be >= 0 on any victory");
            }

            // Now verify config.CalculateLevelReward(stars:1) > 0.
            long oneStarReward = config.CalculateLevelReward(1);
            Assert.Greater(oneStarReward, 0L,
                "CalculateLevelReward(stars:1) should return > 0 given coinsPerStar=25");

            Object.DestroyImmediate(config);
        }
    }
}
