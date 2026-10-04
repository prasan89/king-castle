using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using KingSmash.Retention;
using KingSmash.Economy;
using KingSmash.Services;
using KingSmash.Save;
using KingSmash.PowerUps;
using KingSmash.Core;
using KingSmash.Progression;

namespace KingSmash.Tests.EditMode
{
    internal class M10TestSaveService : ISaveService
    {
        public SaveData Current { get; private set; }

        public M10TestSaveService()
        {
            Current = new SaveData
            {
                playerId         = "m10-test",
                kingProgression  = new KingProgression(),
                powerUpInventory = new PowerUpInventory(),
                ownedProducts    = new List<string>(),
                adSessionStats   = new AdSessionStats(),
                missionProgress     = new List<MissionProgress>(),
                achievementProgress = new List<AchievementProgress>(),
                dailyRewardState = new DailyRewardState(),
            };
        }

        public void Load()  { }
        public void Save()  { }
        public void Delete()
        {
            Current = new SaveData
            {
                playerId         = "m10-test",
                kingProgression  = new KingProgression(),
                powerUpInventory = new PowerUpInventory(),
                ownedProducts    = new List<string>(),
                adSessionStats   = new AdSessionStats(),
                missionProgress     = new List<MissionProgress>(),
                achievementProgress = new List<AchievementProgress>(),
                dailyRewardState = new DailyRewardState(),
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

    // =========================================================================
    // TimeServiceTests
    // =========================================================================

    [TestFixture]
    public class TimeServiceTests
    {
        [Test]
        public void IsSameUtcDay_SameTimestamps_ReturnsTrue()
        {
            long now = TimeService.UtcNow;
            Assert.IsTrue(TimeService.IsSameUtcDay(now, now));
        }

        [Test]
        public void IsSameUtcDay_DifferentDays_ReturnsFalse()
        {
            long today     = TimeService.UtcNow;
            long yesterday = DateTimeOffset.UtcNow.AddDays(-1).ToUnixTimeSeconds();
            Assert.IsFalse(TimeService.IsSameUtcDay(today, yesterday));
        }

        [Test]
        public void IsPreviousUtcDay_Yesterday_ReturnsTrue()
        {
            long today     = TimeService.UtcNow;
            long yesterday = DateTimeOffset.UtcNow.AddDays(-1).ToUnixTimeSeconds();
            Assert.IsTrue(TimeService.IsPreviousUtcDay(yesterday, today));
        }

        [Test]
        public void StartOfUtcDay_ReturnsFlooredToMidnight()
        {
            long now   = TimeService.UtcNow;
            long start = TimeService.StartOfUtcDay(now);
            var  dto   = DateTimeOffset.FromUnixTimeSeconds(start).ToUniversalTime();
            Assert.AreEqual(0, dto.Hour);
            Assert.AreEqual(0, dto.Minute);
            Assert.AreEqual(0, dto.Second);
        }
    }

    // =========================================================================
    // DailyRewardConfigTests
    // =========================================================================

    [TestFixture]
    public class DailyRewardConfigTests
    {
        private DailyRewardConfig _config;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<DailyRewardConfig>();
            _config.InitializeDefaults();
        }

        [TearDown]
        public void TearDown() => UnityEngine.Object.DestroyImmediate(_config);

        [Test]
        public void DefaultValues_7Entries()
        {
            Assert.AreEqual(7, _config.CycleLength);
        }

        [Test]
        public void GetEntry_Day1_Returns100Coins()
        {
            var entry = _config.GetEntry(1);
            Assert.AreEqual(1, entry.day);
            Assert.AreEqual(100L, entry.coinsReward);
        }

        [Test]
        public void GetEntry_Day7_IsTreasureChest()
        {
            var entry = _config.GetEntry(7);
            Assert.IsTrue(entry.isTreasureChest);
        }

        [Test]
        public void GetEntry_WrapsAround()
        {
            var day8 = _config.GetEntry(8);
            var day1 = _config.GetEntry(1);
            Assert.AreEqual(day1.day, day8.day);
            Assert.AreEqual(day1.coinsReward, day8.coinsReward);
        }

        [Test]
        public void CycleLength_Is7()
        {
            Assert.AreEqual(7, _config.CycleLength);
        }
    }

    // =========================================================================
    // DailyRewardServiceTests
    // =========================================================================

    [TestFixture]
    public class DailyRewardServiceTests
    {
        private M10TestSaveService  _save;
        private CurrencyService     _currency;
        private KingProgressionService _progression;
        private RewardService       _rewardService;
        private PowerUpService      _powerUpService;
        private DailyRewardConfig   _config;
        private DailyRewardService  _dailyService;

        [SetUp]
        public void SetUp()
        {
            ServiceLocator.Initialize();

            _save           = new M10TestSaveService();
            _currency       = new CurrencyService(_save);
            _progression    = new KingProgressionService(_save, null);
            _rewardService  = new RewardService(_save, _currency, _progression, null);
            _powerUpService = new PowerUpService(_save, _currency);
            _config         = ScriptableObject.CreateInstance<DailyRewardConfig>();
            _config.InitializeDefaults();

            _dailyService = new DailyRewardService(_save, _rewardService, _config, _powerUpService);
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(_config);
            ServiceLocator.Initialize();
        }

        [Test]
        public void CanClaimToday_FirstTime_ReturnsTrue()
        {
            Assert.IsTrue(_dailyService.CanClaimToday());
        }

        [Test]
        public void CanClaimToday_AlreadyClaimed_SameDay_ReturnsFalse()
        {
            _dailyService.ClaimTodayReward();
            Assert.IsFalse(_dailyService.CanClaimToday());
        }

        [Test]
        public void CanClaimToday_NewDay_AfterPreviousClaim_ReturnsTrue()
        {
            _dailyService.ClaimTodayReward();
            _save.Current.dailyRewardState.lastClaimedTimestamp =
                DateTimeOffset.UtcNow.AddDays(-1).ToUnixTimeSeconds();
            Assert.IsTrue(_dailyService.CanClaimToday());
        }

        [Test]
        public void ClaimTodayReward_GrantsCoins()
        {
            long coinsBefore = _save.Current.coins;
            var result = _dailyService.ClaimTodayReward();
            Assert.IsTrue(result.Success);
            Assert.Greater(_save.Current.coins, coinsBefore);
        }

        [Test]
        public void ClaimTodayReward_AdvancesDay()
        {
            int dayBefore = _dailyService.CurrentDay;
            _dailyService.ClaimTodayReward();
            _save.Current.dailyRewardState.lastClaimedTimestamp =
                DateTimeOffset.UtcNow.AddDays(-1).ToUnixTimeSeconds();
            int dayAfter = _dailyService.CurrentDay;
            Assert.Greater(dayAfter, dayBefore);
        }

        [Test]
        public void ClaimTodayReward_Day7_WrapsToDay1()
        {
            _save.Current.dailyRewardState.currentStreakDay = 7;
            var result = _dailyService.ClaimTodayReward();
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, _save.Current.dailyRewardState.currentStreakDay);
        }

        [Test]
        public void ClaimTodayReward_DoubleClaim_ReturnsFail()
        {
            _dailyService.ClaimTodayReward();
            var second = _dailyService.ClaimTodayReward();
            Assert.IsFalse(second.Success);
            Assert.AreEqual("already_claimed", second.FailReason);
        }
    }

