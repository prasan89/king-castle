using UnityEngine;
using KingSmash.Services;
using KingSmash.Core;
using KingSmash.Economy;
using KingSmash.Progression;
using KingSmash.PowerUps;
using KingSmash.Shop;

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

            ServiceLocator.Initialize();
            RegisterServices();
            ServiceLocator.Get<ISaveService>().Load();
            ServiceLocator.Get<IConfigService>().Initialize();

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
            ServiceLocator.Register<IAdsService>(new AdsServiceMock());
            ServiceLocator.Register<IAudioService>(new AudioManager());

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
        }
    }
}
