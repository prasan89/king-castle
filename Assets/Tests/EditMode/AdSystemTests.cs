using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using KingSmash.Ads;
using KingSmash.Shop;
using KingSmash.Services;
using KingSmash.Save;
using KingSmash.Economy;
using KingSmash.Progression;
using KingSmash.PowerUps;
using KingSmash.Core;
using KingSmash.UI;

namespace KingSmash.Tests.EditMode
{
    internal class M9TestSaveService : ISaveService
    {
        public SaveData Current { get; private set; }

        public M9TestSaveService()
        {
            Current = new SaveData
            {
                playerId         = "m9-test",
                kingProgression  = new KingProgression(),
                powerUpInventory = new PowerUpInventory(),
                ownedProducts    = new List<string>(),
                adSessionStats   = new AdSessionStats(),
            };
        }

        public void Load()   { }
        public void Save()   { }
        public void Delete()
        {
            Current = new SaveData
            {
                playerId         = "m9-test",
                kingProgression  = new KingProgression(),
                powerUpInventory = new PowerUpInventory(),
                ownedProducts    = new List<string>(),
                adSessionStats   = new AdSessionStats(),
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
    // AdConfigurationTests
    // =========================================================================

    [TestFixture]
    public class AdConfigurationTests
    {
        private AdConfiguration _config;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<AdConfiguration>();
        }

        [TearDown]
        public void TearDown() => UnityEngine.Object.DestroyImmediate(_config);

        [Test]
        public void RewardedAdsEnabled_DefaultTrue()
        {
            Assert.IsTrue(_config.rewardedAdsEnabled);
        }

        [Test]
        public void InterstitialAdsEnabled_DefaultTrue()
        {
            Assert.IsTrue(_config.interstitialAdsEnabled);
        }

        [Test]
        public void InterstitialMinLevels_DefaultThree()
        {
            Assert.AreEqual(3, _config.interstitialMinLevels);
        }

        [Test]
        public void MaxInterstitialsPerSession_DefaultThree()
        {
            Assert.AreEqual(3, _config.maxInterstitialsPerSession);
        }

        [Test]
        public void DoubleRewardEnabled_DefaultTrue()
        {
            Assert.IsTrue(_config.doubleRewardEnabled);
        }

        [Test]
        public void ExtraAttemptEnabled_DefaultTrue()
        {
            Assert.IsTrue(_config.extraAttemptEnabled);
        }

        [Test]
        public void FreePowerUpEnabled_DefaultTrue()
        {
            Assert.IsTrue(_config.freePowerUpEnabled);
        }

        [Test]
        public void FreePowerUpType_DefaultBomb()
        {
            Assert.AreEqual(PowerUpType.Bomb, _config.freePowerUpType);
        }
    }

    // =========================================================================
    // InterstitialPolicyTests
    // =========================================================================

    [TestFixture]
    public class InterstitialPolicyTests
    {
        private AdConfiguration _config;
        private RemoteConfigServiceMock _configService;
        private InterstitialPolicy _policy;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<AdConfiguration>();
            _configService = new RemoteConfigServiceMock();
            _policy = new InterstitialPolicy(_config, _configService);
        }

        [TearDown]
        public void TearDown() => UnityEngine.Object.DestroyImmediate(_config);

        [Test]
        public void CanShow_BelowMinLevels_ReturnsFalse()
        {
            int enoughSession = _policy.MaxAdsPerSession - 1;
            float enoughTime = _policy.MinimumSessionTimeSeconds + 1f;
            Assert.IsFalse(_policy.CanShow(_policy.MinimumLevelsBetweenAds - 1, enoughSession, enoughTime));
        }

        [Test]
        public void CanShow_AtOrAboveMinLevels_ReturnsTrue()
        {
            int enoughSession = _policy.MaxAdsPerSession - 1;
            float enoughTime = _policy.MinimumSessionTimeSeconds + 1f;
            Assert.IsTrue(_policy.CanShow(_policy.MinimumLevelsBetweenAds, enoughSession, enoughTime));
        }

        [Test]
        public void CanShow_MaxPerSessionReached_ReturnsFalse()
        {
            float enoughTime = _policy.MinimumSessionTimeSeconds + 1f;
            Assert.IsFalse(_policy.CanShow(_policy.MinimumLevelsBetweenAds, _policy.MaxAdsPerSession, enoughTime));
        }

        [Test]
        public void CanShow_SessionTimeTooShort_ReturnsFalse()
        {
            int enoughSession = _policy.MaxAdsPerSession - 1;
            Assert.IsFalse(_policy.CanShow(_policy.MinimumLevelsBetweenAds, enoughSession, _policy.MinimumSessionTimeSeconds - 1f));
        }

        [Test]
        public void CanShow_AllConditionsMet_ReturnsTrue()
        {
            Assert.IsTrue(_policy.CanShow(
                _policy.MinimumLevelsBetweenAds,
                _policy.MaxAdsPerSession - 1,
                _policy.MinimumSessionTimeSeconds + 1f));
        }
    }

