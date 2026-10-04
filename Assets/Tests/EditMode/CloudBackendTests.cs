using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using KingSmash.Save;
using KingSmash.Services;
using KingSmash.Economy;
using KingSmash.Progression;
using KingSmash.PowerUps;
using KingSmash.Core;

namespace KingSmash.Tests.EditMode
{
    // =========================================================================
    // M11 Internal Mocks
    // =========================================================================

    internal class M11TestSaveService : ISaveService
    {
        public SaveData Current { get; private set; }

        public M11TestSaveService()
        {
            Current = new SaveData
            {
                playerId         = "m11-test",
                kingProgression  = new KingProgression(),
                powerUpInventory = new PowerUpInventory(),
                ownedProducts    = new List<string>(),
                adSessionStats   = new KingSmash.Ads.AdSessionStats(),
                missionProgress     = new List<MissionProgress>(),
                achievementProgress = new List<AchievementProgress>(),
                dailyRewardState = new DailyRewardState(),
                cloudRevision    = 0,
                cloudPlayerId    = "",
            };
        }

        public void Load()  { }
        public void Save()  { }
        public void Delete()
        {
            Current = new SaveData
            {
                playerId         = "m11-test",
                kingProgression  = new KingProgression(),
                powerUpInventory = new PowerUpInventory(),
                ownedProducts    = new List<string>(),
                adSessionStats   = new KingSmash.Ads.AdSessionStats(),
                missionProgress     = new List<MissionProgress>(),
                achievementProgress = new List<AchievementProgress>(),
                dailyRewardState = new DailyRewardState(),
                cloudRevision    = 0,
                cloudPlayerId    = "",
            };
        }

        public void AddCoins(long amount)   { if (amount > 0) Current.coins += amount; }
        public void SpendCoins(long amount) { if (Current.coins < amount) throw new InvalidOperationException("Insufficient coins."); Current.coins -= amount; }
        public bool TrySpendCoins(long amount) { if (Current.coins < amount) return false; SpendCoins(amount); return true; }
        public void AddGems(int amount)     { Current.gems += amount; }
        public void RecordLevelComplete(int levelIndex, int stars)
        {
            if (!Current.completedLevels.Contains(levelIndex)) Current.completedLevels.Add(levelIndex);
            int prev = Current.GetStarsForLevel(levelIndex);
            if (stars > prev) Current.SetStarsForLevel(levelIndex, stars);
        }
    }

    internal class M11TestCloudSaveService : KingSmash.Cloud.ICloudSaveService
    {
        private SaveData _cloudData = new SaveData();
        private bool _isOnline = true;
        private bool _simulateLoadFailure = false;
        private int _savedRevision = 0;

        public void ConfigureCloudData(SaveData data)
        {
            _cloudData = data;
        }

        public void SimulateOffline()
        {
            _isOnline = false;
        }

        public void SimulateLoadFailure()
        {
            _simulateLoadFailure = true;
        }

        public Task<KingSmash.Cloud.CloudLoadResult> LoadAsync(string playerId)
        {
            if (_simulateLoadFailure)
                return Task.FromResult(new KingSmash.Cloud.CloudLoadResult { Success = false, Error = "simulated_load_failure" });

            return Task.FromResult(new KingSmash.Cloud.CloudLoadResult
            {
                Success  = true,
                Data     = _cloudData,
                Revision = _savedRevision,
            });
        }

        public Task<KingSmash.Cloud.CloudSaveResult> SaveAsync(string playerId, KingSmash.Cloud.CloudSavePayload payload)
        {
            _savedRevision++;
            _cloudData = payload.Data;
            return Task.FromResult(new KingSmash.Cloud.CloudSaveResult
            {
                Success     = true,
                NewRevision = _savedRevision,
            });
        }

        public Task<KingSmash.Cloud.CloudSyncResult> SyncAsync(string playerId, SaveData localData)
        {
            return Task.FromResult(new KingSmash.Cloud.CloudSyncResult
            {
                Success  = true,
                HadConflict = false,
                ResolvedData = localData,
            });
        }

        public Task<bool> IsOnlineAsync()
        {
            return Task.FromResult(_isOnline);
        }
    }

    // =========================================================================
    // ConflictResolverTests (7 tests)
    // =========================================================================

