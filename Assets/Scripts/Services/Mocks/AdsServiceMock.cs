using System;
using System.Threading.Tasks;
using KingSmash.Ads;
using KingSmash.Core;

namespace KingSmash.Services
{
    public class AdsServiceMock : IAdsService, IRewardedAdService, IInterstitialAdService
    {
        private bool _nextRewardedWasRewarded = true;
        private bool _simulateOffline = false;

        public bool IsRewardedAdReady => !_simulateOffline;
        public bool IsInterstitialReady => !_simulateOffline;

        public void SimulateNextRewardedOutcome(bool wasRewarded)
        {
            _nextRewardedWasRewarded = wasRewarded;
        }

        public void SimulateOffline(bool offline)
        {
            _simulateOffline = offline;
        }

        public Task InitializeAsync()
        {
            GameLogger.Info("AdsServiceMock", "Ads service initialized.");
            return Task.CompletedTask;
        }

        public Task<AdResult> ShowRewardedAdAsync(string placement)
        {
            GameLogger.Info("AdsServiceMock", $"Showing rewarded ad: {placement}");
            if (ServiceLocator.TryGet<IAnalyticsService>(out var analytics))
                analytics.LogEvent(AnalyticsEvents.RewardedAdCompleted, ("placement", placement));
            return Task.FromResult(_nextRewardedWasRewarded ? AdResult.Completed : AdResult.Skipped);
        }

        public Task ShowInterstitialAsync(string placement)
        {
            GameLogger.Info("AdsServiceMock", $"Showing interstitial: {placement}");
            return Task.CompletedTask;
        }

        public void LoadRewardedAd() => GameLogger.Debug("AdsServiceMock", "LoadRewardedAd");
        public void LoadInterstitial() => GameLogger.Debug("AdsServiceMock", "LoadInterstitial");

        public AdAvailability CheckAvailability(AdPlacement placement)
        {
            if (_simulateOffline) return AdAvailability.Offline;
            return AdAvailability.Available;
        }

        public async Task<AdRewardResult> ShowRewardedAdAsync(AdPlacement placement)
        {
            GameLogger.Info("AdsServiceMock", $"Showing rewarded ad for placement: {placement}");
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

        public void Preload() { }

        public bool CanShowInterstitial(int completedLevelsCount) => !_simulateOffline;

        public Task ShowInterstitialAsync(int completedLevelsCount, Action onDone)
        {
            GameLogger.Info("AdsServiceMock", $"Showing interstitial for level count: {completedLevelsCount}");
            onDone?.Invoke();
            return Task.CompletedTask;
        }
    }
}
