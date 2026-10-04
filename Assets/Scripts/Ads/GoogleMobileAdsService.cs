using System;
using System.Threading.Tasks;
using KingSmash.Services;

#if GOOGLE_MOBILE_ADS
using Google.MobileAds;
#endif

namespace KingSmash.Ads
{
    public sealed class GoogleMobileAdsService : IAdsService, IRewardedAdService, IInterstitialAdService
    {
        private readonly AdConfiguration _config;

        public GoogleMobileAdsService(AdConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

#if GOOGLE_MOBILE_ADS
        private RewardedAd _rewardedAd;
        private InterstitialAd _interstitialAd;
        private bool _rewardedLoading;
        private bool _interstitialLoading;

        public bool IsRewardedAdReady => _rewardedAd != null && _rewardedAd.CanShowAd();
        public bool IsInterstitialReady => _interstitialAd != null && _interstitialAd.CanShowAd();

        public Task InitializeAsync()
        {
            var tcs = new TaskCompletionSource<bool>();
            MobileAds.Initialize(initStatus =>
            {
                tcs.TrySetResult(true);
            });
            return tcs.Task;
        }

        public Task<AdResult> ShowRewardedAdAsync(string placement)
        {
            if (_rewardedAd == null || !_rewardedAd.CanShowAd())
                return Task.FromResult(AdResult.Failed);

            var tcs = new TaskCompletionSource<AdResult>();
            bool rewarded = false;

            _rewardedAd.OnAdFullScreenContentClosed += () =>
            {
                tcs.TrySetResult(rewarded ? AdResult.Completed : AdResult.Skipped);
                LoadRewardedAd();
            };

            _rewardedAd.OnAdFullScreenContentFailed += error =>
            {
                tcs.TrySetResult(AdResult.Failed);
            };

            _rewardedAd.Show(reward =>
            {
                rewarded = true;
            });

            return tcs.Task;
        }

        public Task ShowInterstitialAsync(string placement)
        {
            if (_interstitialAd == null || !_interstitialAd.CanShowAd())
                return Task.CompletedTask;

            var tcs = new TaskCompletionSource<bool>();

            _interstitialAd.OnAdFullScreenContentClosed += () =>
            {
                tcs.TrySetResult(true);
                LoadInterstitial();
            };

            _interstitialAd.OnAdFullScreenContentFailed += error =>
            {
                tcs.TrySetResult(false);
            };

            _interstitialAd.Show();
            return tcs.Task;
        }

        public void LoadRewardedAd()
        {
            if (_rewardedLoading) return;
            _rewardedLoading = true;

            var adRequest = new AdRequest();
            RewardedAd.Load(_config.GetRewardedAdUnitId(), adRequest, (ad, error) =>
            {
                _rewardedLoading = false;
                if (error != null) return;
                _rewardedAd = ad;
            });
        }

        public void LoadInterstitial()
        {
            if (_interstitialLoading) return;
            _interstitialLoading = true;

            var adRequest = new AdRequest();
            InterstitialAd.Load(_config.GetInterstitialAdUnitId(), adRequest, (ad, error) =>
            {
                _interstitialLoading = false;
                if (error != null) return;
                _interstitialAd = ad;
            });
        }

        public AdAvailability CheckAvailability(AdPlacement placement)
        {
            if (IsRewardedAdReady) return AdAvailability.Available;
            if (_rewardedLoading) return AdAvailability.Loading;
            return AdAvailability.Unavailable;
        }

        public async Task<AdRewardResult> ShowRewardedAdAsync(AdPlacement placement)
        {
            var adResult = await ShowRewardedAdAsync(placement.ToString());

            return new AdRewardResult
            {
                WasRewarded   = adResult == AdResult.Completed,
                Placement     = placement,
                RewardClaimId = string.Empty,
                CoinsGranted  = 0,
                GemsGranted   = 0,
                PowerUpType   = KingSmash.PowerUps.PowerUpType.None,
                PowerUpCount  = 0
            };
        }

        void IRewardedAdService.Preload() => LoadRewardedAd();

        public bool CanShowInterstitial(int completedLevelsCount) => IsInterstitialReady;

        public async Task ShowInterstitialAsync(int completedLevelsCount, Action onDone)
        {
            await ShowInterstitialAsync(AdPlacement.InterstitialLevelTransition.ToString());
            onDone?.Invoke();
        }

        void IInterstitialAdService.Preload() => LoadInterstitial();

#else
        public bool IsRewardedAdReady =>
            throw new NotImplementedException("Install com.google.ads.mobile.unity package and add GOOGLE_MOBILE_ADS scripting define symbol");

        public bool IsInterstitialReady =>
            throw new NotImplementedException("Install com.google.ads.mobile.unity package and add GOOGLE_MOBILE_ADS scripting define symbol");

        public Task InitializeAsync() =>
            throw new NotImplementedException("Install com.google.ads.mobile.unity package and add GOOGLE_MOBILE_ADS scripting define symbol");

        public Task<AdResult> ShowRewardedAdAsync(string placement) =>
            throw new NotImplementedException("Install com.google.ads.mobile.unity package and add GOOGLE_MOBILE_ADS scripting define symbol");

        public Task ShowInterstitialAsync(string placement) =>
            throw new NotImplementedException("Install com.google.ads.mobile.unity package and add GOOGLE_MOBILE_ADS scripting define symbol");

        public void LoadRewardedAd() =>
            throw new NotImplementedException("Install com.google.ads.mobile.unity package and add GOOGLE_MOBILE_ADS scripting define symbol");

        public void LoadInterstitial() =>
            throw new NotImplementedException("Install com.google.ads.mobile.unity package and add GOOGLE_MOBILE_ADS scripting define symbol");

        public AdAvailability CheckAvailability(AdPlacement placement) =>
            throw new NotImplementedException("Install com.google.ads.mobile.unity package and add GOOGLE_MOBILE_ADS scripting define symbol");

        public Task<AdRewardResult> ShowRewardedAdAsync(AdPlacement placement) =>
            throw new NotImplementedException("Install com.google.ads.mobile.unity package and add GOOGLE_MOBILE_ADS scripting define symbol");

        public void Preload() =>
            throw new NotImplementedException("Install com.google.ads.mobile.unity package and add GOOGLE_MOBILE_ADS scripting define symbol");

        public bool CanShowInterstitial(int completedLevelsCount) =>
            throw new NotImplementedException("Install com.google.ads.mobile.unity package and add GOOGLE_MOBILE_ADS scripting define symbol");

        public Task ShowInterstitialAsync(int completedLevelsCount, Action onDone) =>
            throw new NotImplementedException("Install com.google.ads.mobile.unity package and add GOOGLE_MOBILE_ADS scripting define symbol");
#endif
    }
}
