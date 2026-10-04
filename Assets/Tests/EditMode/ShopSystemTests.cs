using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using KingSmash.Shop;
using KingSmash.Services;
using KingSmash.Save;
using KingSmash.Economy;
using KingSmash.Progression;
using KingSmash.Core;

namespace KingSmash.Tests.EditMode
{
    // =========================================================================
    // M8 local ISaveService mock — avoids clash with other test mocks
    // =========================================================================

    internal class M8TestSaveService : ISaveService
    {
        public SaveData Current { get; private set; }

        public M8TestSaveService()
        {
            Current = new SaveData
            {
                playerId         = "m8-test",
                kingProgression  = new KingProgression(),
                powerUpInventory = new PowerUpInventory(),
                ownedProducts    = new List<string>(),
            };
        }

        public void Load()   { }
        public void Save()   { }
        public void Delete() { Current = new SaveData { playerId = "m8-test", kingProgression = new KingProgression(), powerUpInventory = new PowerUpInventory(), ownedProducts = new List<string>() }; }

        public void AddCoins(long amount)   { if (amount > 0) Current.coins += amount; }
        public void SpendCoins(long amount) { if (Current.coins < amount) throw new InvalidOperationException("Insufficient coins."); Current.coins -= amount; }
        public bool TrySpendCoins(long amount) { if (Current.coins < amount) return false; SpendCoins(amount); return true; }
        public void AddGems(int amount)     { Current.gems += amount; }
        public void RecordLevelComplete(int levelIndex, int stars)
        {
            if (!Current.completedLevels.Contains(levelIndex)) Current.completedLevels.Add(levelIndex);
            int prev = Current.GetStarsForLevel(levelIndex);
            if (stars > prev) Current.SetStarsForLevel(levelIndex, stars);
        }
    }

    // =========================================================================
    // ShopProductCatalogTests
    // =========================================================================

    [TestFixture]
    public class ShopProductCatalogTests
    {
        private ShopProductCatalog _catalog;

        [SetUp]
        public void SetUp()
        {
            _catalog = ScriptableObject.CreateInstance<ShopProductCatalog>();
            _catalog.InitializeDefaults();
        }

        [TearDown]
        public void TearDown() => UnityEngine.Object.DestroyImmediate(_catalog);

        [Test]
        public void GetProduct_KnownId_ReturnsProduct()
        {
            var product = _catalog.GetProduct("coins_1000");
            Assert.IsNotNull(product);
            Assert.AreEqual("coins_1000", product.ProductId);
        }

        [Test]
        public void GetProduct_Unknown_ReturnsNull()
        {
            Assert.IsNull(_catalog.GetProduct("nonexistent_product_xyz"));
        }

        [Test]
        public void GetByType_CoinPack_ReturnsFourProducts()
        {
            Assert.AreEqual(4, _catalog.GetByType(ShopProductType.CoinPack).Count);
        }

        [Test]
        public void GetAll_ReturnsMoreThanZeroProducts()
        {
            Assert.Greater(_catalog.GetAll().Count, 0);
        }

        [Test]
        public void CoinProducts_ReturnsFour()
        {
            Assert.AreEqual(4, _catalog.CoinProducts.Count);
        }

        [Test]
        public void GemProducts_ReturnsFour()
        {
            Assert.AreEqual(4, _catalog.GemProducts.Count);
        }

        [Test]
        public void StarterBundle_HasCorrectPowerUpIds()
        {
            var bundle = _catalog.GetProduct("starter_bundle");
            Assert.IsNotNull(bundle);
            Assert.IsTrue(bundle.powerUpRewards.Count > 0);
            Assert.AreEqual("powerup_bomb", bundle.powerUpRewards[0].powerUpTypeId);
        }

        [Test]
        public void KingBundle_HasCorrectPowerUpIds()
        {
            var bundle = _catalog.GetProduct("king_bundle");
            Assert.IsNotNull(bundle);
            Assert.IsTrue(bundle.powerUpRewards.Count > 0);
            Assert.AreEqual("powerup_megaking", bundle.powerUpRewards[0].powerUpTypeId);
        }
    }

    // =========================================================================
    // PurchaseStateTrackerTests
    // =========================================================================

    [TestFixture]
    public class PurchaseStateTrackerTests
    {
        private PurchaseStateTracker _tracker;
        private PurchaseState _lastState;
        private string _lastProductId;

