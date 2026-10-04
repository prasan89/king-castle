using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KingSmash.Shop;
using KingSmash.Economy;
using KingSmash.Services;
using KingSmash.Core;
using KingSmash.Audio;
using KingSmash.UI.Components;

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

        [Header("M13 Theme & Animation")]
        [SerializeField] private KingSmashTheme _themeConfig;
        [SerializeField] private CanvasGroup _screenCg;
        [SerializeField] private RectTransform _headerPanel;
        [SerializeField] private RectTransform _tabsPanel;
        [SerializeField] private KSCurrencyDisplay _coinsDisplay;
        [SerializeField] private KSCurrencyDisplay _gemsDisplay;

        private ShopService _shopService;
        private readonly List<KingSmash.UI.Widgets.ShopCardWidget> _spawnedCards = new();
        private int _activeTab = 0;
        private Coroutine _spinnerCoroutine;

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

            if (ServiceLocator.TryGet<IAnalyticsService>(out var analytics))
                analytics.LogEvent(AnalyticsEvents.ShopOpened);

            StartCoroutine(PlayShowAnimation());

            if (_shopService != null && !_shopService.IsInitialized)
            {
                SetLoadingOverlay(true);
                InitializeShopAsync();
            }
            else
            {
                BuildAllCards();
                StartCoroutine(StaggerRevealCards());
            }
        }

        protected override void OnHide()
        {
            CurrencyService.OnCoinsChanged -= OnCoinsChangedHandler;
            if (_spinnerCoroutine != null) StopCoroutine(_spinnerCoroutine);
        }

        // ── Entry Animations ────────────────────────────────────────────

        private IEnumerator PlayShowAnimation()
        {
            if (_screenCg != null)
            {
                _screenCg.alpha = 0f;
                yield return UIAnimationController.Fade(_screenCg, 0f, 1f, 0.2f);
            }

            if (_headerPanel != null)
                StartCoroutine(UIAnimationController.SlideIn(_headerPanel, 50f, 0.25f));

            if (_tabsPanel != null)
            {
                Vector2 orig = _tabsPanel.anchoredPosition;
                _tabsPanel.anchoredPosition = orig + Vector2.up * 60f;
                yield return UIAnimationController.SlideIn(_tabsPanel, -60f, 0.28f);
            }
        }

        private IEnumerator StaggerRevealCards()
        {
            yield return null;
            var containers = new[] { _coinCardContainer, _gemCardContainer, _specialCardContainer };
            int cardIndex = 0;
            foreach (var container in containers)
            {
                if (container == null) continue;
                foreach (Transform child in container)
                {
                    int i = cardIndex++;
                    StartCoroutine(DelayedBounceReveal(child, i * 0.05f));
                }
            }
        }

        private static IEnumerator DelayedBounceReveal(Transform target, float delay)
        {
            if (delay > 0f) yield return new WaitForSecondsRealtime(delay);
            yield return UIAnimationController.BounceReveal(target, 0.30f);
        }

        // ── Tab Management ──────────────────────────────────────────────

        private void ShowTab(int tab)
        {
            int prev = _activeTab;
            _activeTab = tab;

            SetTabVisual(_coinsTabButton,   tab == 0);
            SetTabVisual(_gemsTabButton,    tab == 1);
            SetTabVisual(_specialTabButton, tab == 2);

            _coinsTabActive?.SetActive(tab == 0);
            _gemsTabActive?.SetActive(tab == 1);
            _specialTabActive?.SetActive(tab == 2);

            if (prev != tab)
            {
                GameObject prevPanel = prev == 0 ? _coinsPanel   : prev == 1 ? _gemsPanel   : _specialPanel;
                GameObject nextPanel = tab == 0  ? _coinsPanel   : tab == 1  ? _gemsPanel   : _specialPanel;
                StartCoroutine(CrossFadePanels(prevPanel, nextPanel, 0.15f));
            }
            else
            {
                _coinsPanel?.SetActive(tab == 0);
                _gemsPanel?.SetActive(tab == 1);
                _specialPanel?.SetActive(tab == 2);
            }
        }

        private static void SetTabVisual(Button btn, bool active)
        {
            if (btn == null) return;
            btn.transform.localScale = active ? Vector3.one : Vector3.one * 0.95f;
        }

        private static IEnumerator CrossFadePanels(GameObject fromPanel, GameObject toPanel, float duration)
        {
            if (fromPanel != null)
            {
                var cgFrom = fromPanel.GetComponent<CanvasGroup>();
                if (cgFrom == null) cgFrom = fromPanel.AddComponent<CanvasGroup>();
                yield return UIAnimationController.Fade(cgFrom, 1f, 0f, duration);
                fromPanel.SetActive(false);
            }
            if (toPanel != null)
            {
                toPanel.SetActive(true);
                var cgTo = toPanel.GetComponent<CanvasGroup>();
                if (cgTo == null) cgTo = toPanel.AddComponent<CanvasGroup>();
                yield return UIAnimationController.Fade(cgTo, 0f, 1f, duration);
            }
        }

        // ── Shop Initialization & Cards ─────────────────────────────────────

        private async void InitializeShopAsync()
        {
            if (_shopService != null)
                await _shopService.InitializeAsync();
            SetLoadingOverlay(false);
            BuildAllCards();
            StartCoroutine(StaggerRevealCards());
        }

        private void SetLoadingOverlay(bool show)
        {
            if (_loadingOverlay != null)
            {
                _loadingOverlay.SetActive(show);
                if (show)
                    _spinnerCoroutine = StartCoroutine(SpinLoadingOverlay());
                else if (_spinnerCoroutine != null)
                {
                    StopCoroutine(_spinnerCoroutine);
                    _spinnerCoroutine = null;
                }
            }
        }

        private IEnumerator SpinLoadingOverlay()
        {
            if (_loadingOverlay == null) yield break;
            while (true)
            {
                _loadingOverlay.transform.Rotate(0f, 0f, -360f * Time.unscaledDeltaTime);
                yield return null;
            }
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

        // ── Purchase Flow ───────────────────────────────────────────────

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

                if (result.CoinsGranted > 0)
                    ToastService.ShowCoin(result.CoinsGranted);
                else if (result.GemsGranted > 0)
                    ToastService.ShowGem(result.GemsGranted);

                if (ServiceLocator.TryGet<IAudioService>(out var audioOk))
                    audioOk.Play(SoundId.Purchase);

                if (ServiceLocator.TryGet<IAnalyticsService>(out var analyticsOk))
                    analyticsOk.LogEvent(AnalyticsEvents.PurchaseCompleted, ("product_id", (object)productId));
            }
            else
            {
                _failureModal?.Show(result);
                ToastService.ShowError("Purchase failed. Please try again.");

                if (ServiceLocator.TryGet<IAudioService>(out var audioFail))
                    audioFail.Play(SoundId.Back);

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

        // ── Currency Header ─────────────────────────────────────────────

        private void RefreshCurrencyHeader()
        {
            if (!ServiceLocator.TryGet<ISaveService>(out var save)) return;
            if (_coinsHeaderLabel != null)
                _coinsHeaderLabel.text = CurrencyFormatter.Format(save.Current.coins);
            if (_gemsHeaderLabel != null)
                _gemsHeaderLabel.text = save.Current.gems.ToString();
            _coinsDisplay?.SetAmount(save.Current.coins);
            _gemsDisplay?.SetAmount(save.Current.gems);
        }

        private void OnCoinsChangedHandler(long newBalance, CurrencyTransaction tx) => RefreshCurrencyHeader();

        // ── Tab Callbacks ──────────────────────────────────────────────

        private void OnCoinsTabClicked()
        {
            if (ServiceLocator.TryGet<IAudioService>(out var audio)) audio.Play(SoundId.ButtonClick);
            ShowTab(0);
        }

        private void OnGemsTabClicked()
        {
            if (ServiceLocator.TryGet<IAudioService>(out var audio)) audio.Play(SoundId.ButtonClick);
            ShowTab(1);
        }

        private void OnSpecialTabClicked()
        {
            if (ServiceLocator.TryGet<IAudioService>(out var audio)) audio.Play(SoundId.ButtonClick);
            ShowTab(2);
        }

        private void OnBackClicked()
        {
            if (ServiceLocator.TryGet<IAudioService>(out var audio)) audio.Play(SoundId.Back);
            ScreenManager.Instance.Back();
        }

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
