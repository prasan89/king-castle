using KingSmash.Services;

namespace KingSmash.Ads
{
    public class InterstitialPolicy
    {
        private readonly AdConfiguration _config;
        private readonly IConfigService _configService;

        public int MinimumLevelsBetweenAds { get; private set; }
        public int MaxAdsPerSession { get; private set; }
        public float MinimumSessionTimeSeconds { get; private set; }

        public InterstitialPolicy(AdConfiguration config, IConfigService configService)
        {
            _config = config;
            _configService = configService;
            RefreshFromRemoteConfig();
        }

        public bool CanShow(int levelsSinceLastAd, int shownThisSession, float sessionTimeSeconds)
        {
            if (sessionTimeSeconds < MinimumSessionTimeSeconds) return false;
            if (shownThisSession >= MaxAdsPerSession) return false;
            if (levelsSinceLastAd < MinimumLevelsBetweenAds) return false;
            return true;
        }

        public void RefreshFromRemoteConfig()
        {
            MinimumLevelsBetweenAds = _configService.GetInt("ad_interstitial_min_levels", _config.interstitialMinLevels);
            MaxAdsPerSession = _configService.GetInt("ad_max_interstitials_per_session", _config.maxInterstitialsPerSession);
            MinimumSessionTimeSeconds = _configService.GetFloat("ad_interstitial_min_session_seconds", 60f);
        }
    }
}