    // =========================================================================
    // MissionServiceTests
    // =========================================================================

    [TestFixture]
    public class MissionServiceTests
    {
        private M10TestSaveService  _save;
        private CurrencyService     _currency;
        private KingProgressionService _progression;
        private RewardService       _rewardService;
        private PowerUpService      _powerUpService;
        private MissionConfig       _missionConfig;
        private AchievementConfig   _achConfig;
        private MissionService      _missionService;

        [SetUp]
        public void SetUp()
        {
            ServiceLocator.Initialize();

            _save           = new M10TestSaveService();
            _currency       = new CurrencyService(_save);
            _progression    = new KingProgressionService(_save, null);
            _rewardService  = new RewardService(_save, _currency, _progression, null);
            _powerUpService = new PowerUpService(_save, _currency);

            _missionConfig = ScriptableObject.CreateInstance<MissionConfig>();
            _missionConfig.InitializeDefaults();

            _achConfig = ScriptableObject.CreateInstance<AchievementConfig>();
            _achConfig.InitializeDefaults();

            _missionService = new MissionService(
                _save, _missionConfig, _achConfig, _rewardService, _powerUpService, _currency);
            _missionService.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            _missionService.Dispose();
            UnityEngine.Object.DestroyImmediate(_missionConfig);
            UnityEngine.Object.DestroyImmediate(_achConfig);
            ServiceLocator.Initialize();
        }

        [Test]
        public void GetMissionProgress_NewMission_ReturnsZero()
        {
            var progress = _missionService.GetMissionProgress("play_5_levels");
            Assert.AreEqual(0, progress.progress);
        }

        [Test]
        public void IncrementMissionProgress_LEVELS_PLAYED_Increments()
        {
            _missionService.IncrementMissionProgress(MissionType.LEVELS_PLAYED, 1);
            var progress = _missionService.GetMissionProgress("play_5_levels");
            Assert.AreEqual(1, progress.progress);
        }

        [Test]
        public void CanClaimMission_NotComplete_ReturnsFalse()
        {
            Assert.IsFalse(_missionService.CanClaimMission("play_5_levels"));
        }

