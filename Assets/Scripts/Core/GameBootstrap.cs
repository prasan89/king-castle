using UnityEngine;
using KingSmash.Services;
using KingSmash.Core;

namespace KingSmash.Core
{
    /// Initializes all game systems in dependency order before any scene loads.
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
        }
    }
}
