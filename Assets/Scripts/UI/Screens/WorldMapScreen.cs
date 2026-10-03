using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KingSmash.Core;
using KingSmash.Services;
namespace KingSmash.UI.Screens
{
    public class WorldMapScreen : UIScreen
    {
        [Header("Navigation")]
        [SerializeField] private Button _backButton;

        [Header("World Tabs")]
        [SerializeField] private List<Button> _worldTabButtons;
        [SerializeField] private List<TextMeshProUGUI> _worldTabLabels;

        [Header("Level Grid")]
        [SerializeField] private Transform _levelNodeContainer;
        [SerializeField] private LevelNodeWidget _levelNodePrefab;

        [Header("World Info")]
        [SerializeField] private TextMeshProUGUI _worldNameLabel;
        [SerializeField] private TextMeshProUGUI _worldProgressLabel;

        private int _selectedWorld = 0;
        private readonly List<LevelNodeWidget> _spawnedNodes = new();

        private static readonly string[] WorldNames =
        {
            "Forest Kingdom", "Desert Fortress", "Ice Kingdom", "Dark Realm", "Final Castle"
        };

        protected override void Awake()
        {
            base.Awake();
            _backButton?.onClick.AddListener(OnBackClicked);
            for (int i = 0; i < _worldTabButtons.Count; i++)
            {
                int worldIdx = i;
                _worldTabButtons[i].onClick.AddListener(() => SelectWorld(worldIdx));
            }
        }

        protected override void OnShow()
        {
            SelectWorld(0);
        }

        private void SelectWorld(int worldIndex)
        {
            _selectedWorld = worldIndex;
            if (_worldNameLabel != null)
                _worldNameLabel.text = worldIndex < WorldNames.Length ? WorldNames[worldIndex] : $"World {worldIndex + 1}";

            RefreshLevelNodes(worldIndex);
        }

        private void RefreshLevelNodes(int worldIndex)
        {
            foreach (var node in _spawnedNodes) Destroy(node.gameObject);
            _spawnedNodes.Clear();

            if (_levelNodePrefab == null || _levelNodeContainer == null) return;

            ServiceLocator.TryGet<ISaveService>(out var save);
            int baseLevel = worldIndex * 20;
            int starsEarned = 0;
            int totalLevels = 20;

            for (int i = 0; i < totalLevels; i++)
            {
                int levelIndex = baseLevel + i;
                int stars = save?.Current.GetStarsForLevel(levelIndex) ?? 0;
                bool unlocked = levelIndex <= (save?.Current.currentLevel ?? 0);
                starsEarned += stars;

                var node = Instantiate(_levelNodePrefab, _levelNodeContainer);
                int capturedIdx = levelIndex;
                node.Setup(levelIndex + 1, stars, unlocked, () => OnLevelNodeClicked(capturedIdx));
                _spawnedNodes.Add(node);
            }

            if (_worldProgressLabel != null)
                _worldProgressLabel.text = $"{starsEarned}/{totalLevels * 3}";
        }

        private void OnLevelNodeClicked(int levelIndex)
        {
            var screen = ScreenManager.Instance.Show<LevelSelectScreen>();
            screen?.SetLevel(levelIndex);
        }

        private void OnBackClicked() => ScreenManager.Instance.Back();

        private void OnDestroy()
        {
            _backButton?.onClick.RemoveListener(OnBackClicked);
        }
    }
}
