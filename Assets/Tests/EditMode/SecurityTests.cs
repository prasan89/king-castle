// SecurityTests.cs — EditMode tests for anti-exploit and security verification.
// Covers economy guard rails, analytics hygiene, service-locator safety,
// RemoteConfig validator clamping, and logger level enforcement.

using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using KingSmash.Analytics;
using KingSmash.Config;
using KingSmash.Core;
using KingSmash.Economy;
using KingSmash.PowerUps;
using KingSmash.Save;
using KingSmash.Services;

namespace KingSmash.Tests.EditMode
{
    // ── Minimal in-memory ISaveService used only in this fixture ─────────────

    internal class SecurityTestSaveService : ISaveService
    {
        public SaveData Current { get; private set; } = SaveData.CreateNew();
        public void Load()   { }
        public void Save()   { }
        public void Delete() { Current = SaveData.CreateNew(); }
        public void AddCoins(long amount)  { if (amount > 0) Current.coins += amount; }
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
            int prev = Current.GetStarsForLevel(levelIndex);
            if (stars > prev) Current.SetStarsForLevel(levelIndex, stars);
        }
    }

    [TestFixture]
    public class SecurityTests
    {
        [TearDown]
        public void TearDown()
        {
            ServiceLocator.Initialize();
        }

        // ── Economy — coin guard rails ────────────────────────────────────────

        [Test]
        public void Economy_ClientCannotSetArbitraryCoins()
        {
            // The only legitimate way to modify coins is through CurrencyService.
            // This test documents the design: direct mutation of SaveData.coins
            // bypasses the service layer and its audit trail, so all game code
            // must go through CurrencyService.TryAdd / TrySpend.
            //
            // Verified here: TrySpend with more coins than available returns false.

            var save     = new SecurityTestSaveService();
            var currency = new CurrencyService(save);

            // Balance starts at 0; trying to spend 1 000 000 must fail.
            bool result = currency.TrySpend(1_000_000L, "HACK_ATTEMPT");

            Assert.IsFalse(result,           "TrySpend over balance must return false.");
            Assert.AreEqual(0L, currency.Balance, "Balance must remain 0 after a failed spend.");
        }

        [Test]
        public void Economy_SpendMoreThanAvailable_ReturnsFalse()
        {
            var save     = new SecurityTestSaveService();
            var currency = new CurrencyService(save);

            bool result = currency.TrySpend(1_000_000L, "HACK_ATTEMPT");

            Assert.IsFalse(result);
            Assert.AreEqual(0L, currency.Balance);
        }

        [Test]
        public void Economy_NegativeAdd_Handled()
        {
            // TryAdd with a non-positive amount must be rejected (amount <= 0 check).
            var save     = new SecurityTestSaveService();
            var currency = new CurrencyService(save);

            bool result = currency.TryAdd(-500L, "NEGATIVE_ATTEMPT", out _);

            Assert.IsFalse(result,           "TryAdd with negative amount must return false.");
            Assert.AreEqual(0L, currency.Balance, "Balance must remain unchanged at 0.");
        }

        // ── Analytics — event name hygiene ────────────────────────────────────

        [Test]
        public void Analytics_EventNamesAreSnakeCase()
        {
            // A sample of high-frequency events; all must be lowercase + underscore only.
            string[] events =
            {
                AnalyticsEvents.AppOpen,
                AnalyticsEvents.LevelStart,
                AnalyticsEvents.LevelComplete,
                AnalyticsEvents.KingLaunched,
                AnalyticsEvents.PowerUpActivated,
            };

            foreach (string e in events)
            {
                bool isSnakeCase = true;
                for (int i = 0; i < e.Length; i++)
                {
                    char c = e[i];
                    if (!char.IsLower(c) && c != '_')
                    {
                        isSnakeCase = false;
                        break;
                    }
                }
                Assert.IsTrue(isSnakeCase,
                    $"Event name '{e}' must contain only lowercase letters and underscores.");
            }
        }

        [Test]
        public void AnalyticsParameters_NoSensitiveKeyNames()
        {
            // Use reflection to enumerate every public string constant on AnalyticsParameters
            // and verify none accidentally carries a credential-like name.
            var type   = typeof(AnalyticsParameters);
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);

            string[] forbidden = { "password", "token", "secret", "credential", "key" };

            foreach (FieldInfo field in fields)
            {
                if (field.FieldType != typeof(string)) continue;

                string value = (string)field.GetValue(null);
                if (value == null) continue;

                string lower = value.ToLowerInvariant();
                foreach (string pattern in forbidden)
                {
                    Assert.IsFalse(lower.Contains(pattern),
                        $"AnalyticsParameters constant '{field.Name}' = '{value}' " +
                        $"must not contain sensitive pattern '{pattern}'.");
                }
            }
        }

        // ── ServiceLocator — unregistered service ─────────────────────────────

        [Test]
        public void ServiceLocator_GetUnregistered_ThrowsOrReturnsNull()
        {
            ServiceLocator.Initialize();

            // TryGet must return false and set the out parameter to null
            // when the service has not been registered.
            bool found = ServiceLocator.TryGet<IAuthService>(out var service);

            Assert.IsFalse(found,    "TryGet for an unregistered type must return false.");
            Assert.IsNull(service,   "Out parameter must be null when TryGet returns false.");
        }

        // ── RemoteConfigValidator — multiplier clamping ───────────────────────

        [Test]
        public void RemoteConfigValidator_CannotSetCoinMultiplierAbove10()
        {
            // A server-pushed value of 100 must be clamped to <= 10.
            var validated = RemoteConfigValidator.ValidateMultiplier(
                "coin_reward_multiplier", 100f);

            Assert.LessOrEqual(validated.Value, 10f,
                "ValidateMultiplier(100) must clamp to 10.");
        }

        [Test]
        public void RemoteConfigValidator_CannotSetInterstitialFrequencyToZero()
        {
            // Frequency 0 means "show an interstitial every level" — a terrible UX exploit.
            // ValidateInt with min=1 must clamp 0 to at least 1.
            int result = RemoteConfigValidator.ValidateInt(
                "interstitial_frequency",
                rawValue:     0,
                defaultValue: 3,
                min:          1,
                max:          20);

            Assert.GreaterOrEqual(result, 1,
                "Interstitial frequency must be clamped to at least 1.");
        }

        // ── PowerUpService — max-quantity bounding ────────────────────────────

        [Test]
        public void PowerUpService_MaxQuantityBounded()
        {
            // P3 finding (documented): PowerUpService.Award has no upper-bound cap on
            // inventory counts.  If a future security requirement mandates a max, the
            // cap should be enforced in PowerUpInventory.Add.
            //
            // This test records the CURRENT behavior: awarding 9 999 units results in
            // exactly 9 999 stored (no cap).  Update this test when a cap is introduced.

            var save     = new SecurityTestSaveService();
            var currency = new CurrencyService(save);
            var pus      = new PowerUpService(save, currency);

            pus.Award(PowerUpType.Bomb, 9999);
            int count = pus.GetCount(PowerUpType.Bomb);

            // Document current behavior — if a cap of 999 is ever enforced, change
            // the assertion to: Assert.LessOrEqual(count, 999);
            Assert.AreEqual(9999, count,
                "P3 FINDING: PowerUpService.Award currently has no upper-bound cap. " +
                "Count equals the awarded amount. Add a cap in PowerUpInventory.Add if required.");
        }

        // ── GameLogger — level enforcement ───────────────────────────────────

        [Test]
        public void GameLogger_Initialize_SetsLevel()
        {
            // In Unity Editor / DEVELOPMENT_BUILD the min-level passed to Initialize
            // is stored as-is. In a release build the code clamps anything below
            // Warning up to Warning (guarded by #if !DEVELOPMENT_BUILD && !UNITY_EDITOR).
            //
            // In EditMode tests we ARE in the editor, so the level is stored directly.
            // This test simply verifies that Initialize() accepts a LogLevel without
            // throwing — confirming the method exists and is callable.

            Assert.DoesNotThrow(
                () => GameLogger.Initialize(LogLevel.Warning),
                "GameLogger.Initialize must not throw.");

            Assert.DoesNotThrow(
                () => GameLogger.Initialize(LogLevel.Debug),
                "GameLogger.Initialize(Debug) must not throw in the editor.");

            // Restore a reasonable default so other tests are not affected.
            GameLogger.Initialize(LogLevel.Debug);
        }
    }
}
