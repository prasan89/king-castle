using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KingSmash.Core;
using KingSmash.Services;
namespace KingSmash.UI.Screens
{
    [Serializable]
    public class MissionDef
    {
        public string id;
        public string displayName;
        public string description;
        public int targetCount;
        public long coinReward;
        public bool isAchievement;
    }

    public class MissionsScreen : UIScreen
    {
        [Header("Navigation")]
        [SerializeField] private Button _backButton;

        [Header("Tabs")]
        [SerializeField] private Button _dailyTab;
        [SerializeField] private Button _achievementsTab;
        [SerializeField] private GameObject _dailyPanel;
        [SerializeField] private GameObject _achievementsPanel;

        [Header("Mission Card Spawn")]
        [SerializeField] private Transform _dailyCardContainer;
        [SerializeField] private Transform _achievementCardContainer;
        [SerializeField] private MissionCardWidget _missionCardPrefab;

        // Inline sample missions — real implementation would load from MissionConfig ScriptableObjects
        private static readonly MissionDef[] SampleDailyMissions =
        {
            new MissionDef { id = "play_5",       displayName = "Play 5 levels",          targetCount = 5,  coinReward = 200 },
            new MissionDef { id = "rescue_queen", displayName = "Rescue Queen 3 times",   targetCount = 3,  coinReward = 300 },
            new MissionDef { id = "destroy_10",   displayName = "Destroy 10 castles",     targetCount = 10, coinReward = 200 },
            new MissionDef { id = "defeat_50",    displayName = "Defeat 50 enemies",      targetCount = 50, coinReward = 300 },
        };

        private static readonly MissionDef[] SampleAchievements =
        {
            new MissionDef { id = "ach_100_levels",   displayName = "Complete 100 levels",  targetCount = 100,  coinReward = 1000, isAchievement = true },
            new MissionDef { id = "ach_1000_enemies", displayName = "Defeat 1,000 enemies", targetCount = 1000, coinReward = 500,  isAchievement = true },
            new MissionDef { id = "ach_3star_10",     displayName = "3-star 10 levels",     targetCount = 10,   coinReward = 500,  isAchievement = true },
        };

        protected override void Awake()
        {
            base.Awake();
            _backButton?.onClick.AddListener(OnBackClicked);
            _dailyTab?.onClick.AddListener(() => ShowTab(false));
            _achievementsTab?.onClick.AddListener(() => ShowTab(true));
        }

        protected override void OnShow()
        {
            BuildMissionCards();
            ShowTab(false);
        }

        private void BuildMissionCards()
        {
            if (_missionCardPrefab == null) return;
            BuildCards(_dailyCardContainer, SampleDailyMissions);
            BuildCards(_achievementCardContainer, SampleAchievements);
        }

        private void BuildCards(Transform container, MissionDef[] missions)
        {
            if (container == null) return;
            foreach (Transform child in container) Destroy(child.gameObject);
            foreach (var mission in missions)
            {
                var card = Instantiate(_missionCardPrefab, container);
                // Use placeholder progress from save — real implementation tracks per-mission progress
                int progress = 0;
                ServiceLocator.TryGet<ISaveService>(out var save);
                card.Setup(mission.displayName, progress, mission.targetCount, mission.coinReward, false);
            }
        }

        private void ShowTab(bool achievements)
        {
            _dailyPanel?.SetActive(!achievements);
            _achievementsPanel?.SetActive(achievements);
        }

        private void OnBackClicked() => ScreenManager.Instance.Back();
        private void OnDestroy()     => _backButton?.onClick.RemoveListener(OnBackClicked);
    }
}
