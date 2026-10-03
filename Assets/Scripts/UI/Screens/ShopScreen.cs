using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KingSmash.Core;
using KingSmash.Services;
namespace KingSmash.UI.Screens
{
    [Serializable]
    public class ShopItem
    {
        public string productId;
        public string displayName;
        public string priceDisplay;
        public long coinsGranted;
        public int gemsGranted;
    }

    public class ShopScreen : UIScreen
    {
        [Header("Navigation")]
        [SerializeField] private Button _backButton;

        [Header("Tabs")]
        [SerializeField] private Button _coinsTab;
        [SerializeField] private Button _gemsTab;
        [SerializeField] private Button _specialTab;

        [Header("Currency")]
        [SerializeField] private TextMeshProUGUI _coinsLabel;
        [SerializeField] private TextMeshProUGUI _gemsLabel;

        [Header("Content")]
        [SerializeField] private GameObject _coinPanel;
        [SerializeField] private GameObject _gemPanel;
        [SerializeField] private GameObject _specialPanel;

        [Header("Shop Cards")]
        [SerializeField] private Transform _coinCardContainer;
        [SerializeField] private Transform _gemCardContainer;
        [SerializeField] private ShopCardWidget _shopCardPrefab;

        [Header("Special")]
        [SerializeField] private Button _removeAdsButton;
        [SerializeField] private TextMeshProUGUI _removeAdsPriceLabel;

        private readonly List<ShopItem> _coinItems = new()
        {
            new ShopItem { productId = "coins_1000",  displayName = "1,000 Coins",  priceDisplay = "₹99",  coinsGranted = 1000  },
            new ShopItem { productId = "coins_5000",  displayName = "5,000 Coins",  priceDisplay = "₹299", coinsGranted = 5000  },
            new ShopItem { productId = "coins_15000", displayName = "15,000 Coins", priceDisplay = "₹499", coinsGranted = 15000 },
            new ShopItem { productId = "coins_50000", displayName = "50,000 Coins", priceDisplay = "₹999", coinsGranted = 50000 },
        };

        private readonly List<ShopItem> _gemItems = new()
        {
            new ShopItem { productId = "gems_10",  displayName = "10 Gems",  priceDisplay = "₹99",  gemsGranted = 10  },
            new ShopItem { productId = "gems_50",  displayName = "50 Gems",  priceDisplay = "₹299", gemsGranted = 50  },
            new ShopItem { productId = "gems_100", displayName = "100 Gems", priceDisplay = "₹499", gemsGranted = 100 },
            new ShopItem { productId = "gems_500", displayName = "500 Gems", priceDisplay = "₹999", gemsGranted = 500 },
        };

        protected override void Awake()
        {
            base.Awake();
            _backButton?.onClick.AddListener(OnBackClicked);
            _coinsTab?.onClick.AddListener(() => ShowTab(0));
            _gemsTab?.onClick.AddListener(() => ShowTab(1));
            _specialTab?.onClick.AddListener(() => ShowTab(2));
            _removeAdsButton?.onClick.AddListener(OnRemoveAdsClicked);
        }

        protected override void OnShow()
        {
            RefreshCurrency();
            BuildShopCards();
            ShowTab(0);
            if (ServiceLocator.TryGet<IAnalyticsService>(out var analytics))
                analytics.LogEvent("shop_opened");
        }

        private void RefreshCurrency()
        {
            if (!ServiceLocator.TryGet<ISaveService>(out var save)) return;
            if (_coinsLabel != null) _coinsLabel.text = save.Current.coins.ToString("N0");
            if (_gemsLabel  != null) _gemsLabel.text  = save.Current.gems.ToString();
        }

        private void BuildShopCards()
        {
            if (_shopCardPrefab == null) return;
            BuildCards(_coinCardContainer, _coinItems, false);
            BuildCards(_gemCardContainer,  _gemItems,  true);
        }

        private void BuildCards(Transform container, List<ShopItem> items, bool isGems)
        {
            if (container == null) return;
            foreach (Transform child in container) Destroy(child.gameObject);
            foreach (var item in items)
            {
                var card = Instantiate(_shopCardPrefab, container);
                var capturedItem = item;
                card.Setup(item.displayName, item.priceDisplay, () => OnPurchaseClicked(capturedItem));
            }
        }

        private void ShowTab(int tab)
        {
            _coinPanel?.SetActive(tab == 0);
            _gemPanel?.SetActive(tab == 1);
            _specialPanel?.SetActive(tab == 2);
        }

        private async void OnPurchaseClicked(ShopItem item)
        {
            if (ServiceLocator.TryGet<IPurchaseService>(out var purchase))
                await purchase.PurchaseAsync(item.productId);
            if (ServiceLocator.TryGet<IAnalyticsService>(out var analytics))
                analytics.LogEvent(AnalyticsEvents.IapStarted, ("product_id", item.productId));
            GameLogger.Info("ShopScreen", $"Purchase initiated: {item.productId}");
        }

        private async void OnRemoveAdsClicked()
        {
            if (ServiceLocator.TryGet<IPurchaseService>(out var purchase))
                await purchase.PurchaseAsync("remove_ads");
            GameLogger.Info("ShopScreen", "Remove Ads purchase initiated.");
        }

        private void OnBackClicked() => ScreenManager.Instance.Back();

        private void OnDestroy()
        {
            _backButton?.onClick.RemoveListener(OnBackClicked);
            _removeAdsButton?.onClick.RemoveListener(OnRemoveAdsClicked);
        }
    }
}
