using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KingSmash.Core;
using KingSmash.Progression;
using KingSmash.Services;
using KingSmash.Economy;

namespace KingSmash.UI.Screens
{
    public class UpgradeScreen : UIScreen
    {
        // ------------------------------------------------------------------
        // Serialized fields
        // ------------------------------------------------------------------

        [Header("King Status")]
        [SerializeField] private TextMeshProUGUI _kingLevelLabel;
        [SerializeField] private TextMeshProUGUI _kingXpLabel;
        [SerializeField] private Slider          _kingXpBar;

        [Header("Currency")]
        [SerializeField] private TextMeshProUGUI _coinsLabel;

        [Header("Stat Rows")]
        [SerializeField] private UpgradeStatRow _powerRow;
        [SerializeField] private UpgradeStatRow _speedRow;
        [SerializeField] private UpgradeStatRow _smashRow;
        [SerializeField] private UpgradeStatRow _armorRow;

        [Header("Config")]
        [SerializeField] private KingUpgradeConfig _upgradeConfig;

        [Header("Navigation")]
        [SerializeField] private Button     _backButton;

        [Header("Art")]
        [SerializeField] private GameObject _kingArtPlaceholder;

        // ------------------------------------------------------------------
        // Private state
        // ------------------------------------------------------------------

        private KingUpgradeService    _upgradeService;
        private KingProgressionService _progressionService;

        // ------------------------------------------------------------------
        // Unity lifecycle
        // ------------------------------------------------------------------

        protected override void Awake()
        {
            base.Awake();
            _backButton?.onClick.AddListener(OnBackClicked);

            _powerRow?.SetupButton(OnUpgradePower);
            _speedRow?.SetupButton(OnUpgradeSpeed);
            _smashRow?.SetupButton(OnUpgradeSmash);
            _armorRow?.SetupButton(OnUpgradeArmor);
        }

        private void OnEnable()
        {
            KingUpgradeService.OnStatUpgraded       += OnStatUpgradedHandler;
            KingProgressionService.OnKingLevelUp    += OnKingLevelUpHandler;
        }

        private void OnDisable()
        {
            KingUpgradeService.OnStatUpgraded       -= OnStatUpgradedHandler;
            KingProgressionService.OnKingLevelUp    -= OnKingLevelUpHandler;
        }

        private void OnDestroy()
        {
            _backButton?.onClick.RemoveListener(OnBackClicked);
        }

        // ------------------------------------------------------------------
        // UIScreen overrides
        // ------------------------------------------------------------------

        protected override void OnShow()
        {
            ResolveServices();

            if (ServiceLocator.TryGet<IAnalyticsService>(out var analytics))
                analytics.LogEvent(AnalyticsEvents.UpgradeScreenOpened);

            RefreshAll();
        }

        // ------------------------------------------------------------------
        // Event handlers
        // ------------------------------------------------------------------

        private void OnStatUpgradedHandler(KingStat stat, int oldLevel, int newLevel, float oldVal, float newVal, long cost)
        {
            RefreshAll();
        }

        private void OnKingLevelUpHandler(int oldLevel, int newLevel, KingStats stats)
        {
            RefreshKingLevelDisplay();
        }

        // ------------------------------------------------------------------
        // Upgrade button callbacks
        // ------------------------------------------------------------------

        private void OnUpgradePower() => TryUpgrade(KingStat.Power);
        private void OnUpgradeSpeed() => TryUpgrade(KingStat.Speed);
        private void OnUpgradeSmash() => TryUpgrade(KingStat.SmashRadius);
        private void OnUpgradeArmor() => TryUpgrade(KingStat.Armor);

        private void TryUpgrade(KingStat stat)
        {
            if (_upgradeService == null)
            {
                GameLogger.Warning("UpgradeScreen", "KingUpgradeService not available.");
                return;
            }

            var result = _upgradeService.TryUpgradeStat(stat);

            if (result != UpgradeResult.Success)
            {
                GameLogger.Warning("UpgradeScreen", $"Upgrade failed for {stat}: {result}");
                ShakeCoinsLabel();
            }
        }

