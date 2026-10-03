using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KingSmash.Core;
using KingSmash.Characters;
using KingSmash.Economy;
using KingSmash.Services;
namespace KingSmash.UI.Screens
{
    public class UpgradeScreen : UIScreen
    {
        [Header("Navigation")]
        [SerializeField] private Button _backButton;

        [Header("Character Art")]
        [SerializeField] private GameObject _kingArtPlaceholder;

        [Header("Currency")]
        [SerializeField] private TextMeshProUGUI _coinsLabel;

        [Header("Stat Rows")]
        [SerializeField] private UpgradeStatRow _powerRow;
        [SerializeField] private UpgradeStatRow _speedRow;
        [SerializeField] private UpgradeStatRow _smashRow;
        [SerializeField] private UpgradeStatRow _armorRow;

        [Header("Config")]
        [SerializeField] private KingConfig _kingConfig;
        [SerializeField] private EconomyConfig _economyConfig;

        protected override void Awake()
        {
            base.Awake();
            _backButton?.onClick.AddListener(OnBackClicked);

            _powerRow?.SetupButton(OnUpgradePower);
            _speedRow?.SetupButton(OnUpgradeSpeed);
            _smashRow?.SetupButton(OnUpgradeSmash);
            _armorRow?.SetupButton(OnUpgradeArmor);
        }

        protected override void OnShow()
        {
            ServiceLocator.TryGet<IAnalyticsService>(out var analytics);
            analytics?.LogEvent(AnalyticsEvents.SettingsOpened); // reuse settings event as upgrade view
            RefreshAll();
        }

        private void RefreshAll()
        {
            if (!ServiceLocator.TryGet<ISaveService>(out var save)) return;
            var data = save.Current;

            if (_coinsLabel != null) _coinsLabel.text = data.coins.ToString("N0");

            int kingLv = data.kingLevel;

            // Power
            var powerTier = GetTier(_economyConfig?.kingPowerUpgrades, kingLv);
            _powerRow?.Refresh("Power", kingLv, powerTier?.coinCost ?? 800, data.coins);

            // Speed
            var speedTier = GetTier(_economyConfig?.kingSpeedUpgrades, kingLv);
            _speedRow?.Refresh("Speed", kingLv, speedTier?.coinCost ?? 600, data.coins);

            // Smash Radius
            var smashTier = GetTier(_economyConfig?.kingSmashUpgrades, kingLv);
            _smashRow?.Refresh("Smash Radius", kingLv, smashTier?.coinCost ?? 400, data.coins);

            // Armor
            var armorTier = GetTier(_economyConfig?.kingArmorUpgrades, kingLv);
            _armorRow?.Refresh("Armor", kingLv, armorTier?.coinCost ?? 400, data.coins);
        }

        private UpgradeTier GetTier(List<UpgradeTier> tiers, int currentLevel)
        {
            if (tiers == null) return null;
            foreach (var t in tiers) if (t.level == currentLevel + 1) return t;
            return null;
        }

        private void OnUpgradePower()  => TryUpgrade(_economyConfig?.kingPowerUpgrades,  "power");
        private void OnUpgradeSpeed()  => TryUpgrade(_economyConfig?.kingSpeedUpgrades,  "speed");
        private void OnUpgradeSmash()  => TryUpgrade(_economyConfig?.kingSmashUpgrades,  "smash");
        private void OnUpgradeArmor()  => TryUpgrade(_economyConfig?.kingArmorUpgrades,  "armor");

        private void TryUpgrade(List<UpgradeTier> tiers, string stat)
        {
            if (!ServiceLocator.TryGet<ISaveService>(out var save)) return;
            var data = save.Current;
            var tier = GetTier(tiers, data.kingLevel);
            if (tier == null || data.coins < tier.coinCost)
            {
                GameLogger.Warning("UpgradeScreen", $"Cannot upgrade {stat}: insufficient coins or max level.");
                return;
            }
            data.coins -= tier.coinCost;
            data.kingLevel++;
            save.Save();
            if (ServiceLocator.TryGet<IAnalyticsService>(out var analytics))
                analytics.LogEvent(AnalyticsEvents.UpgradePurchase, ("stat", stat), ("new_level", data.kingLevel));
            RefreshAll();
        }

        private void OnBackClicked() => ScreenManager.Instance.Back();
        private void OnDestroy()     => _backButton?.onClick.RemoveListener(OnBackClicked);
    }
}