        [Test]
        public void CanClaimMission_Complete_ReturnsTrue()
        {
            var def = _missionConfig.GetMission("play_5_levels");
            _save.Current.GetOrCreateMissionProgress("play_5_levels").progress = def.target;
            Assert.IsTrue(_missionService.CanClaimMission("play_5_levels"));
        }

        [Test]
        public void ClaimMission_GrantsReward()
        {
            var def = _missionConfig.GetMission("play_5_levels");
            _save.Current.GetOrCreateMissionProgress("play_5_levels").progress = def.target;
            long coinsBefore = _save.Current.coins;
            var result = _missionService.ClaimMission("play_5_levels");
            Assert.IsTrue(result.Success);
            Assert.Greater(_save.Current.coins, coinsBefore);
        }

        [Test]
        public void ClaimMission_DoubleClaimPrevented()
        {
            var def = _missionConfig.GetMission("play_5_levels");
            _save.Current.GetOrCreateMissionProgress("play_5_levels").progress = def.target;
            _missionService.ClaimMission("play_5_levels");
            var second = _missionService.ClaimMission("play_5_levels");
            Assert.IsFalse(second.Success);
        }

        [Test]
        public void ResetDailyMissions_ResetsProgress()
        {
            var mp = _save.Current.GetOrCreateMissionProgress("play_5_levels");
            mp.progress = 3;
            mp.lastResetTimestamp = DateTimeOffset.UtcNow.AddDays(-1).ToUnixTimeSeconds();
            _missionService.ResetDailyMissions();
            Assert.AreEqual(0, _missionService.GetMissionProgress("play_5_levels").progress);
        }

        [Test]
        public void ResetDailyMissions_SameDay_DoesNotReset()
        {
            var mp = _save.Current.GetOrCreateMissionProgress("play_5_levels");
            mp.progress = 3;
            mp.lastResetTimestamp = TimeService.UtcNow;
            _missionService.ResetDailyMissions();
            Assert.AreEqual(3, _missionService.GetMissionProgress("play_5_levels").progress);
        }
    }

    // =========================================================================
    // AchievementTests
    // =========================================================================

    [TestFixture]
    public class AchievementTests
    {
        private M10TestSaveService  _save;
        private CurrencyService     _currency;
        private KingProgressionService _progression;
        private RewardService       _rewardService;
        private PowerUpService      _powerUpService;
        private MissionConfig       _missionConfig;
        private AchievementConfig   _achConfig;
        private MissionService      _missionService;

        [SetUp]
        public void SetUp()
        {
            ServiceLocator.Initialize();

            _save           = new M10TestSaveService();
            _currency       = new CurrencyService(_save);
            _progression    = new KingProgressionService(_save, null);
            _rewardService  = new RewardService(_save, _currency, _progression, null);
            _powerUpService = new PowerUpService(_save, _currency);

            _missionConfig = ScriptableObject.CreateInstance<MissionConfig>();
            _missionConfig.InitializeDefaults();

            _achConfig = ScriptableObject.CreateInstance<AchievementConfig>();
            _achConfig.InitializeDefaults();

            _missionService = new MissionService(
                _save, _missionConfig, _achConfig, _rewardService, _powerUpService, _currency);
            _missionService.Initialize();
        }

        [TearDown]
        public void TearDown()
        {
            _missionService.Dispose();
            UnityEngine.Object.DestroyImmediate(_missionConfig);
            UnityEngine.Object.DestroyImmediate(_achConfig);
            ServiceLocator.Initialize();
        }

        [Test]
        public void GetAchievementProgress_New_ReturnsZero()
        {
            var progress = _missionService.GetAchievementProgress("first_smash");
            Assert.AreEqual(0, progress.progress);
        }

        [Test]
        public void IncrementProgress_LEVELS_COMPLETED_Increments()
        {
            _missionService.IncrementMissionProgress(MissionType.LEVELS_COMPLETED, 1);
            var progress = _missionService.GetAchievementProgress("level_milestones");
            Assert.AreEqual(1, progress.progress);
        }

        [Test]
        public void ClaimAchievementTier_FirstTier_Success()
        {
            var ap = _save.Current.GetOrCreateAchievementProgress("first_smash");
            ap.progress = 1;
            var result = _missionService.ClaimAchievementTier("first_smash");
            Assert.IsTrue(result.Success);
        }

        [Test]
        public void ClaimAchievementTier_TierNotReached_Fails()
        {
            var result = _missionService.ClaimAchievementTier("first_smash");
            Assert.IsFalse(result.Success);
        }