    [TestFixture]
    public class ConflictResolverTests
    {
        private static SaveData MakeLocal(Action<SaveData> configure = null)
        {
            var d = new SaveData
            {
                playerId         = "local-player",
                kingProgression  = new KingProgression(),
                powerUpInventory = new PowerUpInventory(),
                missionProgress  = new List<MissionProgress>(),
                achievementProgress = new List<AchievementProgress>(),
                dailyRewardState = new DailyRewardState(),
                adSessionStats   = new KingSmash.Ads.AdSessionStats(),
            };
            configure?.Invoke(d);
            return d;
        }

        private static SaveData MakeCloud(Action<SaveData> configure = null)
        {
            var d = new SaveData
            {
                playerId         = "cloud-player",
                kingProgression  = new KingProgression(),
                powerUpInventory = new PowerUpInventory(),
                missionProgress  = new List<MissionProgress>(),
                achievementProgress = new List<AchievementProgress>(),
                dailyRewardState = new DailyRewardState(),
                adSessionStats   = new KingSmash.Ads.AdSessionStats(),
            };
            configure?.Invoke(d);
            return d;
        }

        [Test]
        public void Resolve_CurrentLevel_TakesMax()
        {
            var local = MakeLocal(d => d.currentLevel = 5);
            var cloud = MakeCloud(d => d.currentLevel = 10);

            var result = KingSmash.Cloud.ConflictResolver.Resolve(local, cloud);

            Assert.AreEqual(10, result.currentLevel);
        }

        [Test]
        public void Resolve_CompletedLevels_TakesUnion()
        {
            var local = MakeLocal(d => d.completedLevels = new List<int> { 1, 2, 3 });
            var cloud = MakeCloud(d => d.completedLevels = new List<int> { 2, 3, 4, 5 });

            var result = KingSmash.Cloud.ConflictResolver.Resolve(local, cloud);

            Assert.IsTrue(result.completedLevels.Contains(1));
            Assert.IsTrue(result.completedLevels.Contains(2));
            Assert.IsTrue(result.completedLevels.Contains(3));
            Assert.IsTrue(result.completedLevels.Contains(4));
            Assert.IsTrue(result.completedLevels.Contains(5));
        }

        [Test]
        public void Resolve_StarsPerLevel_TakesPerLevelMax()
        {
            var local = MakeLocal(d =>
            {
                d.starsPerLevel = new List<LevelStarEntry>
                {
                    new LevelStarEntry { levelIndex = 0, stars = 3 },
                    new LevelStarEntry { levelIndex = 1, stars = 1 },
                };
            });
            var cloud = MakeCloud(d =>
            {
                d.starsPerLevel = new List<LevelStarEntry>
                {
                    new LevelStarEntry { levelIndex = 0, stars = 2 },
                    new LevelStarEntry { levelIndex = 1, stars = 3 },
                };
            });

            var result = KingSmash.Cloud.ConflictResolver.Resolve(local, cloud);

            Assert.AreEqual(3, result.GetStarsForLevel(0), "Level 0: local 3 > cloud 2");
            Assert.AreEqual(3, result.GetStarsForLevel(1), "Level 1: cloud 3 > local 1");
        }

        [Test]
        public void Resolve_CoinsAndGems_CloudWins()
        {
            var local = MakeLocal(d => { d.coins = 9999; d.gems = 50; });
            var cloud = MakeCloud(d => { d.coins = 100;  d.gems = 10; });

            var result = KingSmash.Cloud.ConflictResolver.Resolve(local, cloud);

            Assert.AreEqual(100L, result.coins, "coins: cloud wins");
            Assert.AreEqual(10,   result.gems,  "gems: cloud wins");
        }

        [Test]
        public void Resolve_KingProgression_TakesMaxPerField()
        {
            var local = MakeLocal(d =>
            {
                d.kingProgression = new KingProgression
                { KingLevel = 5, PowerLevel = 3, SpeedLevel = 7, SmashRadiusLevel = 2, ArmorLevel = 4 };
            });
            var cloud = MakeCloud(d =>
            {
                d.kingProgression = new KingProgression
                { KingLevel = 8, PowerLevel = 1, SpeedLevel = 5, SmashRadiusLevel = 9, ArmorLevel = 4 };
            });

            var result = KingSmash.Cloud.ConflictResolver.Resolve(local, cloud);

            Assert.AreEqual(8, result.kingProgression.KingLevel,        "KingLevel: max");
            Assert.AreEqual(3, result.kingProgression.PowerLevel,       "PowerLevel: max");
            Assert.AreEqual(7, result.kingProgression.SpeedLevel,       "SpeedLevel: max");
            Assert.AreEqual(9, result.kingProgression.SmashRadiusLevel, "SmashRadiusLevel: max");
            Assert.AreEqual(4, result.kingProgression.ArmorLevel,       "ArmorLevel: equal");
        }