        // ------------------------------------------------------------------
        // Refresh helpers
        // ------------------------------------------------------------------

        private void RefreshAll()
        {
            RefreshCoinsDisplay();
            RefreshKingLevelDisplay();
            RefreshStatRow(_powerRow, KingStat.Power,       "Power");
            RefreshStatRow(_speedRow, KingStat.Speed,       "Speed");
            RefreshStatRow(_smashRow, KingStat.SmashRadius, "Smash Radius");
            RefreshStatRow(_armorRow, KingStat.Armor,       "Armor");
        }

        private void RefreshCoinsDisplay()
        {
            if (!ServiceLocator.TryGet<ISaveService>(out var save)) return;
            if (_coinsLabel != null)
                _coinsLabel.text = save.Current.coins.ToString("N0");
        }

        private void RefreshKingLevelDisplay()
        {
            if (_progressionService == null) return;

            int  kingLevel = _progressionService.KingLevel;
            long currentXP = _progressionService.KingXP;
            long xpForNext = _progressionService.XPRequiredForNextLevel(kingLevel);
            bool isMaxLevel = kingLevel >= _progressionService.MaxKingLevel;

            if (_kingLevelLabel != null)
                _kingLevelLabel.text = $"King Lv {kingLevel}";

            if (isMaxLevel)
            {
                if (_kingXpLabel != null) _kingXpLabel.text = "MAX";
                if (_kingXpBar  != null) _kingXpBar.value   = 1f;
            }
            else
            {
                if (_kingXpLabel != null)
                    _kingXpLabel.text = $"{currentXP:N0} / {xpForNext:N0} XP";
                if (_kingXpBar != null)
                    _kingXpBar.value = xpForNext > 0
                        ? Mathf.Clamp01((float)currentXP / xpForNext)
                        : 0f;
            }
        }

        private void RefreshStatRow(UpgradeStatRow row, KingStat stat, string displayName)
        {
            if (row == null) return;
            if (_upgradeService == null)
            {
                // Fallback: show row in disabled state without preview data.
                row.Refresh(displayName, 1, 0, 0);
                return;
            }

            if (!ServiceLocator.TryGet<ISaveService>(out var save)) return;
            long playerCoins = save.Current.coins;

            var preview = _upgradeService.GetUpgradePreview(stat);
            row.Refresh(
                displayName,
                preview.currentLevel,
                preview.cost,
                playerCoins);
        }

        // ------------------------------------------------------------------
        // Coin shake feedback
        // ------------------------------------------------------------------

        private void ShakeCoinsLabel()
        {
            if (_coinsLabel == null) return;
            StartCoroutine(ShakeTransform(_coinsLabel.rectTransform, 6f, 0.4f));
        }

        private static IEnumerator ShakeTransform(RectTransform rt, float magnitude, float duration)
        {
            Vector2 originalPos = rt.anchoredPosition;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / duration;
                float dampen = 1f - t;
                float offsetX = Random.Range(-magnitude, magnitude) * dampen;
                float offsetY = Random.Range(-magnitude * 0.5f, magnitude * 0.5f) * dampen;
                rt.anchoredPosition = originalPos + new Vector2(offsetX, offsetY);
                yield return null;
            }

            rt.anchoredPosition = originalPos;
        }

        // ------------------------------------------------------------------
        // Navigation
        // ------------------------------------------------------------------

        private void OnBackClicked()
        {
            ScreenManager.Instance.Back();
        }

        // ------------------------------------------------------------------
        // Service resolution
        // ------------------------------------------------------------------

        private void ResolveServices()
        {
            ServiceLocator.TryGet<KingUpgradeService>(out _upgradeService);
            ServiceLocator.TryGet<KingProgressionService>(out _progressionService);

            if (_upgradeService == null)
                GameLogger.Warning("UpgradeScreen", "KingUpgradeService not registered in ServiceLocator.");
            if (_progressionService == null)
                GameLogger.Warning("UpgradeScreen", "KingProgressionService not registered in ServiceLocator.");
        }
    }
}
