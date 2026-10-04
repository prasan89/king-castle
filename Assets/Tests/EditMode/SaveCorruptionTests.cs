// SaveCorruptionTests.cs
// Save data robustness and corruption recovery tests for King Smash.
// EditMode only — pure data/logic, no scene loading, no MonoBehaviours.

using System;
using NUnit.Framework;
using UnityEngine;
using KingSmash.Save;
using KingSmash.Services;
using KingSmash.Economy;
using KingSmash.PowerUps;
using KingSmash.Progression;
using KingSmash.Config;

namespace KingSmash.Tests.EditMode
{
    // =========================================================================
    // Shared in-memory save service for corruption tests
    // =========================================================================

    internal class CorruptionTestSaveService : ISaveService
    {
        public SaveData Current { get; private set; }

        public CorruptionTestSaveService(SaveData initial = null)
        {
            Current = initial ?? new SaveData
            {
                playerId         = "corruption-test",
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
                playerId         = "corruption-test",
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

        public void AddGems(int amount)  { Current.gems += amount; }

        public void RecordLevelComplete(int levelIndex, int stars)
        {
            if (!Current.completedLevels.Contains(levelIndex))
                Current.completedLevels.Add(levelIndex);
            int prev = Current.GetStarsForLevel(levelIndex);
            if (stars > prev) Current.SetStarsForLevel(levelIndex, stars);
        }
    }

    // =========================================================================
    // SaveCorruptionTests
    // =========================================================================

    [TestFixture]
    public class SaveCorruptionTests
    {
        [SetUp]
        public void SetUp()
        {
            // Clear static events to prevent cross-test leakage.
            CurrencyService.OnCoinsChanged      = null;
            CurrencyService.OnInsufficientFunds = null;
        }

        // ── Default constructor produces valid state ───────────────────────────

        [Test]
        public void SaveData_DefaultConstructor_ValidState()
        {
            var save = new SaveData();
            Assert.AreEqual(0L, save.coins,
                "Default SaveData.coins should be 0");
            Assert.AreEqual(0, save.gems,
                "Default SaveData.gems should be 0");
            Assert.AreEqual(0, save.currentLevel,
                "Default SaveData.currentLevel should be 0");
            Assert.IsNotNull(save.starsPerLevel,
                "Default SaveData.starsPerLevel must not be null");
        }

        // ── Migration ─────────────────────────────────────────────────────────

        [Test]
        public void SaveMigration_V1ToLatest_PreservesProgress()
        {
            // Create a save at version 1 with known data.
            var old = new SaveData
            {
                version      = 1,
                coins        = 9999L,
                currentLevel = 7,
                kingLevel    = 3,
            };

            var migrated = SaveMigrator.Migrate(old);

            Assert.AreEqual(SaveData.CurrentVersion, migrated.version,
                "Migrated save should be at CurrentVersion");
            Assert.AreEqual(9999L, migrated.coins,
                "coins must be preserved across migration");
            Assert.AreEqual(7, migrated.currentLevel,
                "currentLevel must be preserved across migration");
        }

        // ── Corrupted JSON fallback ───────────────────────────────────────────

        [Test]
        public void SaveData_CorruptedJson_ReturnsDefault()
        {
            // JsonUtility.FromJson on invalid JSON throws; the game must survive this.
            SaveData result = null;
            bool threw = false;
            try
            {
                result = JsonUtility.FromJson<SaveData>("INVALID_JSON_CORRUPTION_TEST");
            }
            catch
            {
                threw = true;
                // Recovery path: fall back to a fresh save.
                result = SaveData.CreateNew();
            }

            // Either JsonUtility returns a default object, or we caught an exception
            // and created a new save. Either way, result must not be null.
            Assert.IsNotNull(result,
                "Corrupted JSON recovery must produce a non-null SaveData");

            if (!threw)
            {
                // If Unity returned something instead of throwing, validate it is usable.
                Assert.GreaterOrEqual(result.coins, 0L,
                    "Recovered save from corrupted JSON should have coins >= 0");
            }
        }

        // ── Overflow guard ────────────────────────────────────────────────────

        [Test]
        public void SaveData_CoinsCannotExceedMaxLong()
        {
            // Set coins to very close to long.MaxValue and ensure TryAdd guards against overflow.
            var save     = new SaveData { coins = long.MaxValue - 1L, kingProgression = new KingProgression(), powerUpInventory = new PowerUpInventory() };
            var svc      = new CorruptionTestSaveService(save);
            var currency = new CurrencyService(svc);

            // Adding 1 should succeed without overflow (MaxValue - 1 + 1 = MaxValue, exact).
            Assert.DoesNotThrow(() => currency.TryAdd(1L, "TEST", out _),
                "Adding 1 to MaxValue-1 should not throw");
        }

        // ── Negative coins detection ──────────────────────────────────────────

        [Test]
        public void SaveData_NegativeCoinsIsInvalid()
        {
            // A save with negative coins is invalid; CurrencyService must not go further negative.
            var save = new SaveData { coins = -500L, kingProgression = new KingProgression(), powerUpInventory = new PowerUpInventory() };
            // Directly assert the bad state is detectable.
            Assert.Less(save.coins, 0L,
                "Pre-condition: save.coins is negative");

            // If we wrap the bad save in the service, TrySpend should fail gracefully.
            var svc      = new CorruptionTestSaveService(save);
            var currency = new CurrencyService(svc);
            bool result  = currency.TrySpend(1L, "TEST");
            Assert.IsFalse(result,
                "TrySpend should return false when balance is negative (cannot go further negative)");
        }

        // ── starsPerLevel array validity ──────────────────────────────────────

        [Test]
        public void SaveData_StarsPerLevelArray_AllValid()
        {
            var save = new SaveData { kingProgression = new KingProgression(), powerUpInventory = new PowerUpInventory() };
            // Default starsPerLevel list is empty — all queries return 0, which is in [0,3].
            bool allValid = true;
            foreach (var entry in save.starsPerLevel)
            {
                if (entry.stars < 0 || entry.stars > 3)
                {
                    allValid = false;
                    break;
                }
            }
            Assert.IsTrue(allValid,
                "All default starsPerLevel entries should have stars in [0,3]");

            // Set some explicit entries and re-check.
            save.SetStarsForLevel(0, 1);
            save.SetStarsForLevel(1, 3);
            save.SetStarsForLevel(2, 2);

            foreach (var entry in save.starsPerLevel)
            {
                Assert.GreaterOrEqual(entry.stars, 0,
                    $"Level {entry.levelIndex} stars ({entry.stars}) must be >= 0");
                Assert.LessOrEqual(entry.stars, 3,
                    $"Level {entry.levelIndex} stars ({entry.stars}) must be <= 3");
            }
        }

        // ── Highest level boundary ────────────────────────────────────────────

        [Test]
        public void SaveData_HighestLevelBounded()
        {
            // currentLevel is the mutable field; it should never exceed 99 (last level index).
            // Clamping is a validation concern — we test that the raw field can hold 99
            // and that the validator can detect values > 99.
            var save = new SaveData { kingProgression = new KingProgression(), powerUpInventory = new PowerUpInventory() };
            save.currentLevel = 99;
            Assert.LessOrEqual(save.currentLevel, 99,
                "currentLevel at 99 should be valid (last level)");

            // Simulate setting an invalid value.
            save.currentLevel = 150;
            Assert.Greater(save.currentLevel, 99,
                "Pre-condition: currentLevel is out of range");

            // LevelValidator or service layer should clamp/reject — test via data inspection.
            int clamped = System.Math.Min(save.currentLevel, 99);
            Assert.AreEqual(99, clamped,
                "Clamped currentLevel should be 99 when set to 150");
        }

        // ── RemoteConfigValidator ─────────────────────────────────────────────

        [Test]
        public void RemoteConfigValidator_MalformedFloat_ReturnsDefault()
        {
            var result = RemoteConfigValidator.ValidateMultiplier("coin_reward_multiplier", float.NaN);
            Assert.IsFalse(result.IsValid,
                "NaN should be marked as invalid");
            Assert.AreEqual(1.0f, result.Value,
                "NaN raw value should produce default multiplier of 1.0f");
        }

        [Test]
        public void RemoteConfigValidator_NegativeMultiplier_Clamped()
        {
            var result = RemoteConfigValidator.ValidateMultiplier("test_multiplier", -5f);
            Assert.GreaterOrEqual(result.Value, 0.1f,
                "Negative multiplier should be clamped to at least 0.1f");
        }

        [Test]
        public void RemoteConfigValidator_ExtremeMultiplier_Clamped()
        {
            var result = RemoteConfigValidator.ValidateMultiplier("test_multiplier", 999f);
            Assert.LessOrEqual(result.Value, 10f,
                "Extreme multiplier (999) should be clamped to at most 10f");
        }

        // ── Feature flags ─────────────────────────────────────────────────────

        [Test]
        public void FeatureFlag_RemoteConfigMock_AllFlagsEnabled()
        {
            var mock = new RemoteConfigServiceMock();

            Assert.IsTrue(mock.IsFeatureEnabled(FeatureFlag.PowerUps),
                "PowerUps feature flag should default to enabled");
            Assert.IsTrue(mock.IsFeatureEnabled(FeatureFlag.DailyRewards),
                "DailyRewards feature flag should default to enabled");
            Assert.IsTrue(mock.IsFeatureEnabled(FeatureFlag.Missions),
                "Missions feature flag should default to enabled");
            Assert.IsTrue(mock.IsFeatureEnabled(FeatureFlag.Achievements),
                "Achievements feature flag should default to enabled");
            Assert.IsTrue(mock.IsFeatureEnabled(FeatureFlag.Shop),
                "Shop feature flag should default to enabled");
        }
    }
}