        [SetUp]
        public void SetUp()
        {
            _tracker = new PurchaseStateTracker();
            PurchaseStateTracker.OnStateChanged += CaptureState;
        }

        [TearDown]
        public void TearDown()
        {
            PurchaseStateTracker.OnStateChanged -= CaptureState;
        }

        private void CaptureState(string productId, PurchaseState state)
        {
            _lastProductId = productId;
            _lastState = state;
        }

        [Test]
        public void GetState_Default_IsIdle()
        {
            Assert.AreEqual(PurchaseState.Idle, _tracker.GetState("coins_1000"));
        }

        [Test]
        public void SetState_FiresEvent()
        {
            _tracker.SetState("coins_1000", PurchaseState.Purchasing);
            Assert.AreEqual("coins_1000", _lastProductId);
            Assert.AreEqual(PurchaseState.Purchasing, _lastState);
        }

        [Test]
        public void SetState_UpdatesState()
        {
            _tracker.SetState("coins_1000", PurchaseState.Purchasing);
            Assert.AreEqual(PurchaseState.Purchasing, _tracker.GetState("coins_1000"));
        }

        [Test]
        public void MultiProduct_StatesAreIndependent()
        {
            _tracker.SetState("coins_1000", PurchaseState.Purchasing);
            _tracker.SetState("gems_10",    PurchaseState.Success);

            Assert.AreEqual(PurchaseState.Purchasing, _tracker.GetState("coins_1000"));
            Assert.AreEqual(PurchaseState.Success,    _tracker.GetState("gems_10"));
            Assert.AreEqual(PurchaseState.Idle,       _tracker.GetState("remove_ads"));
        }
    }

    // =========================================================================
    // EntitlementServiceTests
    // =========================================================================

    [TestFixture]
    public class EntitlementServiceTests
    {
        private EntitlementService _service;
        private M8TestSaveService  _save;

        [SetUp]
        public void SetUp()
        {
            _save    = new M8TestSaveService();
            _service = new EntitlementService(_save);
        }

        [Test]
        public void HasEntitlement_False_Initially()
        {
            Assert.IsFalse(_service.HasEntitlement("remove_ads"));
        }

        [Test]
        public void GrantEntitlement_Persists()
        {
            _service.GrantEntitlement("remove_ads");
            Assert.IsTrue(_service.HasEntitlement("remove_ads"));
            Assert.Contains("remove_ads", _save.Current.ownedProducts);
        }

        [Test]
        public void RevokeEntitlement_Removes()
        {
            _service.GrantEntitlement("remove_ads");
            _service.RevokeEntitlement("remove_ads");
            Assert.IsFalse(_service.HasEntitlement("remove_ads"));
            Assert.IsFalse(_save.Current.ownedProducts.Contains("remove_ads"));
        }

        [Test]
        public void CanShowAds_True_WhenNoEntitlement()
        {
            Assert.IsTrue(_service.CanShowAds());
        }

        [Test]
        public void CanShowAds_False_WhenRemoveAdsGranted()
        {
            _service.GrantEntitlement(EntitlementService.RemoveAdsProductId);
            Assert.IsFalse(_service.CanShowAds());
        }
    }

    // =========================================================================
    // ReceiptVerificationServiceMockTests
    // =========================================================================

    [TestFixture]
    public class ReceiptVerificationServiceMockTests
    {
        private ReceiptVerificationServiceMock _verifier;

        [SetUp]
        public void SetUp() => _verifier = new ReceiptVerificationServiceMock();

        [Test]
        public async Task Verify_ValidProduct_ReturnsValid()
        {
            var result = await _verifier.VerifyAsync("coins_1000", "fake_receipt_token_valid");
            Assert.IsTrue(result.IsValid);
        }

        [Test]
        public async Task Verify_InvalidToken_ReturnsInvalid()
        {
            var result = await _verifier.VerifyAsync("coins_1000", ReceiptVerificationServiceMock.InvalidReceiptToken);
            Assert.IsFalse(result.IsValid);
        }

        [Test]
        public async Task Verify_CoinsProduct_ReturnsCorrectAmount()
        {
            var result = await _verifier.VerifyAsync("coins_1000", "fake_receipt_token_valid");
            Assert.IsTrue(result.IsValid);
            Assert.AreEqual(1000L, result.CoinsGranted);
        }
    }

    // =========================================================================
    // ShopServiceTests
    // =========================================================================

