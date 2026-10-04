using System;
using System.Threading.Tasks;
using KingSmash.Core;
using KingSmash.Services;
using KingSmash.Shop;

namespace KingSmash.Ads
{
    public sealed class AdMobInterstitialService : IInterstitialAdService
    {
        private readonly IAdsService           _adsService;
        private readonly AdConfiguration       _adConfig;
        private readonly IAdEntitlementService _adEntitlement;
        private readonly ISaveService          _save;
        private readonly InterstitialPolicy    _policy;

        private int _levelsSinceLastAd = 0;

        public AdMobInterstitialService(
            IAdsService           adsService,
            AdConfiguration       adConfig,
            IAdEntitlementService adEntitlement,
            ISaveService          save,
            IConfigService        configService)
        {
            _adsService    = adsService    ?? throw new ArgumentNullException(nameof(adsService));
            _adConfig      = adConfig      ?? throw new ArgumentNullException(nameof(adConfig));
            _adEntitlement = adEntitlement ?? throw new ArgumentNullException(nameof(adEntitlement));
            _save          = save          ?? throw new ArgumentNullException(nameof(save));
            _policy        = new InterstitialPolicy(adConfig, configService);
        }

        public bool CanShowInterstitial(int completedLevelsCount)
        {
            if (!_adConfig.interstitialAdsEnabled) return false;
            if (!_adEntitlement.CanShowAds()) return false;

            var stats = _save.Current.adSessionStats;
            float sessionSeconds = GetSessionElapsedSeconds(stats);

            return _policy.CanShow(_levelsSinceLastAd, stats.interstitialsShownThisSession, sessionSeconds);
        }

        public async Task ShowInterstitialAsync(int completedLevelsCount, Action onDone)
        {
            if (!CanShowInterstitial(completedLevelsCount))
            {
                onDone?.Invoke();
                return;
            }

            await _adsService.ShowInterstitialAsync(AdPlacement.InterstitialLevelTransition.ToString());

            _save.Current.adSessionStats.RecordInterstitialShown(completedLevelsCount);
            _levelsSinceLastAd = 0;
            _save.Save();

            onDone?.Invoke();
        }

        public void Preload() => _adsService.LoadInterstitial();

        public void IncrementLevelCount() => _levelsSinceLastAd++;

        private float GetSessionElapsedSeconds(AdSessionStats stats)
        {
            if (stats.sessionStartTimestamp == 0) return 0f;
            long nowSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return (float)(nowSeconds - stats.sessionStartTimestamp);
        }
    }
}
