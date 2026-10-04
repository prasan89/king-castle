using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KingSmash.Core;
using KingSmash.Services;
using KingSmash.Audio;
using KingSmash.UI.Components;

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
        [SerializeField] private Toggle _muteToggle;

        [Header("Audio Sliders")]
        [SerializeField] private Slider _musicVolumeSlider;
        [SerializeField] private Slider _sfxVolumeSlider;

        [Header("Graphics")]
        [SerializeField] private TextMeshProUGUI _graphicsValueLabel;
        [SerializeField] private Button _graphicsCycleButton;

        [Header("Language")]
        [SerializeField] private TextMeshProUGUI _languageLabel;

        [Header("Legal")]
        [SerializeField] private Button _privacyPolicyButton;
        [SerializeField] private Button _termsButton;
        [SerializeField] private Button _restorePurchasesButton;

        [Header("M13 Theme & Animation")]
        [SerializeField] private KingSmashTheme _themeConfig;
        [SerializeField] private CanvasGroup _screenCg;
        [SerializeField] private RectTransform _settingsPanel;
        [SerializeField] private Button _logoutButton;
        [SerializeField] private TextMeshProUGUI _versionLabel;
        [SerializeField] private TextMeshProUGUI _accountLabel;

        private readonly string[] _graphicsOptions = { "High", "Medium", "Low" };
        private int _graphicsIndex = 0;
        private bool _suppressCallbacks;

        protected override void Awake()
        {
            base.Awake();
            _backButton?.onClick.AddListener(OnBackClicked);
            _musicToggle?.onValueChanged.AddListener(OnMusicToggled);
            _sfxToggle?.onValueChanged.AddListener(OnSfxToggled);
            _vibrationToggle?.onValueChanged.AddListener(OnVibrationToggled);
            _muteToggle?.onValueChanged.AddListener(OnMuteToggled);
            _musicVolumeSlider?.onValueChanged.AddListener(OnMusicVolumeChanged);
            _sfxVolumeSlider?.onValueChanged.AddListener(OnSfxVolumeChanged);
            _graphicsCycleButton?.onClick.AddListener(OnGraphicsCycle);
            _privacyPolicyButton?.onClick.AddListener(OnPrivacyPolicy);
            _termsButton?.onClick.AddListener(OnTerms);
            _restorePurchasesButton?.onClick.AddListener(OnRestorePurchases);
            _logoutButton?.onClick.AddListener(OnLogoutClicked);
        }

        protected override void OnShow()
        {
            if (ServiceLocator.TryGet<IAnalyticsService>(out var analytics))
                analytics.LogEvent(AnalyticsEvents.SettingsOpened);

            UpdateGraphicsLabel();
            if (_languageLabel != null) _languageLabel.text = "English";

            if (_versionLabel != null)
                _versionLabel.text = $"v{Application.version}";

            if (_accountLabel != null)
            {
                if (ServiceLocator.TryGet<IAuthService>(out var auth) && auth.CurrentUser != null)
                {
                    _accountLabel.text = auth.CurrentUser.Provider == AuthProvider.Google
                        ? "Signed in as Google"
                        : "Guest";
                }
                else
                {
                    _accountLabel.text = "Guest";
                }
            }

            LoadAudioValues();
            StartCoroutine(PlayShowAnimation());
        }

        private System.Collections.IEnumerator PlayShowAnimation()
        {
            if (_screenCg != null)
            {
                _screenCg.alpha = 0f;
                yield return UIAnimationController.Fade(_screenCg, 0f, 1f, 0.2f);
            }

            if (_settingsPanel != null)
            {
                Vector2 orig = _settingsPanel.anchoredPosition;
                _settingsPanel.anchoredPosition = orig + Vector2.down * 80f;
                yield return UIAnimationController.SlideIn(_settingsPanel, 80f, 0.28f);
            }
        }

        private void LoadAudioValues()
        {
            if (!ServiceLocator.TryGet<IAudioService>(out var audio)) return;

            _suppressCallbacks = true;
            if (_musicVolumeSlider != null) _musicVolumeSlider.value = audio.MusicVolume;
            if (_sfxVolumeSlider   != null) _sfxVolumeSlider.value   = audio.SfxVolume;
            if (_muteToggle        != null) _muteToggle.isOn          = audio.IsMuted;
            if (_musicToggle       != null) _musicToggle.isOn         = audio.MusicVolume > 0f && !audio.IsMuted;
            if (_sfxToggle         != null) _sfxToggle.isOn           = audio.SfxVolume   > 0f && !audio.IsMuted;
            _suppressCallbacks = false;
        }

        private void OnMusicVolumeChanged(float value)
        {
            if (_suppressCallbacks) return;
            if (ServiceLocator.TryGet<IAudioService>(out var audio))
            {
                audio.SetMusicVolume(value);
                AudioSettingsPersistence.Save(audio.MusicVolume, audio.SfxVolume, audio.IsMuted);
            }
            PlayButtonClickSound();
        }

        private void OnSfxVolumeChanged(float value)
        {
            if (_suppressCallbacks) return;
            if (ServiceLocator.TryGet<IAudioService>(out var audio))
            {
                audio.SetSfxVolume(value);
                AudioSettingsPersistence.Save(audio.MusicVolume, audio.SfxVolume, audio.IsMuted);
            }
            PlayButtonClickSound();
        }

        private void OnMuteToggled(bool value)
        {
            if (_suppressCallbacks) return;
            if (ServiceLocator.TryGet<IAudioService>(out var audio))
            {
                audio.SetMuted(value);
                AudioSettingsPersistence.Save(audio.MusicVolume, audio.SfxVolume, audio.IsMuted);
            }
            PlayButtonClickSound();
        }

        private void OnMusicToggled(bool value)
        {
            if (_suppressCallbacks) return;
            if (ServiceLocator.TryGet<IAudioService>(out var audio))
                audio.SetMusicVolume(value ? 1f : 0f);
            PlayButtonClickSound();
        }

        private void OnSfxToggled(bool value)
        {
            if (_suppressCallbacks) return;
            if (ServiceLocator.TryGet<IAudioService>(out var audio))
                audio.SetSfxVolume(value ? 1f : 0f);
            PlayButtonClickSound();
        }

        private void OnVibrationToggled(bool value)
        {
            GameLogger.Info("SettingsScreen", $"Vibration: {value}");
            PlayButtonClickSound();
        }

        private static void PlayButtonClickSound()
        {
            if (ServiceLocator.TryGet<IAudioService>(out var audio))
                audio.Play(SoundId.ButtonClick);
        }

        private void OnGraphicsCycle()
        {
            PlayButtonClickSound();
            _graphicsIndex = (_graphicsIndex + 1) % _graphicsOptions.Length;
            QualitySettings.SetQualityLevel(_graphicsOptions.Length - 1 - _graphicsIndex);
            UpdateGraphicsLabel();
        }

        private void UpdateGraphicsLabel()
        {
            if (_graphicsValueLabel != null)
                _graphicsValueLabel.text = _graphicsOptions[_graphicsIndex];
        }

        private void OnPrivacyPolicy() => Application.OpenURL("https://yourstudio.com/privacy");
        private void OnTerms()         => Application.OpenURL("https://yourstudio.com/terms");

        private async void OnRestorePurchases()
        {
            if (ServiceLocator.TryGet<IPurchaseService>(out var purchase))
                await purchase.RestorePurchasesAsync();
        }

        private void OnLogoutClicked()
        {
            PlayButtonClickSound();
            KSModal.Instance?.Show(
                "Sign Out?",
                "Are you sure you want to sign out?",
                confirm:   "Sign Out",
                cancel:    "Cancel",
                onConfirm: () => _ = DoLogoutAsync(),
                onCancel:  null);
        }

        private async Task DoLogoutAsync()
        {
            if (!ServiceLocator.TryGet<IAuthService>(out var auth)) return;
            await auth.SignOutAsync();
            ScreenManager.Instance.Show<LoginScreen>();
        }

        private void OnBackClicked()
        {
            if (ServiceLocator.TryGet<IAudioService>(out var audio)) audio.Play(SoundId.Back);
            ScreenManager.Instance.Back();
        }

        private void OnDestroy()
        {
            _backButton?.onClick.RemoveListener(OnBackClicked);
            _musicToggle?.onValueChanged.RemoveListener(OnMusicToggled);
            _sfxToggle?.onValueChanged.RemoveListener(OnSfxToggled);
            _vibrationToggle?.onValueChanged.RemoveListener(OnVibrationToggled);
            _muteToggle?.onValueChanged.RemoveListener(OnMuteToggled);
            _musicVolumeSlider?.onValueChanged.RemoveListener(OnMusicVolumeChanged);
            _sfxVolumeSlider?.onValueChanged.RemoveListener(OnSfxVolumeChanged);
            _graphicsCycleButton?.onClick.RemoveListener(OnGraphicsCycle);
            _privacyPolicyButton?.onClick.RemoveListener(OnPrivacyPolicy);
            _termsButton?.onClick.RemoveListener(OnTerms);
            _restorePurchasesButton?.onClick.RemoveListener(OnRestorePurchases);
            _logoutButton?.onClick.RemoveListener(OnLogoutClicked);
        }
    }
}
