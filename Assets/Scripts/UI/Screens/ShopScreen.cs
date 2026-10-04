using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KingSmash.Shop;
using KingSmash.Economy;
using KingSmash.Services;
using KingSmash.Core;

namespace KingSmash.UI.Screens
{
    public class ShopScreen : UIScreen
    {
        [Header("Navigation")]
        [SerializeField] private Button _backButton;

        [Header("Tabs")]
        [SerializeField] private Button _coinsTabButton;
        [SerializeField] private Button _gemsTabButton;
        [SerializeField] private Button _specialTabButton;

        [Header("Tab Indicators")]
        [SerializeField] private GameObject _coinsTabActive;
        [SerializeField] private GameObject _gemsTabActive;
        [SerializeField] private GameObject _specialTabActive;

        [Header("Currency Header")]
        [SerializeField] private TextMeshProUGUI _coinsHeaderLabel;
        [SerializeField] private TextMeshProUGUI _gemsHeaderLabel;

        [Header("Panels")]
        [SerializeField] private GameObject _coinsPanel;
        [SerializeField] private GameObject _gemsPanel;
        [SerializeField] private GameObject _specialPanel;

        [Header("Card Containers")]
        [SerializeField] private Transform _coinCardContainer;
        [SerializeField] private Transform _gemCardContainer;
        [SerializeField] private Transform _specialCardContainer;

        [Header("Prefab")]
        [SerializeField] private KingSmash.UI.Widgets.ShopCardWidget _cardPrefab;

        [Header("Overlays")]
        [SerializeField] private GameObject _loadingOverlay;

        [Header("Restore")]
        [SerializeField] private Button _restorePurchasesButton;

        [Header("Catalog")]
        [SerializeField] private ShopProductCatalog _catalog;

        [Header("Modals")]
        [SerializeField] private PurchaseSuccessModal _successModal;
        [SerializeField] private PurchaseFailureModal _failureModal;

        private ShopService _shopService;
        private readonly List<KingSmash.UI.Widgets.ShopCardWidget> _spawnedCards = new();

        protected override void Awake()
        {
            base.Awake();
            _backButton?.onClick.AddListener(OnBackClicked);
            _coinsTabButton?.onClick.AddListener(OnCoinsTabClicked);
            _gemsTabButton?.onClick.AddListener(OnGemsTabClicked);
            _specialTabButton?.onClick.AddListener(OnSpecialTabClicked);
            _restorePurchasesButton?.onClick.AddListener(OnRestoreClicked);
        }

        protected override void OnShow()
        {
            ServiceLocator.TryGet<ShopService>(out _shopService);
            RefreshCurrencyHeader();
            ShowTab(0);
            CurrencyService.OnCoinsChanged += OnCoinsChangedHandler;

            if (_shopService != null && !_shopService.IsInitialized)
            {
                if (_loadingOverlay != null) _loadingOverlay.SetActive(true);
                InitializeShopAsync();
            }
            else
            {
                BuildAllCards();
            }

            if (ServiceLocator.TryGet<IAnalyticsService>(out var analytics))
                analytics.LogEvent(AnalyticsEvents.ShopOpened);
        }

        protected override void OnHide()
        {
            CurrencyService.OnCoinsChanged -= OnCoinsChangedHandler;
        }

        private async void InitializeShopAsync()
        {
            if (_shopService != null)
                await _shopService.InitializeAsync();
            if (_loadingOverlay != null) _loadingOverlay.SetActive(false);
            BuildAllCards();
        }

        private void BuildAllCards()
        {
            ClearCards();
            if (_catalog == null || _cardPrefab == null) return;

            foreach (var product in _catalog.CoinProducts)
                SpawnCard(product, _coinCardContainer);
            foreach (var product in _catalog.GemProducts)
                SpawnCard(product, _gemCardContainer);
            foreach (var product in _catalog.SpecialProducts)
                SpawnCard(product, _specialCardContainer);
        }

