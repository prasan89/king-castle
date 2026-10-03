using System.Threading.Tasks;
using KingSmash.Core;

namespace KingSmash.Services
{
    public class AdsServiceMock : IAdsService
    {
        public bool IsRewardedAdReady => true;
        public bool IsInterstitialReady => true;

        public Task InitializeAsync()
        {
            GameLogger.Info("AdsServiceMock", "Ads service initialized.");
            return Task.CompletedTask;
        }

        public Task<AdResult> ShowRewardedAdAsync(string placement)
        {
            GameLogger.Info("AdsServiceMock", $"Showing rewarded ad: {placement}");
            ServiceLocator.Get<IAnalyticsService>().LogEvent(AnalyticsEvents.RewardedAdCompleted,
                ("placement", placement));
            return Task.FromResult(AdResult.Completed);
        }

        public Task ShowInterstitialAsync(string placement)
        {
            GameLogger.Info("AdsServiceMock", $"Showing interstitial: {placement}");
            return Task.CompletedTask;
        }

        public void LoadRewardedAd() => GameLogger.Debug("AdsServiceMock", "LoadRewardedAd");
        public void LoadInterstitial() => GameLogger.Debug("AdsServiceMock", "LoadInterstitial");
    }
}
