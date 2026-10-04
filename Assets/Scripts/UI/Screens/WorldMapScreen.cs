using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KingSmash.Core;
using KingSmash.Levels;
using KingSmash.Services;
namespace KingSmash.UI.Screens
{
    public class WorldMapScreen : UIScreen
    {
        [Header("Navigation")]
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _powerUpShopButton;

        [Header("World Tabs")]
        [SerializeField] private List<Button>           _worldTabButtons;
        [SerializeField] private List<TextMeshProUGUI>  _worldTabLabels;
        [SerializeField] private List<Image>            _worldTabBgs;

        [Header("Level Grid")]
        [SerializeField] private Transform      _levelNodeContainer;
        [SerializeField] private LevelNodeWidget _levelNodePrefab;
        [SerializeField] private LevelNodeWidget _bossNodePrefab;

        [Header("World Info Panel")]
        [SerializeField] private TextMeshProUGUI _worldNameLabel;
        [SerializeField] private TextMeshProUGUI _worldDescLabel;
        [SerializeField] private TextMeshProUGUI _worldProgressLabel;
        [SerializeField] private TextMeshProUGUI _worldStatusLabel;

        [Header("Lock Overlay")]
        [SerializeField] private GameObject _worldLockedOverlay;
        [SerializeField] private TextMeshProUGUI _requiredStarsLabel;

        private int _selectedWorldIndex = 0;
        private readonly List<LevelNodeWidget> _spawnedNodes = new();

        private static readonly Color ColorLocked      = new Color(0.35f, 0.35f, 0.45f);
        private static readonly Color ColorAvailable   = new Color(0.20f, 0.45f, 0.85f);
        private static readonly Color ColorInProgress  = new Color(1.00f, 0.75f, 0.10f);
        private static readonly Color ColorCompleted   = new Color(0.18f, 0.72f, 0.25f);

        protected override void Awake()
        {
            base.Awake();
            _backButton?.onClick.AddListener(OnBackClicked);
            _powerUpShopButton?.onClick.AddListener(OnPowerUpShopClicked);
            for (int i = 0; i < _worldTabButtons.Count; i++)
            {
                int idx = i;
                _worldTabButtons[i]?.onClick.AddListener(() => SelectWorld(idx));
            }
        }

        private void OnEnable()
        {
            WorldProgressionService.OnWorldUnlocked  += HandleWorldUnlocked;
            WorldProgressionService.OnWorldCompleted += HandleWorldCompleted;
        }

        private void OnDisable()
        {
            WorldProgressionService.OnWorldUnlocked  -= HandleWorldUnlocked;
            WorldProgressionService.OnWorldCompleted -= HandleWorldCompleted;
        }

        protected override void OnShow()
        {
            RefreshAllWorldTabs();
            if (ServiceLocator.TryGet<ISaveService>(out var save))
            {
                int cur = save.Current.currentLevel;
                var world = WorldRegistry.GetWorldForLevel(cur);
                SelectWorld(world?.WorldIndex ?? 0);
            }
            else
            {
                SelectWorld(0);
            }
        }

        private void RefreshAllWorldTabs()
        {
            ServiceLocator.TryGet<ISaveService>(out var save);
            int highestUnlocked = save?.Current.currentLevel ?? 0;
            int totalStars = save?.Current.GetTotalStars() ?? 0;

            for (int i = 0; i < WorldRegistry.All.Count && i < _worldTabButtons.Count; i++)
            {
                var world = WorldRegistry.All[i];
                var status = world.GetStatus(highestUnlocked, totalStars);

                if (i < _worldTabLabels.Count && _worldTabLabels[i] != null)
                    _worldTabLabels[i].text = world.DisplayName;

                if (i < _worldTabBgs.Count && _worldTabBgs[i] != null)
                    _worldTabBgs[i].color = StatusColor(status);

                if (_worldTabButtons[i] != null)
                    _worldTabButtons[i].interactable = status != WorldStatus.Locked;
            }
        }

        private void SelectWorld(int worldIndex)
        {
            _selectedWorldIndex = worldIndex;
            var world = WorldRegistry.GetWorld(worldIndex);
            if (world == null) return;

            ServiceLocator.TryGet<ISaveService>(out var save);
            int highestUnlocked = save?.Current.currentLevel ?? 0;
            int totalStars      = save?.Current.GetTotalStars() ?? 0;
            var status          = world.GetStatus(highestUnlocked, totalStars);

            if (_worldNameLabel   != null) _worldNameLabel.text   = world.DisplayName;
            if (_worldDescLabel   != null) _worldDescLabel.text   = world.Description;
            if (_worldStatusLabel != null) _worldStatusLabel.text = status.ToString();

            bool locked = status == WorldStatus.Locked;
            _worldLockedOverlay?.SetActive(locked);
            if (_requiredStarsLabel != null)
                _requiredStarsLabel.text = $"Need {world.RequiredStarsToUnlock} ⭐";

            RebuildLevelNodes(world, save);
        }

        private void RebuildLevelNodes(WorldDefinition world, ISaveService save)
        {
            foreach (var n in _spawnedNodes) if (n != null) Destroy(n.gameObject);
            _spawnedNodes.Clear();

            if (_levelNodePrefab == null || _levelNodeContainer == null) return;

            int highestUnlocked = save?.Current.currentLevel ?? 0;
            int starsEarned = 0, starsMax = world.LevelsPerWorld * 3;

            for (int li = world.LevelStart; li <= world.LevelEnd; li++)
            {
                int stars    = save?.Current.GetStarsForLevel(li) ?? 0;
                bool unlocked = li <= highestUnlocked;
                starsEarned  += stars;

                bool isBoss   = WorldRegistry.IsBossLevel(li);
                var prefab    = (isBoss && _bossNodePrefab != null) ? _bossNodePrefab : _levelNodePrefab;
                var node      = Instantiate(prefab, _levelNodeContainer);

                int captured = li;
                node.Setup(li + 1, stars, unlocked, () => OnLevelNodeClicked(captured));
                _spawnedNodes.Add(node);
            }

            if (_worldProgressLabel != null)
                _worldProgressLabel.text = $"{starsEarned}/{starsMax} ⭐";
        }

        private void OnLevelNodeClicked(int levelIndex)
        {
            var screen = ScreenManager.Instance.Show<LevelSelectScreen>();
            screen?.SetLevel(levelIndex);
        }

        private void HandleWorldUnlocked(int worldIdx)  => RefreshAllWorldTabs();
        private void HandleWorldCompleted(int worldIdx) => RefreshAllWorldTabs();
        private void OnBackClicked() => ScreenManager.Instance.Back();
        private void OnPowerUpShopClicked() => ScreenManager.Instance.Show<PowerUpShopScreen>();

        private static Color StatusColor(WorldStatus s) => s switch
        {
            WorldStatus.Locked     => ColorLocked,
            WorldStatus.Available  => ColorAvailable,
            WorldStatus.InProgress => ColorInProgress,
            WorldStatus.Completed  => ColorCompleted,
            _                      => ColorAvailable,
        };

        private void OnDestroy()
        {
            _backButton?.onClick.RemoveListener(OnBackClicked);
            _powerUpShopButton?.onClick.RemoveListener(OnPowerUpShopClicked);
        }
    }
}