        private void SpawnCard(ShopProduct product, Transform container)
        {
            if (container == null) return;
            string localizedPrice = _shopService != null
                ? _shopService.GetLocalizedPrice(product.ProductId)
                : product.FallbackPrice;

            var card = Instantiate(_cardPrefab, container);
            card.Setup(product, localizedPrice, OnCardPurchaseClicked);
            _spawnedCards.Add(card);
        }

        private void ClearCards()
        {
            _spawnedCards.Clear();
            ClearContainer(_coinCardContainer);
            ClearContainer(_gemCardContainer);
            ClearContainer(_specialCardContainer);
        }

        private static void ClearContainer(Transform container)
        {
            if (container == null) return;
            foreach (Transform child in container)
                Destroy(child.gameObject);
        }

        private void ShowTab(int tab)
        {
            _coinsPanel?.SetActive(tab == 0);
            _gemsPanel?.SetActive(tab == 1);
            _specialPanel?.SetActive(tab == 2);

            _coinsTabActive?.SetActive(tab == 0);
            _gemsTabActive?.SetActive(tab == 1);
            _specialTabActive?.SetActive(tab == 2);
        }

        private async void OnCardPurchaseClicked(string productId)
        {
            if (_shopService == null)
            {
                GameLogger.Warning("ShopScreen", "ShopService not available.");
                return;
            }

            if (ServiceLocator.TryGet<IAnalyticsService>(out var analytics))
                analytics.LogEvent(AnalyticsEvents.PurchaseStarted, ("product_id", (object)productId));

            var result = await _shopService.PurchaseAsync(productId);

            if (result.IsSuccess)
            {
                RefreshCurrencyHeader();
                _successModal?.Show(result);

                if (ServiceLocator.TryGet<IAnalyticsService>(out var analyticsOk))
                    analyticsOk.LogEvent(AnalyticsEvents.PurchaseCompleted, ("product_id", (object)productId));
            }
            else
            {
                _failureModal?.Show(result);

                if (ServiceLocator.TryGet<IAnalyticsService>(out var analyticsFail))
                {
                    string evt = result.IsCancelled ? AnalyticsEvents.PurchaseCancelled : AnalyticsEvents.PurchaseFailed;
                    analyticsFail.LogEvent(evt, ("product_id", (object)productId));
                }
            }
        }

        private async void OnRestoreClicked()
        {
            if (_shopService == null) return;
            bool restored = await _shopService.RestoreAsync();
            if (restored)
            {
                if (ServiceLocator.TryGet<IAnalyticsService>(out var analytics))
                    analytics.LogEvent(AnalyticsEvents.PurchaseRestored);
                BuildAllCards();
            }
        }

        private void RefreshCurrencyHeader()
        {
            if (!ServiceLocator.TryGet<ISaveService>(out var save)) return;
            if (_coinsHeaderLabel != null)
                _coinsHeaderLabel.text = CurrencyFormatter.Format(save.Current.coins);
            if (_gemsHeaderLabel != null)
                _gemsHeaderLabel.text = save.Current.gems.ToString();
        }

        private void OnCoinsChangedHandler(long newBalance, CurrencyTransaction tx) => RefreshCurrencyHeader();

        private void OnCoinsTabClicked()   => ShowTab(0);
        private void OnGemsTabClicked()    => ShowTab(1);
        private void OnSpecialTabClicked() => ShowTab(2);
        private void OnBackClicked()       => ScreenManager.Instance.Back();

        private void OnDestroy()
        {
            _backButton?.onClick.RemoveListener(OnBackClicked);
            _coinsTabButton?.onClick.RemoveListener(OnCoinsTabClicked);
            _gemsTabButton?.onClick.RemoveListener(OnGemsTabClicked);
            _specialTabButton?.onClick.RemoveListener(OnSpecialTabClicked);
            _restorePurchasesButton?.onClick.RemoveListener(OnRestoreClicked);
        }
    }
}
