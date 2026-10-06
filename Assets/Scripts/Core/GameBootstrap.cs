// GameBootstrap.cs — M15: production service switching added.
//
// Service registration follows a two-tier compile-guard pattern:
//
//   #if FIREBASE_ENABLED && !KING_SMASH_DEV
//       Register real Firebase/AdMob service (Staging + Production)
//   #else
//       Register mock service (Development / editor / CI without Firebase SDK)
//   #endif
//
// This means:
//   - CI builds without FIREBASE_ENABLED always get mocks   → compiles everywhere
//   - KING_SMASH_DEV builds with FIREBASE_ENABLED still get mocks (safe for dev)
//   - KING_SMASH_STAGING + Production builds with FIREBASE_ENABLED get real services
//
// All non-service registrations (CurrencyService, KingProgressionService, etc.)
// are unchanged from M14.

using UnityEngine;
using KingSmash.Services;
using KingSmash.Services.Mocks;
using KingSmash.Cloud;
using KingSmash.Economy;
using KingSmash.Progression;
using KingSmash.PowerUps;
using KingSmash.Shop;
using KingSmash.Ads;
using KingSmash.Retention;
using KingSmash.Services.Firebase;
using KingSmash.Audio;
using KingSmash.VFX;
using KingSmash.Camera;
using KingSmash.Analytics;
using KingSmash.Config;
using KingSmash.Save;

namespace KingSmash.Core
{
    [DefaultExecutionOrder(-1000)]
    public class GameBootstrap : MonoBehaviour
    {
        [SerializeField] private GameConfig _gameConfig;

        private static bool _initialized;

        private void Awake()
        {
            if (_initialized) { Destroy(gameObject); return; }
            _initialized = true;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }

        private void Initialize()
        {
            GameLogger.Initialize(_gameConfig.logLevel);
            GameLogger.Info("GameBootstrap", "Initializing game systems...");

            FirebaseEnvironmentConfig.LogEnvironment();

            ServiceLocator.Initialize();
            RegisterServices();
            ServiceLocator.Get<ISaveService>().Load();
            ServiceLocator.Get<IConfigService>().Initialize();

            ServiceLocator.Get<DailyRewardService>().CheckAndResetIfNewDay();

            GameLogger.Info("GameBootstrap", "All systems ready.");
            ServiceLocator.Get<IAnalyticsService>().LogEvent(AnalyticsEvents.AppOpen);
            SceneLoader.LoadScene(SceneNames.MainMenu);
        }

