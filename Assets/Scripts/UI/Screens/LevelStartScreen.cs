using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KingSmash.Core;
using KingSmash.Services;
using KingSmash.UI.Widgets;
namespace KingSmash.UI.Screens
{
    public class LevelStartScreen : UIScreen
    {
        [Header("Navigation")]
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _playButton;

        [Header("Level Info")]
        [SerializeField] private TextMeshProUGUI _levelLabel;
        [SerializeField] private TextMeshProUGUI _objectivesSummary;

        [Header("Power-ups")]
        [SerializeField] private Transform _powerUpContainer;
        [SerializeField] private PowerUpCardWidget _powerUpCardPrefab;

        private int _currentLevelIndex;

        protected override void Awake()
        {
            base.Awake();
            _backButton?.onClick.AddListener(OnBackClicked);
            _playButton?.onClick.AddListener(OnPlayClicked);
        }

        public void SetLevel(int levelIndex)
        {
            _currentLevelIndex = levelIndex;
            if (_levelLabel != null) _levelLabel.text = $"Level {levelIndex + 1}";
            if (_objectivesSummary != null) _objectivesSummary.text = "Rescue the Queen\nDefeat all enemies\nDestroy the castle";
        }

        private void OnPlayClicked()
        {
            if (ServiceLocator.TryGet<IAnalyticsService>(out var analytics))
                analytics.LogEvent(AnalyticsEvents.LevelStart, ("level_index", _currentLevelIndex));
            SceneLoader.LoadScene(SceneNames.Level);
        }

        private void OnBackClicked() => ScreenManager.Instance.Back();

        private void OnDestroy()
        {
            _backButton?.onClick.RemoveListener(OnBackClicked);
            _playButton?.onClick.RemoveListener(OnPlayClicked);
        }
    }
}
