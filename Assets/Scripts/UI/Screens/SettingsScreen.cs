using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KingSmash.Core;
using KingSmash.Services;
namespace KingSmash.UI.Screens
{
    public class SettingsScreen : UIScreen
    {
        [Header("Navigation")]
        [SerializeField] private Button _backButton;

        [Header("Audio Toggles")]
        [SerializeField] private Toggle _musicToggle;
        [SerializeField] private Toggle _sfxToggle;
        [SerializeField] private Toggle _vibrationToggle;

        [Header("Graphics")]
        [SerializeField] private TextMeshProUGUI _graphicsValueLabel;
        [SerializeField] private Button _graphicsCycleButton;

        [Header("Language")]
        [SerializeField] private TextMeshProUGUI _languageLabel;

        [Header("Legal")]
        [SerializeField] private Button _privacyPolicyButton;
        [SerializeField] private Button _termsButton;
        [SerializeField] private Button _restorePurchasesButton;

        private readonly string[] _graphicsOptions = { "High", "Medium", "Low" };
        private int _graphicsIndex = 0;

        protected override void Awake()
        {
            base.Awake();
            _backButton?.onClick.AddListener(OnBackClicked);
            _musicToggle?.onValueChanged.AddListener(OnMusicToggled);
            _sfxToggle?.onValueChanged.AddListener(OnSfxToggled);
            _vibrationToggle?.onValueChanged.AddListener(OnVibrationToggled);
            _graphicsCycleButton?.onClick.AddListener(OnGraphicsCycle);
            _privacyPolicyButton?.onClick.AddListener(OnPrivacyPolicy);
            _termsButton?.onClick.AddListener(OnTerms);
            _restorePurchasesButton?.onClick.AddListener(OnRestorePurchases);
        }

        protected override void OnShow()
        {
            if (ServiceLocator.TryGet<IAnalyticsService>(out var analytics))
                analytics.LogEvent(AnalyticsEvents.SettingsOpened);
            UpdateGraphicsLabel();
            if (_languageLabel != null) _languageLabel.text = "English";
        }

        private void OnMusicToggled(bool value)
        {
            if (ServiceLocator.TryGet<IAudioService>(out var audio))
                audio.SetMusicVolume(value ? 1f : 0f);
        }

        private void OnSfxToggled(bool value)
        {
            if (ServiceLocator.TryGet<IAudioService>(out var audio))
                audio.SetSfxVolume(value ? 1f : 0f);
        }

        private void OnVibrationToggled(bool value)
        {
            // Vibration handled by platform haptics — placeholder
            GameLogger.Info("SettingsScreen", $"Vibration: {value}");
        }

        private void OnGraphicsCycle()
        {
            _graphicsIndex = (_graphicsIndex + 1) % _graphicsOptions.Length;
            QualitySettings.SetQualityLevel(_graphicsOptions.Length - 1 - _graphicsIndex);
            UpdateGraphicsLabel();
        }

        private void UpdateGraphicsLabel()
        {
            if (_graphicsValueLabel != null) _graphicsValueLabel.text = _graphicsOptions[_graphicsIndex];
        }

        private void OnPrivacyPolicy()  => Application.OpenURL("https://yourstudio.com/privacy");
        private void OnTerms()          => Application.OpenURL("https://yourstudio.com/terms");
        private async void OnRestorePurchases()
        {
            if (ServiceLocator.TryGet<IPurchaseService>(out var purchase))
                await purchase.RestorePurchasesAsync();
        }

        private void OnBackClicked() => ScreenManager.Instance.Back();

        private void OnDestroy()
        {
            _backButton?.onClick.RemoveListener(OnBackClicked);
            _musicToggle?.onValueChanged.RemoveListener(OnMusicToggled);
            _sfxToggle?.onValueChanged.RemoveListener(OnSfxToggled);
            _vibrationToggle?.onValueChanged.RemoveListener(OnVibrationToggled);
            _graphicsCycleButton?.onClick.RemoveListener(OnGraphicsCycle);
            _privacyPolicyButton?.onClick.RemoveListener(OnPrivacyPolicy);
            _termsButton?.onClick.RemoveListener(OnTerms);
            _restorePurchasesButton?.onClick.RemoveListener(OnRestorePurchases);
        }
    }
}
