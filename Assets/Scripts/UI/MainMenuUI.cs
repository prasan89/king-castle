using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KingSmash.Core;
using KingSmash.Services;

namespace KingSmash.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _settingsButton;

        [Header("Currency Display")]
        [SerializeField] private TextMeshProUGUI _coinsText;
        [SerializeField] private TextMeshProUGUI _gemsText;

        [Header("Logo")]
        [SerializeField] private GameObject _logoPlaceholder;

        private void Awake()
        {
            _playButton.onClick.AddListener(OnPlayClicked);
            _settingsButton.onClick.AddListener(OnSettingsClicked);
        }

        private void Start()
        {
            RefreshCurrencyDisplay();
            ServiceLocator.Get<IAnalyticsService>().LogEvent(AnalyticsEvents.MainMenuOpened);
            GameManager.Instance?.TransitionTo(GameState.MainMenu);
        }

        private void RefreshCurrencyDisplay()
        {
            if (!ServiceLocator.TryGet<ISaveService>(out var save)) return;
            if (_coinsText != null) _coinsText.text = FormatCoins(save.Current.coins);
            if (_gemsText != null) _gemsText.text = save.Current.gems.ToString();
        }

        private void OnPlayClicked()
        {
            GameLogger.Info("MainMenuUI", "Play clicked.");
            SceneLoader.LoadScene(SceneNames.Level);
        }

        private void OnSettingsClicked()
        {
            GameLogger.Info("MainMenuUI", "Settings clicked.");
            ServiceLocator.Get<IAnalyticsService>().LogEvent(AnalyticsEvents.SettingsOpened);
            // Settings panel will be implemented in M2 UI milestone
        }

        private static string FormatCoins(long coins)
        {
            if (coins >= 1_000_000) return $"{coins / 1_000_000f:F1}M";
            if (coins >= 1_000) return $"{coins / 1_000f:F1}K";
            return coins.ToString();
        }

        private void OnDestroy()
        {
            _playButton.onClick.RemoveListener(OnPlayClicked);
            _settingsButton.onClick.RemoveListener(OnSettingsClicked);
        }
    }
}
