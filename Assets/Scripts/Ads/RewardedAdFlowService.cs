using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KingSmash.Core;
using KingSmash.Economy;
using KingSmash.PowerUps;
using KingSmash.Services;
using KingSmash.Shop;
using KingSmash.UI;

namespace KingSmash.Ads
{
    public sealed class RewardedAdFlowService
    {
        public static event Action<AdRewardResult> OnAdRewardGranted;

        private readonly IRewardedAdService       _rewardedAd;
        private readonly RewardService            _rewardService;
        private readonly ISaveService             _save;
        private readonly IAdEntitlementService    _adEntitlement;
        private readonly IConfigService           _config;
        private readonly PowerUpService           _powerUpService;
        private readonly AdConfiguration          _adConfig;
        private readonly HashSet<string>          _claimedRewardIds = new HashSet<string>();

        public RewardedAdFlowService(
            IRewardedAdService    rewardedAd,
            RewardService         rewardService,
            ISaveService          save,
            IAdEntitlementService adEntitlement,
            IConfigService        config,
            PowerUpService        powerUpService,
            AdConfiguration       adConfig)
        {
            _rewardedAd    = rewardedAd    ?? throw new ArgumentNullException(nameof(rewardedAd));
            _rewardService = rewardService ?? throw new ArgumentNullException(nameof(rewardService));
            _save          = save          ?? throw new ArgumentNullException(nameof(save));
            _adEntitlement = adEntitlement ?? throw new ArgumentNullException(nameof(adEntitlement));
            _config        = config        ?? throw new ArgumentNullException(nameof(config));
            _powerUpService = powerUpService ?? throw new ArgumentNullException(nameof(powerUpService));
            _adConfig      = adConfig      ?? throw new ArgumentNullException(nameof(adConfig));
        }

        public async Task<AdRewardResult> RequestDoubleRewardAsync(LevelResult levelResult)
        {
            if (levelResult == null) throw new ArgumentNullException(nameof(levelResult));

            if (!_adEntitlement.CanShowAds())
                return AdRewardResult.Unavailable(AdPlacement.LevelCompleteDoubleReward);

            if (!_adConfig.doubleRewardEnabled)
                return AdRewardResult.Unavailable(AdPlacement.LevelCompleteDoubleReward);

            if (!_adConfig.rewardedAdsEnabled)
                return AdRewardResult.Unavailable(AdPlacement.LevelCompleteDoubleReward);

            if (_rewardedAd.CheckAvailability(AdPlacement.LevelCompleteDoubleReward) != AdAvailability.Available)
                return AdRewardResult.Unavailable(AdPlacement.LevelCompleteDoubleReward);

            var adResult = await _rewardedAd.ShowRewardedAdAsync(AdPlacement.LevelCompleteDoubleReward);

            if (!adResult.WasRewarded)
                return adResult;

            var claimId = Guid.NewGuid().ToString();
            if (_claimedRewardIds.Contains(claimId))
                return AdRewardResult.Unavailable(AdPlacement.LevelCompleteDoubleReward);

            long bonusCoins = (long)(levelResult.CoinsEarned * (_adConfig.doubleRewardMultiplier - 1f));
            GrantAdReward(bonusCoins, "double_reward_" + claimId);
            _claimedRewardIds.Add(claimId);

            var result = new AdRewardResult
            {
                WasRewarded   = true,
                Placement     = AdPlacement.LevelCompleteDoubleReward,
                RewardClaimId = claimId,
                CoinsGranted  = bonusCoins,
                GemsGranted   = 0,
                PowerUpType   = PowerUpType.None,
                PowerUpCount  = 0
            };

            OnAdRewardGranted?.Invoke(result);
            return result;
        }

        public async Task<AdRewardResult> RequestExtraAttemptAsync()
        {
            if (!_adEntitlement.CanShowAds())
                return AdRewardResult.Unavailable(AdPlacement.LevelFailedExtraAttempt);

            if (!_adConfig.extraAttemptEnabled)
                return AdRewardResult.Unavailable(AdPlacement.LevelFailedExtraAttempt);

            if (!_adConfig.rewardedAdsEnabled)
                return AdRewardResult.Unavailable(AdPlacement.LevelFailedExtraAttempt);

            if (_rewardedAd.CheckAvailability(AdPlacement.LevelFailedExtraAttempt) != AdAvailability.Available)
                return AdRewardResult.Unavailable(AdPlacement.LevelFailedExtraAttempt);

            var adResult = await _rewardedAd.ShowRewardedAdAsync(AdPlacement.LevelFailedExtraAttempt);

            if (!adResult.WasRewarded)
                return adResult;

            var result = new AdRewardResult
            {
                WasRewarded   = true,
                Placement     = AdPlacement.LevelFailedExtraAttempt,
                RewardClaimId = Guid.NewGuid().ToString(),
                CoinsGranted  = 0,
                GemsGranted   = 0,
                PowerUpType   = PowerUpType.None,
                PowerUpCount  = 0
            };

            OnAdRewardGranted?.Invoke(result);
            return result;
        }

        public async Task<AdRewardResult> RequestFreePowerUpAsync()
        {
            if (!_adEntitlement.CanShowAds())
                return AdRewardResult.Unavailable(AdPlacement.FreePowerUp);

            if (!_adConfig.freePowerUpEnabled)
                return AdRewardResult.Unavailable(AdPlacement.FreePowerUp);

            if (!_adConfig.rewardedAdsEnabled)
                return AdRewardResult.Unavailable(AdPlacement.FreePowerUp);

            if (_rewardedAd.CheckAvailability(AdPlacement.FreePowerUp) != AdAvailability.Available)
                return AdRewardResult.Unavailable(AdPlacement.FreePowerUp);

            var adResult = await _rewardedAd.ShowRewardedAdAsync(AdPlacement.FreePowerUp);

            if (!adResult.WasRewarded)
                return adResult;

            _powerUpService.Award(_adConfig.freePowerUpType, 1);

            var result = new AdRewardResult
            {
                WasRewarded   = true,
                Placement     = AdPlacement.FreePowerUp,
                RewardClaimId = Guid.NewGuid().ToString(),
                CoinsGranted  = 0,
                GemsGranted   = 0,
                PowerUpType   = _adConfig.freePowerUpType,
                PowerUpCount  = 1
            };

            OnAdRewardGranted?.Invoke(result);
            return result;
        }

        public void GrantAdReward(long coins, string reason)
        {
            if (coins <= 0) return;
            ServiceLocator.Get<CurrencyService>().TryAdd(coins, reason, out _);
        }
    }
}
