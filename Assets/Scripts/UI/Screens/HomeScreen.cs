using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KingSmash.Core;
using KingSmash.Services;
using KingSmash.Retention;
namespace KingSmash.UI.Screens
{
    public class HomeScreen : UIScreen
    {
        [Header("Top Bar")]
        [SerializeField] private TextMeshProUGUI _kingLevelLabel;
        [SerializeField] private TextMeshProUGUI _coinsLabel;
        [SerializeField] private TextMeshProUGUI _gemsLabel;
        [SerializeField] private Button _settingsButton;

        [Header("Primary")]
        [SerializeField] private Button _playButton;

        [Header("Bottom Nav")]
        [SerializeField] private Button _kingUpgradeButton;
        [SerializeField] private Button _shopButton;
        [SerializeField] private Button _dailyButton;
        [SerializeField] private Button _missionsButton;

        [Header("Notification Badges")]
        [SerializeField] private GameObject _dailyBadge;
        [SerializeField] private GameObject _missionsBadge;
        [SerializeField] private GameObject _achievementsBadge;

        protected override void Awake()
        {
            base.Awake();
            _playButton?.onClick.AddListener(OnPlayClicked);
            _settingsButton?.onClick.AddListener(OnSettingsClicked);
            _kingUpgradeButton?.onClick.AddListener(OnKingUpgradeClicked);
            _shopButton?.onClick.AddListener(OnShopClicked);
            _dailyButton?.onClick.AddListener(OnDailyClicked);
            _missionsButton?.onClick.AddListener(OnMissionsClicked);
        }

        protected override void OnShow()
        {
            RefreshPlayerData();
            RefreshBadges();
            DailyRewardService.OnDailyClaimed += HandleDailyClaimed;
            MissionService.OnMissionClaimed += HandleMissionClaimed;
            ServiceLocator.TryGet<IAnalyticsService>(out var analytics);
            analytics?.LogEvent(AnalyticsEvents.MainMenuOpened);
            GameManager.Instance?.TransitionTo(GameState.MainMenu);
        }

        private void OnDisable()
        {
            DailyRewardService.OnDailyClaimed -= HandleDailyClaimed;
            MissionService.OnMissionClaimed -= HandleMissionClaimed;
        }

        private void HandleDailyClaimed(DailyRewardResult result) => RefreshBadges();
        private void HandleMissionClaimed(string missionId, MissionClaimResult result) => RefreshBadges();

        private void RefreshBadges()
        {
            if (!ServiceLocator.TryGet<ISaveService>(out var save)
                || !ServiceLocator.TryGet<MissionConfig>(out var missionConfig)
                || !ServiceLocator.TryGet<AchievementConfig>(out var achConfig)
                || !ServiceLocator.TryGet<DailyRewardService>(out var dailyService))
            {
                _dailyBadge?.SetActive(false);
                _missionsBadge?.SetActive(false);
                _achievementsBadge?.SetActive(false);
                return;
            }

            _dailyBadge?.SetActive(dailyService.CanClaimToday());
            _missionsBadge?.SetActive(NotificationBadgeService.HasClaimableMission(save, missionConfig));
            _achievementsBadge?.SetActive(NotificationBadgeService.HasClaimableAchievement(save, achConfig));
        }

        private void RefreshPlayerData()
        {
            if (!ServiceLocator.TryGet<ISaveService>(out var save)) return;
            var data = save.Current;
            if (_kingLevelLabel != null) _kingLevelLabel.text = $"Lv {data.kingLevel}";
            if (_coinsLabel != null)     _coinsLabel.text     = FormatNumber(data.coins);
            if (_gemsLabel != null)      _gemsLabel.text      = data.gems.ToString();
        }

        private void OnPlayClicked()
        {
            StartCoroutine(UIAnimationController.ButtonPress(_playButton.transform));
            ScreenManager.Instance.Show<WorldMapScreen>();
        }
        private void OnSettingsClicked()     => ScreenManager.Instance.Show<SettingsScreen>();
        private void OnKingUpgradeClicked()  => ScreenManager.Instance.Show<UpgradeScreen>();
        private void OnShopClicked()         => ScreenManager.Instance.Show<ShopScreen>();
        private void OnDailyClicked()        => ScreenManager.Instance.Show<DailyRewardsScreen>();
        private void OnMissionsClicked()     => ScreenManager.Instance.Show<MissionsScreen>();

        private static string FormatNumber(long n)
        {
            if (n >= 1_000_000) return $"{n / 1_000_000f:F1}M";
            if (n >= 1_000)     return $"{n / 1_000f:F1}K";
            return n.ToString();
        }

        private void OnDestroy()
        {
            _playButton?.onClick.RemoveListener(OnPlayClicked);
            _settingsButton?.onClick.RemoveListener(OnSettingsClicked);
            _kingUpgradeButton?.onClick.RemoveListener(OnKingUpgradeClicked);
            _shopButton?.onClick.RemoveListener(OnShopClicked);
            _dailyButton?.onClick.RemoveListener(OnDailyClicked);
            _missionsButton?.onClick.RemoveListener(OnMissionsClicked);
        }
    }
}
