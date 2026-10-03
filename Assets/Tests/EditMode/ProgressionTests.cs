using NUnit.Framework;
using UnityEngine;
using KingSmash.Progression;
using KingSmash.Economy;
using KingSmash.Save;
using KingSmash.Services;
using KingSmash.Core;

namespace KingSmash.Tests.EditMode
{
    public class ProgressionTests
    {
        private EconomyConfig _economyConfig;
        private LocalSaveService _saveService;
        private ProgressionManager _progressionManager;

        [SetUp]
        public void SetUp()
        {
            ServiceLocator.Initialize();
            ServiceLocator.Register<IAnalyticsService>(new AnalyticsServiceMock());

            _economyConfig = ScriptableObject.CreateInstance<EconomyConfig>();
            _economyConfig.coinsPerStar = 25;
            _economyConfig.bonusCoinsThreeStars = 100;

            _saveService = new LocalSaveService();
            _saveService.Load();

            ServiceLocator.Register<ISaveService>(_saveService);
            _progressionManager = new ProgressionManager(_saveService, _economyConfig);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_economyConfig);
            _saveService.Delete();
        }

        [Test]
        public void HandleLevelComplete_AwardsCorrectCoins()
        {
            _progressionManager.HandleLevelComplete(0, 3);
            // 3 stars = 3*25 + 100 bonus = 175
            Assert.AreEqual(175, _saveService.Current.coins);
        }

        [Test]
        public void HandleLevelComplete_RecordsLevelInCompleted()
        {
            _progressionManager.HandleLevelComplete(5, 2);
            Assert.Contains(5, _saveService.Current.completedLevels);
        }

        [Test]
        public void HandleLevelComplete_AdvancesCurrentLevel()
        {
            _progressionManager.HandleLevelComplete(0, 1);
            Assert.AreEqual(1, _saveService.Current.currentLevel);
        }

        [Test]
        public void GetTotalPlayerStars_SumsAllLevelStars()
        {
            _saveService.RecordLevelComplete(0, 3);
            _saveService.RecordLevelComplete(1, 2);
            _saveService.RecordLevelComplete(2, 1);
            Assert.AreEqual(6, _progressionManager.GetTotalPlayerStars());
        }
    }
}
