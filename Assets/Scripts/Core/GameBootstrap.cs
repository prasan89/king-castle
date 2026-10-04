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
            ServiceLocator.Register<ISaveService>(new LocalSaveService());
            ServiceLocator.Register<IConfigService>(new RemoteConfigServiceMock());
            ServiceLocator.Register<IAnalyticsService>(new AnalyticsServiceMock());
            ServiceLocator.Register<IAuthService>(new AuthServiceMock());
            ServiceLocator.Register<IPlayerDataService>(new PlayerDataServiceMock());
            ServiceLocator.Register<IPurchaseService>(new PurchaseServiceMock());

            // ── Audio (MonoBehaviour) ─────────────────────────────────────────
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

            // ── VFX (MonoBehaviour) ───────────────────────────────────────────
            var vfxService = FindObjectOfType<VFXService>();
            if (vfxService != null)
                ServiceLocator.Register<IVFXService>(vfxService);
            else
                GameLogger.Warning("GameBootstrap", "VFXService MonoBehaviour not found in scene.");

            // ── CameraEffect (MonoBehaviour) ──────────────────────────────────
            var cameraEffectService = FindObjectOfType<CameraEffectService>();
            if (cameraEffectService != null)
                ServiceLocator.Register<ICameraEffectService>(cameraEffectService);
            else
                GameLogger.Warning("GameBootstrap", "CameraEffectService MonoBehaviour not found in scene.");

            // ── HitStop (MonoBehaviour) ───────────────────────────────────────
            var hitStopService = FindObjectOfType<Gameplay.HitStopService>();
            if (hitStopService != null)
                ServiceLocator.Register<Gameplay.IHitStopService>(hitStopService);
            else
                GameLogger.Warning("GameBootstrap", "HitStopService MonoBehaviour not found in scene.");

            // ── Cloud save (mock for dev; swap for FirebaseCloudSaveService in prod) ──
            var cloudSaveMock = new CloudSaveServiceMock();
            ServiceLocator.Register<ICloudSaveService>(cloudSaveMock);

            var cloudSyncService = new CloudSyncService(
                ServiceLocator.Get<ICloudSaveService>(),
                ServiceLocator.Get<ISaveService>(),
                ServiceLocator.Get<IAuthService>(),
                ServiceLocator.Get<IConfigService>());
            ServiceLocator.Register<CloudSyncService>(cloudSyncService);

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

            var adConfig = Resources.Load<AdConfiguration>("AdConfiguration");
            if (adConfig == null)
                GameLogger.Warning("GameBootstrap", "AdConfiguration not found in Resources.");
            else
                ServiceLocator.Register<AdConfiguration>(adConfig);

            var adsMock = new AdsServiceMock();
            ServiceLocator.Register<IAdsService>(adsMock);
            ServiceLocator.Register<IRewardedAdService>(adsMock);

            var interstitialService = new AdMobInterstitialService(
                adsMock,
                adConfig,
                ServiceLocator.Get<IAdEntitlementService>(),
                ServiceLocator.Get<ISaveService>(),
                ServiceLocator.Get<IConfigService>());
            ServiceLocator.Register<IInterstitialAdService>(interstitialService);

            var rewardedAdFlowService = new RewardedAdFlowService(
                adsMock,
                ServiceLocator.Get<RewardService>(),
                ServiceLocator.Get<ISaveService>(),
                ServiceLocator.Get<IAdEntitlementService>(),
                ServiceLocator.Get<IConfigService>(),
                powerUpService,
                adConfig);
            ServiceLocator.Register<RewardedAdFlowService>(rewardedAdFlowService);

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