        [Test]
        public void ClaimAchievementTier_DoubleClaimSameTier_Fails()
        {
            var ap = _save.Current.GetOrCreateAchievementProgress("first_smash");
            ap.progress = 1;
            _missionService.ClaimAchievementTier("first_smash");
            var second = _missionService.ClaimAchievementTier("first_smash");
            Assert.IsFalse(second.Success);
        }

        [Test]
        public void MultiTierAchievement_ClaimsInOrder()
        {
            var def = _achConfig.GetAchievement("level_milestones");
            var ap  = _save.Current.GetOrCreateAchievementProgress("level_milestones");

            ap.progress = def.tiers[0].targetCount;
            var tier0 = _missionService.ClaimAchievementTier("level_milestones");
            Assert.IsTrue(tier0.Success);

            ap.progress = def.tiers[1].targetCount;
            var tier1 = _missionService.ClaimAchievementTier("level_milestones");
            Assert.IsTrue(tier1.Success);

            Assert.AreEqual(2, _save.Current.GetOrCreateAchievementProgress("level_milestones").claimedTierCount);
        }
    }

    // =========================================================================
    // SaveDataV6Tests
    // =========================================================================

    [TestFixture]
    public class SaveDataV6Tests
    {
        [Test]
        public void CurrentVersion_Is6()
        {
            Assert.AreEqual(6, SaveData.CurrentVersion);
        }

        [Test]
        public void MissionProgress_DefaultsToEmpty()
        {
            var data = new SaveData();
            Assert.IsNotNull(data.missionProgress);
            Assert.AreEqual(0, data.missionProgress.Count);
        }

        [Test]
        public void AchievementProgress_DefaultsToEmpty()
        {
            var data = new SaveData();
            Assert.IsNotNull(data.achievementProgress);
            Assert.AreEqual(0, data.achievementProgress.Count);
        }

        [Test]
        public void GetOrCreateMissionProgress_CreatesIfMissing()
        {
            var data = new SaveData();
            var mp   = data.GetOrCreateMissionProgress("play_5_levels");
            Assert.IsNotNull(mp);
            Assert.AreEqual("play_5_levels", mp.missionId);
            Assert.AreEqual(1, data.missionProgress.Count);
        }
    }

    // =========================================================================
    // NotificationBadgeTests
    // =========================================================================

    [TestFixture]
    public class NotificationBadgeTests
    {
        private M10TestSaveService  _save;
        private CurrencyService     _currency;
        private KingProgressionService _progression;
        private RewardService       _rewardService;
        private PowerUpService      _powerUpService;
        private DailyRewardConfig   _dailyConfig;
        private MissionConfig       _missionConfig;
        private AchievementConfig   _achConfig;
        private DailyRewardService  _dailyService;

        [SetUp]
        public void SetUp()
        {
            ServiceLocator.Initialize();

            _save           = new M10TestSaveService();
            _currency       = new CurrencyService(_save);
            _progression    = new KingProgressionService(_save, null);
            _rewardService  = new RewardService(_save, _currency, _progression, null);
            _powerUpService = new PowerUpService(_save, _currency);

            _dailyConfig = ScriptableObject.CreateInstance<DailyRewardConfig>();
            _dailyConfig.InitializeDefaults();

            _missionConfig = ScriptableObject.CreateInstance<MissionConfig>();
            _missionConfig.InitializeDefaults();

            _achConfig = ScriptableObject.CreateInstance<AchievementConfig>();
            _achConfig.InitializeDefaults();

            _dailyService = new DailyRewardService(_save, _rewardService, _dailyConfig, _powerUpService);
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(_dailyConfig);
            UnityEngine.Object.DestroyImmediate(_missionConfig);
            UnityEngine.Object.DestroyImmediate(_achConfig);
            ServiceLocator.Initialize();
        }

        [Test]
        public void HasDailyRewardAvailable_WhenCanClaim_ReturnsTrue()
        {
            bool result = NotificationBadgeService.HasDailyRewardAvailable(_save, _dailyConfig, _dailyService);
            Assert.IsTrue(result);
        }

        [Test]
        public void HasClaimableMission_WhenComplete_ReturnsTrue()
        {
            var def = _missionConfig.GetMission("play_5_levels");
            _save.Current.GetOrCreateMissionProgress("play_5_levels").progress = def.target;
            bool result = NotificationBadgeService.HasClaimableMission(_save, _missionConfig);
            Assert.IsTrue(result);
        }

        [Test]
        public void HasClaimableAchievement_WhenTierReached_ReturnsTrue()
        {
            var def = _achConfig.GetAchievement("first_smash");
            _save.Current.GetOrCreateAchievementProgress("first_smash").progress = def.tiers[0].targetCount;
            bool result = NotificationBadgeService.HasClaimableAchievement(_save, _achConfig);
            Assert.IsTrue(result);
        }
    }
}