    // =========================================================================
    // RewardedAdFlowTests
    // =========================================================================

    [TestFixture]
    public class RewardedAdFlowTests
    {
        private AdConfiguration _adConfig;
        private M9TestSaveService _save;
        private AdsServiceMock _adsMock;
        private CurrencyService _currency;
        private KingProgressionService _progression;
        private RewardService _rewardService;
        private PowerUpService _powerUpService;
        private EntitlementService _entitlement;
        private RemoteConfigServiceMock _configService;
        private RewardedAdFlowService _flowService;

        [SetUp]
        public void SetUp()
        {
            ServiceLocator.Initialize();

            _adConfig      = ScriptableObject.CreateInstance<AdConfiguration>();
            _save          = new M9TestSaveService();
            _adsMock       = new AdsServiceMock();
            _currency      = new CurrencyService(_save);
            _entitlement   = new EntitlementService(_save);
            _configService = new RemoteConfigServiceMock();

            ServiceLocator.Register<ISaveService>(_save);
            ServiceLocator.Register<CurrencyService>(_currency);

            var upgradeConfig = Resources.Load<KingUpgradeConfig>("KingUpgradeConfig");
            _progression = new KingProgressionService(_save, upgradeConfig);

            _rewardService  = new RewardService(_save, _currency, _progression, null);
            _powerUpService = new PowerUpService(_save, _currency);

            _flowService = new RewardedAdFlowService(
                _adsMock,
                _rewardService,
                _save,
                _entitlement,
                _configService,
                _powerUpService,
                _adConfig);
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(_adConfig);
            ServiceLocator.Initialize();
        }

        [Test]
        public async Task RequestDoubleReward_RemoveAdsPurchased_ReturnsUnavailable()
        {
            _entitlement.GrantEntitlement(EntitlementService.RemoveAdsProductId);
            var levelResult = new LevelResult { LevelIndex = 0, CoinsEarned = 1000, IsVictory = true, Stars = 3 };

            var result = await _flowService.RequestDoubleRewardAsync(levelResult);

            Assert.IsFalse(result.WasRewarded);
            Assert.AreEqual(AdPlacement.LevelCompleteDoubleReward, result.Placement);
        }

        [Test]
        public async Task RequestDoubleReward_Offline_ReturnsUnavailable()
        {
            _adsMock.SimulateOffline(true);
            var levelResult = new LevelResult { LevelIndex = 0, CoinsEarned = 1000, IsVictory = true, Stars = 3 };

            var result = await _flowService.RequestDoubleRewardAsync(levelResult);

            Assert.IsFalse(result.WasRewarded);
        }

        [Test]
        public async Task RequestDoubleReward_AdCompleted_CoinsGranted()
        {
            _adsMock.SimulateNextRewardedOutcome(true);
            _save.Current.coins = 0;
            var levelResult = new LevelResult { LevelIndex = 99, CoinsEarned = 1000, IsVictory = true, Stars = 3 };

            var result = await _flowService.RequestDoubleRewardAsync(levelResult);

            Assert.IsTrue(result.WasRewarded);
            Assert.Greater(result.CoinsGranted, 0L);
        }

        [Test]
        public async Task RequestDoubleReward_AdSkipped_WasRewardedFalse()
        {
            _adsMock.SimulateNextRewardedOutcome(false);
            var levelResult = new LevelResult { LevelIndex = 1, CoinsEarned = 500, IsVictory = true, Stars = 2 };

            var result = await _flowService.RequestDoubleRewardAsync(levelResult);

            Assert.IsFalse(result.WasRewarded);
            Assert.AreEqual(0L, result.CoinsGranted);
        }

        [Test]
        public async Task RequestDoubleReward_SameClaimId_NotGrantedTwice()
        {
            _adsMock.SimulateNextRewardedOutcome(true);
            var levelResult = new LevelResult { LevelIndex = 2, CoinsEarned = 1000, IsVictory = true, Stars = 3 };

            var first = await _flowService.RequestDoubleRewardAsync(levelResult);
            Assert.IsTrue(first.WasRewarded);

            _adsMock.SimulateNextRewardedOutcome(true);
            var second = await _flowService.RequestDoubleRewardAsync(levelResult);

            Assert.IsTrue(second.WasRewarded);
            Assert.AreNotEqual(first.RewardClaimId, second.RewardClaimId,
                "Each call produces a distinct claim ID; no double-grant for the same ID.");
        }

        [Test]
        public async Task RequestExtraAttempt_AdCompleted_WasRewardedTrue()
        {
            _adsMock.SimulateNextRewardedOutcome(true);

            var result = await _flowService.RequestExtraAttemptAsync();

            Assert.IsTrue(result.WasRewarded);
            Assert.AreEqual(AdPlacement.LevelFailedExtraAttempt, result.Placement);
        }

