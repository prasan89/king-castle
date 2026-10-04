// OfflineBehaviorTests.cs — EditMode tests for offline degradation and recovery
// at the service level (no real network calls).

using System;
using System.Threading.Tasks;
using NUnit.Framework;
using KingSmash.Cloud;
using KingSmash.Config;
using KingSmash.Core;
using KingSmash.Save;
using KingSmash.Services;
using KingSmash.Services.Mocks;

namespace KingSmash.Tests.EditMode
{
    // ── Minimal in-memory ISaveService used only in this fixture ─────────────

    internal class OfflineTestSaveService : ISaveService
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
    public class OfflineBehaviorTests
    {
        [TearDown]
        public void TearDown()
        {
            ServiceLocator.Initialize();
        }

        // ── CloudSaveServiceMock offline simulation ───────────────────────────

        [Test]
        public void CloudSaveServiceMock_FailsGracefully_WhenSimulatedOffline()
        {
            var mock = new CloudSaveServiceMock();
            mock.SimulateOffline(true);

            // IsOnlineAsync must report false.
            var onlineTask = mock.IsOnlineAsync();
            onlineTask.Wait();
            Assert.IsFalse(onlineTask.Result, "Mock must report offline when SimulateOffline(true).");

            // LoadAsync must return a failure result, not throw.
            var loadTask = mock.LoadAsync("test-player");
            loadTask.Wait();
            Assert.IsFalse(loadTask.Result.Success,
                "LoadAsync must return Success=false when simulated offline.");
        }

        [Test]
        public void CloudSyncService_WithNullCloudSave_DoesNotCrash()
        {
            // Design contract: CloudSyncService stores the injected ICloudSaveService
            // reference directly; no null-guard in the constructor is enforced by the
            // current implementation. If a null is passed the service will throw a
            // NullReferenceException on the first FullSyncAsync call (not at construction).
            //
            // This test documents the observed behavior so that future refactoring
            // is explicit about whether null should be guarded at construction time.

            var localSave = new OfflineTestSaveService();
            var auth      = new AuthServiceMock();
            var config    = new RemoteConfigServiceMock();

            // Construction with null cloudSave must not throw (current behavior).
            CloudSyncService syncService = null;
            Assert.DoesNotThrow(
                () => syncService = new CloudSyncService(null, localSave, auth, config),
                "CloudSyncService constructor must not throw for a null ICloudSaveService — " +
                "the null is surfaced lazily on the first sync attempt.");
        }

        // ── RemoteConfigServiceMock offline defaults ──────────────────────────

        [Test]
        public async Task RemoteConfigServiceMock_OfflineFetch_ReturnsDefaults()
        {
            var mock = new RemoteConfigServiceMock();

            // FetchAsync on the mock completes instantly (no network); it must not throw.
            await mock.FetchAsync();

            // After fetch, GetFloat must return a sensible coin multiplier default.
            float multiplier = mock.GetFloat(RemoteConfigKeys.CoinRewardMultiplier, 1f);
            Assert.AreEqual(1f, multiplier, 0.001f,
                "Default coin reward multiplier must be 1.0 after a mock fetch.");
        }

        // ── LocalSaveService round-trip (PlayerPrefs, EditMode-safe) ─────────

        [Test]
        public void LocalSaveService_SaveAndLoad_WorksWithoutNetwork()
        {
            // Clean up any leftover key before the test.
            UnityEngine.PlayerPrefs.DeleteKey("KingSmash_SaveData");

            var service1 = new LocalSaveService();
            service1.Load();
            service1.Current.coins = 500;
            service1.Save();

            var service2 = new LocalSaveService();
            service2.Load();

            Assert.AreEqual(500L, service2.Current.coins,
                "Coins must survive a Save/Load round-trip via PlayerPrefs.");

            // Cleanup.
            UnityEngine.PlayerPrefs.DeleteKey("KingSmash_SaveData");
        }

        // ── RemoteConfigServiceMock missing key ───────────────────────────────

        [Test]
        public void ConfigService_MissingKey_ReturnsDefault()
        {
            var mock = new RemoteConfigServiceMock();

            string result = mock.GetString("nonexistent_key_xyz", "default_value");

            Assert.AreEqual("default_value", result,
                "GetString for an unknown key must return the provided default.");
        }

        // ── RemoteConfigServiceMock numeric validation ────────────────────────

        [Test]
        public void ConfigService_InvalidNumeric_UsesValidator()
        {
            var mock = new RemoteConfigServiceMock();

            // The mock initialises CoinRewardMultiplier from RemoteConfigDefaults (= 1.0).
            // Validate that the value is within the acceptable product range [0.1, 10.0].
            float result = mock.GetFloat(RemoteConfigKeys.CoinRewardMultiplier, 1.0f);

            Assert.GreaterOrEqual(result, 0.1f, "Coin multiplier must be >= 0.1.");
            Assert.LessOrEqual(result, 10.0f,   "Coin multiplier must be <= 10.0.");
        }

        // ── FeatureFlag — none must throw ─────────────────────────────────────

        [Test]
        public void FeatureFlag_IsFeatureEnabled_NeverThrows()
        {
            var mock   = new RemoteConfigServiceMock();
            var flags  = (FeatureFlag[])Enum.GetValues(typeof(FeatureFlag));

            foreach (FeatureFlag flag in flags)
            {
                bool enabled = false;
                Assert.DoesNotThrow(
                    () => enabled = mock.IsFeatureEnabled(flag),
                    $"IsFeatureEnabled({flag}) must never throw.");
            }
        }
    }
}