        [Test]
        public void Resolve_AdSessionStats_LocalWins()
        {
            var local = MakeLocal(d =>
            {
                d.adSessionStats = new KingSmash.Ads.AdSessionStats
                { interstitialsShownThisSession = 7, totalInterstitialsShown = 42 };
            });
            var cloud = MakeCloud(d =>
            {
                d.adSessionStats = new KingSmash.Ads.AdSessionStats
                { interstitialsShownThisSession = 0, totalInterstitialsShown = 0 };
            });

            var result = KingSmash.Cloud.ConflictResolver.Resolve(local, cloud);

            Assert.AreEqual(7,  result.adSessionStats.interstitialsShownThisSession, "adSessionStats: local wins");
            Assert.AreEqual(42, result.adSessionStats.totalInterstitialsShown,        "adSessionStats: local wins");
        }

        [Test]
        public void Resolve_DailyRewardState_CloudWins()
        {
            long cloudTs = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long localTs = cloudTs - 86400;

            var local = MakeLocal(d =>
            {
                d.dailyRewardState = new DailyRewardState
                { currentStreakDay = 2, lastClaimedTimestamp = localTs, claimedToday = false };
            });
            var cloud = MakeCloud(d =>
            {
                d.dailyRewardState = new DailyRewardState
                { currentStreakDay = 5, lastClaimedTimestamp = cloudTs, claimedToday = true };
            });

            var result = KingSmash.Cloud.ConflictResolver.Resolve(local, cloud);

            Assert.AreEqual(5,       result.dailyRewardState.currentStreakDay,      "dailyRewardState: cloud wins");
            Assert.AreEqual(cloudTs, result.dailyRewardState.lastClaimedTimestamp,  "dailyRewardState: cloud wins");
            Assert.IsTrue(result.dailyRewardState.claimedToday,                     "dailyRewardState: cloud wins");
        }
    }

    // =========================================================================
    // SaveDataV7Tests (4 tests)
    // =========================================================================

    [TestFixture]
    public class SaveDataV7Tests
    {
        [Test]
        public void CurrentVersion_Is7()
        {
            Assert.AreEqual(7, SaveData.CurrentVersion);
        }

        [Test]
        public void SaveData_CloudRevision_DefaultsToZero()
        {
            var data = new SaveData();
            Assert.AreEqual(0, data.cloudRevision);
        }

        [Test]
        public void SaveData_LastSyncTimestamp_DefaultsToZero()
        {
            var data = new SaveData();
            Assert.AreEqual(0L, data.lastSyncTimestamp);
        }

        [Test]
        public void SaveData_CloudPlayerId_DefaultsToEmpty()
        {
            var data = new SaveData();
            Assert.AreEqual("", data.cloudPlayerId);
        }
    }

    // =========================================================================
    // AuthUserTests (3 tests)
    // =========================================================================

    [TestFixture]
    public class AuthUserTests
    {
        [Test]
        public void AuthUser_Anonymous_IsAnonymousTrue()
        {
            var user = new AuthUser
            {
                UserId   = "anon-uid-123",
                Provider = AuthProvider.Anonymous,
            };

            Assert.IsTrue(user.IsAnonymous);
        }

        [Test]
        public void AuthUser_Google_IsAnonymousFalse()
        {
            var user = new AuthUser
            {
                UserId   = "google-uid-456",
                Email    = "player@example.com",
                Provider = AuthProvider.Google,
            };

            Assert.IsFalse(user.IsAnonymous);
        }

        [Test]
        public void AuthServiceMock_SignInAnonymously_SetsAnonymousProvider()
        {
            var mock = new AuthServiceMock();
            var task = mock.SignInAnonymouslyAsync();
            task.Wait();

            Assert.IsNotNull(mock.CurrentUser);
            Assert.AreEqual(AuthProvider.Anonymous, mock.CurrentUser.Provider);
            Assert.IsTrue(mock.CurrentUser.IsAnonymous);
        }
    }

    // =========================================================================
    // CloudSyncServiceTests (5 tests)
    // =========================================================================

    [TestFixture]
    public class CloudSyncServiceTests
    {
        private M11TestSaveService       _localSave;
        private M11TestCloudSaveService  _cloudSave;
        private AuthServiceMock          _auth;
        private RemoteConfigServiceMock  _config;
        private KingSmash.Cloud.CloudSyncService _syncService;