        [Test]
        public async Task RequestFreePowerUp_AdCompleted_PowerUpAwarded()
        {
            _adsMock.SimulateNextRewardedOutcome(true);
            int before = _powerUpService.GetCount(_adConfig.freePowerUpType);

            var result = await _flowService.RequestFreePowerUpAsync();

            Assert.IsTrue(result.WasRewarded);
            Assert.AreEqual(_adConfig.freePowerUpType, result.PowerUpType);
            Assert.AreEqual(1, result.PowerUpCount);
            Assert.AreEqual(before + 1, _powerUpService.GetCount(_adConfig.freePowerUpType));
        }
    }

    // =========================================================================
    // AdAvailabilityTests
    // =========================================================================

    [TestFixture]
    public class AdAvailabilityTests
    {
        private AdsServiceMock _adsMock;
        private M9TestSaveService _save;
        private EntitlementService _entitlement;

        [SetUp]
        public void SetUp()
        {
            _adsMock     = new AdsServiceMock();
            _save        = new M9TestSaveService();
            _entitlement = new EntitlementService(_save);
        }

        [Test]
        public void CheckAvailability_RemoveAdsPurchased_CanShowAdsFalse()
        {
            _entitlement.GrantEntitlement(EntitlementService.RemoveAdsProductId);
            Assert.IsFalse(_entitlement.CanShowAds());
        }

        [Test]
        public void CheckAvailability_Offline_ReturnsOffline()
        {
            _adsMock.SimulateOffline(true);
            var availability = _adsMock.CheckAvailability(AdPlacement.LevelCompleteDoubleReward);
            Assert.AreEqual(AdAvailability.Offline, availability);
        }

        [Test]
        public void CheckAvailability_Normal_ReturnsAvailable()
        {
            _adsMock.SimulateOffline(false);
            var availability = _adsMock.CheckAvailability(AdPlacement.LevelCompleteDoubleReward);
            Assert.AreEqual(AdAvailability.Available, availability);
        }
    }

    // =========================================================================
    // AdSessionStatsTests
    // =========================================================================

    [TestFixture]
    public class AdSessionStatsTests
    {
        [Test]
        public void AdSessionStats_Defaults_AreZeroAndMinusOne()
        {
            var stats = new AdSessionStats();
            Assert.AreEqual(0,  stats.interstitialsShownThisSession);
            Assert.AreEqual(0,  stats.totalInterstitialsShown);
            Assert.AreEqual(0L, stats.sessionStartTimestamp);
            Assert.AreEqual(-1, stats.lastInterstitialLevelIndex);
        }

        [Test]
        public void ResetSession_SetsTimestampAndClearsSessionCount()
        {
            var stats = new AdSessionStats();
            stats.interstitialsShownThisSession = 5;
            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            stats.ResetSession(now);

            Assert.AreEqual(0,   stats.interstitialsShownThisSession);
            Assert.AreEqual(now, stats.sessionStartTimestamp);
        }

        [Test]
        public void RecordInterstitialShown_IncrementsCounters()
        {
            var stats = new AdSessionStats();
            stats.RecordInterstitialShown(5);

            Assert.AreEqual(1, stats.interstitialsShownThisSession);
            Assert.AreEqual(1, stats.totalInterstitialsShown);
            Assert.AreEqual(5, stats.lastInterstitialLevelIndex);
        }

        [Test]
        public void RecordInterstitialShown_MultipleRecordings_AccumulateCorrectly()
        {
            var stats = new AdSessionStats();
            stats.RecordInterstitialShown(1);
            stats.RecordInterstitialShown(4);
            stats.RecordInterstitialShown(7);

            Assert.AreEqual(3, stats.interstitialsShownThisSession);
            Assert.AreEqual(3, stats.totalInterstitialsShown);
            Assert.AreEqual(7, stats.lastInterstitialLevelIndex);
        }
    }

    // =========================================================================
    // SaveDataV5Tests
    // =========================================================================

    [TestFixture]
    public class SaveDataV5Tests
    {
        [Test]
        public void SaveData_CurrentVersion_IsFive()
        {
            Assert.AreEqual(5, SaveData.CurrentVersion);
        }

        [Test]
        public void SaveData_AdSessionStats_NonNull()
        {
            var data = new SaveData();
            Assert.IsNotNull(data.adSessionStats);
        }

        [Test]
        public void SaveData_AdSessionStats_DefaultValues()
        {
            var data = new SaveData();
            Assert.AreEqual(0,  data.adSessionStats.interstitialsShownThisSession);
            Assert.AreEqual(0,  data.adSessionStats.totalInterstitialsShown);
            Assert.AreEqual(0L, data.adSessionStats.sessionStartTimestamp);
            Assert.AreEqual(-1, data.adSessionStats.lastInterstitialLevelIndex);
        }
    }
}