        private void RegisterServices()
        {
            // ── Persistence (local save — all environments) ─────────────────
            ServiceLocator.Register<ISaveService>(new LocalSaveService());

            // ── Remote Config ─────────────────────────────────────────
#if FIREBASE_ENABLED && !KING_SMASH_DEV
            ServiceLocator.Register<IConfigService>(new FirebaseRemoteConfigService());
#else
            ServiceLocator.Register<IConfigService>(new RemoteConfigServiceMock());
#endif

            // ── Analytics ───────────────────────────────────────────
#if FIREBASE_ENABLED && !KING_SMASH_DEV
            ServiceLocator.Register<IAnalyticsService>(new FirebaseAnalyticsService());
#else
            ServiceLocator.Register<IAnalyticsService>(new AnalyticsServiceMock());
#endif

            // ── Crash Reporting ─────────────────────────────────────
#if FIREBASE_ENABLED && !KING_SMASH_DEV
            var crashService = new FirebaseCrashlyticsService();
            ServiceLocator.Register<ICrashReportingService>(crashService);
            GameLogger.SetCrashService(crashService);
            crashService.Initialize();
#else
            ServiceLocator.Register<ICrashReportingService>(new MockCrashReportingService());
            GameLogger.SetCrashService(ServiceLocator.Get<ICrashReportingService>());
#endif

            // ── Auth ──────────────────────────────────────────────
#if FIREBASE_ENABLED && !KING_SMASH_DEV
            ServiceLocator.Register<IAuthService>(new FirebaseAuthService());
#else
            ServiceLocator.Register<IAuthService>(new AuthServiceMock());
#endif

            // ── Player data (stub only — backed by cloud save below) ────────
            ServiceLocator.Register<IPlayerDataService>(new PlayerDataServiceMock());

            // ── Billing / IAP ─────────────────────────────────────
            // TODO (M16): Replace PurchaseServiceMock with the Unity IAP or
            // Google Play Billing Library implementation once the store listing
            // and billing credentials are in place.  All server-side purchase
            // verification (receipt + entitlement write) is already implemented
            // in Cloud Run; this stub only affects the client-side purchase flow.
            ServiceLocator.Register<IPurchaseService>(new PurchaseServiceMock());

            // ── Audio (MonoBehaviour) ───────────────────────────────
            var audioService = FindObjectOfType<AudioService>();
            if (audioService != null)
            {
                ServiceLocator.Register<IAudioService>(audioService);
            }
            else
            {
                GameLogger.Warning("GameBootstrap", "AudioService MonoBehaviour not found in scene — falling back to AudioManager stub.");
                ServiceLocator.Register<IAudioService>(new AudioManager());
            }

            // ── VFX (MonoBehaviour) ─────────────────────────────────
            var vfxService = FindObjectOfType<VFXService>();
            if (vfxService != null)
                ServiceLocator.Register<IVFXService>(vfxService);
            else
                GameLogger.Warning("GameBootstrap", "VFXService MonoBehaviour not found in scene.");

            // ── CameraEffect (MonoBehaviour) ──────────────────────────
            var cameraEffectService = FindObjectOfType<CameraEffectService>();
            if (cameraEffectService != null)
                ServiceLocator.Register<ICameraEffectService>(cameraEffectService);
            else
                GameLogger.Warning("GameBootstrap", "CameraEffectService MonoBehaviour not found in scene.");

            // ── HitStop (MonoBehaviour) ───────────────────────────────
            var hitStopService = FindObjectOfType<Gameplay.HitStopService>();
            if (hitStopService != null)
                ServiceLocator.Register<Gameplay.IHitStopService>(hitStopService);
            else
                GameLogger.Warning("GameBootstrap", "HitStopService MonoBehaviour not found in scene.");

            // ── Cloud Save ──────────────────────────────────────────
#if FIREBASE_ENABLED && !KING_SMASH_DEV
            ServiceLocator.Register<ICloudSaveService>(new FirebaseCloudSaveService());
#else
            ServiceLocator.Register<ICloudSaveService>(new CloudSaveServiceMock());
#endif

            var cloudSyncService = new CloudSyncService(
                ServiceLocator.Get<ICloudSaveService>(),
                ServiceLocator.Get<ISaveService>(),
                ServiceLocator.Get<IAuthService>(),
                ServiceLocator.Get<IConfigService>());
            ServiceLocator.Register<CloudSyncService>(cloudSyncService);

            // ── Analytics environment and config version ──────────────────
            if (ServiceLocator.TryGet<IAnalyticsService>(out var analytics))
            {
                analytics.SetEnvironment(FirebaseEnvironmentConfig.Environment);
                analytics.SetConfigVersion("1");
            }

            // ── Upgrades & progression ──────────────────────────────
            var upgradeConfig = Resources.Load<KingUpgradeConfig>("KingUpgradeConfig");
            if (upgradeConfig == null)
                GameLogger.Warning("GameBootstrap", "KingUpgradeConfig not found in Resources.");
            else
                ServiceLocator.Register<KingUpgradeConfig>(upgradeConfig);

            ServiceLocator.Register<CurrencyService>(new CurrencyService(ServiceLocator.Get<ISaveService>()));
            ServiceLocator.Register<KingProgressionService>(new KingProgressionService(ServiceLocator.Get<ISaveService>(), upgradeConfig));
            ServiceLocator.Register<KingUpgradeService>(new KingUpgradeService(ServiceLocator.Get<ISaveService>(), upgradeConfig, ServiceLocator.Get<CurrencyService>()));

            var economyConfig = Resources.Load<EconomyConfig>("EconomyConfig");
            if (economyConfig == null)
                GameLogger.Warning("GameBootstrap", "EconomyConfig not found in Resources.");
            else
                ServiceLocator.Register<EconomyConfig>(economyConfig);

            ServiceLocator.Register<RewardService>(new RewardService(
                ServiceLocator.Get<ISaveService>(),
                ServiceLocator.Get<CurrencyService>(),
                ServiceLocator.Get<KingProgressionService>(),
                economyConfig));

            var powerUpService = new PowerUpService(ServiceLocator.Get<ISaveService>(), ServiceLocator.Get<CurrencyService>());
            ServiceLocator.Register<PowerUpService>(powerUpService);

            // ── Shop & billing infrastructure ─────────────────────────
            var shopCatalog = Resources.Load<ShopProductCatalog>("ShopProductCatalog");
            if (shopCatalog == null)
                GameLogger.Warning("GameBootstrap", "ShopProductCatalog not found in Resources.");

            var storeService = new StoreServiceMock();
            ServiceLocator.Register<IStoreService>(storeService);

            var receiptVerifier = new ReceiptVerificationServiceMock();
            ServiceLocator.Register<IReceiptVerificationService>(receiptVerifier);

            var entitlementService = new EntitlementService(ServiceLocator.Get<ISaveService>());
            ServiceLocator.Register<EntitlementService>(entitlementService);
            ServiceLocator.Register<IEntitlementService>(entitlementService);
            ServiceLocator.Register<IAdEntitlementService>(entitlementService);

            ServiceLocator.Register<ShopService>(new ShopService(
                ServiceLocator.Get<IStoreService>(),
                ServiceLocator.Get<IReceiptVerificationService>(),
                ServiceLocator.Get<RewardService>(),
                ServiceLocator.Get<IEntitlementService>(),
                ServiceLocator.Get<ISaveService>(),
                ServiceLocator.Get<IAuthService>(),
                shopCatalog));

            // ── Ads ───────────────────────────────────────────────
            var adConfig = Resources.Load<AdConfiguration>("AdConfiguration");
            if (adConfig == null)
                GameLogger.Warning("GameBootstrap", "AdConfiguration not found in Resources.");
            else
                ServiceLocator.Register<AdConfiguration>(adConfig);

#if FIREBASE_ENABLED && !KING_SMASH_DEV
            // Production/Staging: use real AdMob SDK.
            // Requires GOOGLE_MOBILE_ADS scripting define symbol and the
            // com.google.ads.mobile.unity package to be imported.
            var adsService = new GoogleMobileAdsService(adConfig);
            ServiceLocator.Register<IAdsService>(adsService);
            ServiceLocator.Register<IRewardedAdService>(adsService);

            var interstitialService = new AdMobInterstitialService(
                adsService,
                adConfig,
                ServiceLocator.Get<IAdEntitlementService>(),
                ServiceLocator.Get<ISaveService>(),
                ServiceLocator.Get<IConfigService>());
            ServiceLocator.Register<IInterstitialAdService>(interstitialService);

            var rewardedAdFlowService = new RewardedAdFlowService(
                adsService,
                ServiceLocator.Get<RewardService>(),
                ServiceLocator.Get<ISaveService>(),
                ServiceLocator.Get<IAdEntitlementService>(),
                ServiceLocator.Get<IConfigService>(),
                powerUpService,
                adConfig);
            ServiceLocator.Register<RewardedAdFlowService>(rewardedAdFlowService);
#else
            // Development: use mock ads so the simulator never calls AdMob servers.
            var adsMock = new AdsServiceMock();
            ServiceLocator.Register<IAdsService>(adsMock);
            ServiceLocator.Register<IRewardedAdService>(adsMock);

            var interstitialServiceMock = new AdMobInterstitialService(
                adsMock,
                adConfig,
                ServiceLocator.Get<IAdEntitlementService>(),
                ServiceLocator.Get<ISaveService>(),
                ServiceLocator.Get<IConfigService>());
            ServiceLocator.Register<IInterstitialAdService>(interstitialServiceMock);

            var rewardedAdFlowServiceMock = new RewardedAdFlowService(
                adsMock,
                ServiceLocator.Get<RewardService>(),
                ServiceLocator.Get<ISaveService>(),
                ServiceLocator.Get<IAdEntitlementService>(),
                ServiceLocator.Get<IConfigService>(),
                powerUpService,
                adConfig);
            ServiceLocator.Register<RewardedAdFlowService>(rewardedAdFlowServiceMock);
#endif

            // ── Retention ───────────────────────────────────────────
            var dailyRewardConfig = Resources.Load<DailyRewardConfig>("DailyRewardConfig");
            if (dailyRewardConfig == null)
                GameLogger.Warning("GameBootstrap", "DailyRewardConfig not found in Resources.");
            else
                ServiceLocator.Register<DailyRewardConfig>(dailyRewardConfig);

            var missionConfig = Resources.Load<MissionConfig>("MissionConfig");
            if (missionConfig == null)
                GameLogger.Warning("GameBootstrap", "MissionConfig not found in Resources.");
            else
                ServiceLocator.Register<MissionConfig>(missionConfig);

            var achievementConfig = Resources.Load<AchievementConfig>("AchievementConfig");
            if (achievementConfig == null)
                GameLogger.Warning("GameBootstrap", "AchievementConfig not found in Resources.");
            else
                ServiceLocator.Register<AchievementConfig>(achievementConfig);

            var dailyRewardService = new DailyRewardService(
                ServiceLocator.Get<ISaveService>(),
                ServiceLocator.Get<RewardService>(),
                dailyRewardConfig,
                ServiceLocator.Get<PowerUpService>());
            ServiceLocator.Register<DailyRewardService>(dailyRewardService);

            var missionService = new MissionService(
                ServiceLocator.Get<ISaveService>(),
                missionConfig,
                achievementConfig,
                ServiceLocator.Get<RewardService>(),
                ServiceLocator.Get<PowerUpService>(),
                ServiceLocator.Get<CurrencyService>());
            missionService.Initialize();
            ServiceLocator.Register<MissionService>(missionService);
        }
    }
}