        [SetUp]
        public void SetUp()
        {
            ServiceLocator.Initialize();

            _localSave = new M11TestSaveService();
            _cloudSave = new M11TestCloudSaveService();
            _auth      = new AuthServiceMock();
            _config    = new RemoteConfigServiceMock();

            // Sign in so sync has a valid user
            _auth.SignInAnonymouslyAsync().Wait();

            _syncService = new KingSmash.Cloud.CloudSyncService(_cloudSave, _localSave, _auth, _config);
        }

        [TearDown]
        public void TearDown()
        {
            ServiceLocator.Initialize();
        }

        [Test]
        public async Task FullSync_WhenOnline_ReturnsSuccess()
        {
            var result = await _syncService.FullSyncAsync();

            Assert.IsTrue(result.Success);
        }

        [Test]
        public async Task FullSync_WhenOffline_ReturnsOfflineResult()
        {
            _cloudSave.SimulateOffline();

            var result = await _syncService.FullSyncAsync();

            Assert.IsFalse(result.Success);
        }

        [Test]
        public async Task FullSync_LoadFailure_ReturnsFailedResult()
        {
            _cloudSave.SimulateLoadFailure();

            var result = await _syncService.FullSyncAsync();

            Assert.IsFalse(result.Success);
        }

        [Test]
        public async Task FullSync_NoConflict_HadConflictFalse()
        {
            // Cloud and local are identical → no conflict
            var result = await _syncService.FullSyncAsync();

            Assert.IsTrue(result.Success);
            Assert.IsFalse(result.HadConflict);
        }

        [Test]
        public async Task FullSync_StatusChangedEvent_FiredInOrder()
        {
            var statuses = new List<KingSmash.Cloud.SyncStatus>();
            KingSmash.Cloud.CloudSyncService.OnSyncStatusChanged += s => statuses.Add(s);

            try
            {
                await _syncService.FullSyncAsync();

                Assert.Greater(statuses.Count, 0, "At least one SyncStatus event must fire");
                Assert.AreEqual(KingSmash.Cloud.SyncStatus.Synced, statuses[statuses.Count - 1],
                    "Last status must be Synced on success");
            }
            finally
            {
                KingSmash.Cloud.CloudSyncService.OnSyncStatusChanged -= s => statuses.Add(s);
            }
        }
    }

    // =========================================================================
    // EconomyTransactionTests (4 tests — in-memory only, no real Firestore)
    // =========================================================================

    [TestFixture]
    public class EconomyTransactionTests
    {
        private M11TestSaveService _save;
        private CurrencyService    _currency;

        [SetUp]
        public void SetUp()
        {
            ServiceLocator.Initialize();
            _save     = new M11TestSaveService();
            _currency = new CurrencyService(_save);
        }

        [TearDown]
        public void TearDown()
        {
            ServiceLocator.Initialize();
        }

        [Test]
        public void TryAdd_PositiveAmount_IncreasesBalance()
        {
            long before = _currency.Balance;

            bool ok = _currency.TryAdd(500, "test_reward", out var tx);

            Assert.IsTrue(ok);
            Assert.IsNotNull(tx);
            Assert.AreEqual(before + 500, _currency.Balance);
            Assert.AreEqual("add", tx.type);
            Assert.AreEqual(500L, tx.amount);
        }

        [Test]
        public void TrySpend_SufficientFunds_DecreasesBalance()
        {
            _currency.TryAdd(1000, "setup", out _);
            long before = _currency.Balance;

            bool ok = _currency.TrySpend(200, "shop_purchase", out var tx);

            Assert.IsTrue(ok);
            Assert.IsNotNull(tx);
            Assert.AreEqual(before - 200, _currency.Balance);
            Assert.AreEqual("spend", tx.type);
        }

        [Test]
        public void TrySpend_InsufficientFunds_ReturnsFalse()
        {
            // Balance is 0 by default
            bool ok = _currency.TrySpend(999, "shop_purchase", out var tx);

            Assert.IsFalse(ok);
            Assert.IsNull(tx);
            Assert.AreEqual(0L, _currency.Balance);
        }

        [Test]
        public void GetHistory_AfterMultipleTransactions_ReturnsAll()
        {
            _currency.TryAdd(100, "r1", out _);
            _currency.TryAdd(200, "r2", out _);
            _currency.TrySpend(50, "s1", out _);

            var history = _currency.GetHistory();

            Assert.AreEqual(3, history.Length);
            Assert.AreEqual("add",   history[0].type);
            Assert.AreEqual("add",   history[1].type);
            Assert.AreEqual("spend", history[2].type);
        }
    }
}
