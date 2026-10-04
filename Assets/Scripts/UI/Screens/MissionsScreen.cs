using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using KingSmash.Core;
using KingSmash.Retention;
using KingSmash.UI.Widgets;

namespace KingSmash.UI.Screens
{
    public class MissionsScreen : UIScreen
    {
        [Header("Navigation")]
        [SerializeField] private Button _backButton;

        [Header("Tabs")]
        [SerializeField] private Button _dailyTab;
        [SerializeField] private Button _achievementsTab;
        [SerializeField] private GameObject _dailyPanel;
        [SerializeField] private GameObject _achievementsPanel;

        [Header("Cards")]
        [SerializeField] private Transform _dailyCardContainer;
        [SerializeField] private Transform _achievementCardContainer;
        [SerializeField] private MissionCardWidget _missionCardPrefab;
        [SerializeField] private AchievementCardWidget _achievementCardPrefab;

        protected override void Awake()
        {
            base.Awake();
            _backButton?.onClick.AddListener(OnBackClicked);
            _dailyTab?.onClick.AddListener(OnDailyTabClicked);
            _achievementsTab?.onClick.AddListener(OnAchievementsTabClicked);
        }

        protected override void OnShow()
        {
            RefreshDailyMissions();
            RefreshAchievements();
            ShowTab(false);
            MissionService.OnMissionProgressed += HandleMissionProgressed;
        }

        protected override void OnHide()
        {
            MissionService.OnMissionProgressed -= HandleMissionProgressed;
        }

        private void OnDisable()
        {
            MissionService.OnMissionProgressed -= HandleMissionProgressed;
        }

        private void HandleMissionProgressed(string missionId)
        {
            RefreshDailyMissions();
        }

        private void RefreshDailyMissions()
        {
            if (_dailyCardContainer == null || _missionCardPrefab == null) return;
            if (!ServiceLocator.TryGet<MissionConfig>(out var config)) return;
            if (!ServiceLocator.TryGet<MissionService>(out var missionService)) return;

            foreach (Transform child in _dailyCardContainer)
                Destroy(child.gameObject);

            foreach (var def in config.dailyMissions)
            {
                var card = Instantiate(_missionCardPrefab, _dailyCardContainer);
                var progress = missionService.GetMissionProgress(def.missionId);
                card.Setup(def, progress);
                card.OnClaimPressed += ClaimMission;
            }
        }

        private void RefreshAchievements()
        {
            if (_achievementCardContainer == null || _achievementCardPrefab == null) return;
            if (!ServiceLocator.TryGet<AchievementConfig>(out var achConfig)) return;
            if (!ServiceLocator.TryGet<MissionService>(out var missionService)) return;

            foreach (Transform child in _achievementCardContainer)
                Destroy(child.gameObject);

            foreach (var def in achConfig.achievements)
            {
                var card = Instantiate(_achievementCardPrefab, _achievementCardContainer);
                var progress = missionService.GetAchievementProgress(def.achievementId);
                card.Setup(def, progress);
                card.OnClaimPressed += ClaimAchievementTier;
            }
        }

        private void ClaimMission(string missionId)
        {
            if (!ServiceLocator.TryGet<MissionService>(out var missionService)) return;
            var result = missionService.ClaimMission(missionId);
            if (result.Success)
            {
                var r = result.Reward;
                RewardRevealUI.Show(r.coins, r.gems, r.powerUpType ?? KingSmash.PowerUps.PowerUpType.None, r.powerUpCount);
            }
            RefreshDailyMissions();
        }

        private void ClaimAchievementTier(string achievementId)
        {
            if (!ServiceLocator.TryGet<MissionService>(out var missionService)) return;
            var result = missionService.ClaimAchievementTier(achievementId);
            if (result.Success)
            {
                var r = result.Reward;
                RewardRevealUI.Show(r.coins, r.gems, r.powerUpType ?? KingSmash.PowerUps.PowerUpType.None, r.powerUpCount);
            }
            RefreshAchievements();
        }

        private void ShowTab(bool achievements)
        {
            _dailyPanel?.SetActive(!achievements);
            _achievementsPanel?.SetActive(achievements);
        }

        private void OnDailyTabClicked() => ShowTab(false);
        private void OnAchievementsTabClicked() => ShowTab(true);
        private void OnBackClicked() => ScreenManager.Instance.Back();

        private void OnDestroy()
        {
            _backButton?.onClick.RemoveListener(OnBackClicked);
            _dailyTab?.onClick.RemoveListener(OnDailyTabClicked);
            _achievementsTab?.onClick.RemoveListener(OnAchievementsTabClicked);
        }
    }
}