    [TestFixture]
    public class ShopServiceTests
    {
        private ShopService _shopService;
        private M8TestSaveService _save;
        private StoreServiceMock _store;
        private ReceiptVerificationServiceMock _verifier;
        private EntitlementService _entitlement;
        private CurrencyService _currency;

        [SetUp]
        public void SetUp()
        {
            ServiceLocator.Initialize();

            _save        = new M8TestSaveService();
            _store       = new StoreServiceMock();
            _verifier    = new ReceiptVerificationServiceMock();
            _currency    = new CurrencyService(_save);
            _entitlement = new EntitlementService(_save);

            ServiceLocator.Register<ISaveService>(_save);
            ServiceLocator.Register<CurrencyService>(_currency);

            var catalog = ScriptableObject.CreateInstance<ShopProductCatalog>();
            catalog.InitializeDefaults();

            _shopService = new ShopService(_store, _verifier, _entitlement, _currency, catalog);
        }

        [TearDown]
        public void TearDown() => ServiceLocator.Initialize();

        [Test]
        public async Task InitAsync_CompletesSuccessfully()
        {
            await _shopService.InitAsync();
            Assert.IsTrue(_shopService.IsInitialized);
        }

        [Test]
        public async Task Purchase_ValidConsumable_ReturnsSuccess()
        {
            await _shopService.InitAsync();
            var result = await _shopService.PurchaseAsync("coins_1000");
            Assert.IsTrue(result.Success);
            Assert.AreEqual("coins_1000", result.ProductId);
        }

        [Test]
        public async Task Purchase_CancelledByUser_ReturnsCancelled()
        {
            await _shopService.InitAsync();
            _store.SimulateNextPurchaseOutcome(StorePurchaseOutcome.UserCancelled);
            var result = await _shopService.PurchaseAsync("coins_1000");
            Assert.IsFalse(result.Success);
            Assert.AreEqual(ShopPurchaseError.UserCancelled, result.Error);
        }

        [Test]
        public async Task Purchase_VerificationFailed_ReturnsFailed()
        {
            await _shopService.InitAsync();
            _verifier.SimulateNextVerificationOutcome(false);
            var result = await _shopService.PurchaseAsync("coins_1000");
            Assert.IsFalse(result.Success);
            Assert.AreEqual(ShopPurchaseError.VerificationFailed, result.Error);
        }

        [Test]
        public async Task Purchase_NonConsumable_GrantsEntitlement()
        {
            await _shopService.InitAsync();
            var result = await _shopService.PurchaseAsync("remove_ads");
            Assert.IsTrue(result.Success);
            Assert.IsTrue(_entitlement.HasEntitlement("remove_ads"));
        }

        [Test]
        public async Task GetLocalizedPrice_DelegatesToStore()
        {
            await _shopService.InitAsync();
            _store.SetLocalizedPrice("coins_1000", "₹99");
            Assert.AreEqual("₹99", _shopService.GetLocalizedPrice("coins_1000"));
        }

        [Test]
        public async Task Purchase_CoinsGranted_UpdatesBalance()
        {
            await _shopService.InitAsync();
            long before = _save.Current.coins;
            await _shopService.PurchaseAsync("coins_1000");
            Assert.Greater(_save.Current.coins, before);
        }
    }

    // =========================================================================
    // SaveDataOwnedProductsTests
    // =========================================================================

    [TestFixture]
    public class SaveDataOwnedProductsTests
    {
        [Test]
        public void OwnedProducts_DefaultsToEmptyList()
        {
            var data = new SaveData();
            Assert.IsNotNull(data.ownedProducts);
            Assert.AreEqual(0, data.ownedProducts.Count);
        }

        [Test]
        public void GrantEntitlement_AddsToOwnedProducts()
        {
            var save    = new M8TestSaveService();
            var service = new EntitlementService(save);
            service.GrantEntitlement("remove_ads");
            Assert.AreEqual(1, save.Current.ownedProducts.Count);
            Assert.AreEqual("remove_ads", save.Current.ownedProducts[0]);
        }

        [Test]
        public void OwnedProducts_CanHoldMultipleEntries()
        {
            var save    = new M8TestSaveService();
            var service = new EntitlementService(save);
            service.GrantEntitlement("remove_ads");
            service.GrantEntitlement("starter_pack");
            service.GrantEntitlement("vip_pass");
            Assert.AreEqual(3, save.Current.ownedProducts.Count);
        }
    }
}
