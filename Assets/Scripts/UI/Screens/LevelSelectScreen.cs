using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KingSmash.Core;
using KingSmash.Levels;
using KingSmash.Services;
namespace KingSmash.UI.Screens
{
    public class LevelSelectScreen : UIScreen
    {
        [Header("Navigation")]
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _playButton;

        [Header("Level Info")]
        [SerializeField] private TextMeshProUGUI _levelNumberLabel;
        [SerializeField] private TextMeshProUGUI _worldNameLabel;

        [Header("Stars")]
        [SerializeField] private List<GameObject> _starIcons;

        [Header("Objectives")]
        [SerializeField] private Transform _objectivesContainer;
        [SerializeField] private TextMeshProUGUI _objectiveItemPrefab;

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
            PopulateLevel(levelIndex);
        }

        protected override void OnShow() { }

        private void PopulateLevel(int levelIndex)
        {
            if (_levelNumberLabel != null) _levelNumberLabel.text = $"Level {levelIndex + 1}";

            // Stars from save
            ServiceLocator.TryGet<ISaveService>(out var save);
            int stars = save?.Current.GetStarsForLevel(levelIndex) ?? 0;
            for (int i = 0; i < _starIcons.Count; i++)
                if (_starIcons[i] != null) _starIcons[i].SetActive(i < stars);

            // Default objectives
            ClearContainer(_objectivesContainer);
            AddObjective("Rescue the Queen");
            AddObjective("Defeat all enemies");
            AddObjective("Destroy 70% of castle");
        }

        private void AddObjective(string text)
        {
            if (_objectiveItemPrefab == null || _objectivesContainer == null) return;
            var item = Instantiate(_objectiveItemPrefab, _objectivesContainer);
            item.text = text;
        }

        private void ClearContainer(Transform container)
        {
            if (container == null) return;
            foreach (Transform child in container) Destroy(child.gameObject);
        }

        private void OnPlayClicked()
        {
            var screen = ScreenManager.Instance.Show<LevelStartScreen>();
            screen?.SetLevel(_currentLevelIndex);
        }

        private void OnBackClicked() => ScreenManager.Instance.Back();

        private void OnDestroy()
        {
            _backButton?.onClick.RemoveListener(OnBackClicked);
            _playButton?.onClick.RemoveListener(OnPlayClicked);
        }
    }
}
