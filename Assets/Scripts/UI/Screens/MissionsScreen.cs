using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KingSmash.Core;
using KingSmash.Retention;
using KingSmash.UI.Widgets;
using KingSmash.Audio;
using KingSmash.Services;

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

        [Header("M13 Theme & Animation")]
        [SerializeField] private KingSmashTheme _themeConfig;
        [SerializeField] private CanvasGroup _screenCg;
        [SerializeField] private RectTransform _listPanel;
        [SerializeField] private TextMeshProUGUI _refreshTimerLabel;

        private Coroutine _timerCoroutine;

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
            StartCoroutine(PlayShowAnimation());
            StartRefreshTimer();
        }

        protected override void OnHide()
        {
            MissionService.OnMissionProgressed -= HandleMissionProgressed;
            if (_timerCoroutine != null) { StopCoroutine(_timerCoroutine); _timerCoroutine = null; }
        }

        private void OnDisable()
        {
            MissionService.OnMissionProgressed -= HandleMissionProgressed;
        }

        private IEnumerator PlayShowAnimation()
        {
            if (_screenCg != null)
            {
                _screenCg.alpha = 0f;
                yield return UIAnimationController.Fade(_screenCg, 0f, 1f, 0.2f);
            }

            if (_dailyCardContainer != null)
            {
                int idx = 0;
                foreach (Transform child in _dailyCardContainer)
                {
                    int captured = idx++;
                    StartCoroutine(DelayedSlideInFromRight(child, captured * 0.05f));
                }
            }
        }

        private static IEnumerator DelayedSlideInFromRight(Transform target, float delay)
        {
            if (delay > 0f) yield return new WaitForSecondsRealtime(delay);
            var rt = target as RectTransform ?? target.GetComponent<RectTransform>();
            if (rt != null)
            {
                Vector2 orig = rt.anchoredPosition;
                rt.anchoredPosition = orig + Vector2.right * 100f;
                yield return UIAnimationController.SlideIn(rt, 100f, 0.25f);
            }
        }

        private void StartRefreshTimer()
        {
            if (_timerCoroutine != null) StopCoroutine(_timerCoroutine);
            _timerCoroutine = StartCoroutine(UpdateRefreshTimer());
        }

        private IEnumerator UpdateRefreshTimer()
        {
            while (true)
            {
                if (_refreshTimerLabel != null)
                {
                    var now          = System.DateTimeOffset.UtcNow;
                    var midnight     = now.Date.AddDays(1);
                    long secondsLeft = (long)(midnight - now.DateTime).TotalSeconds;
                    int h = (int)(secondsLeft / 3600);
                    int m = (int)((secondsLeft % 3600) / 60);
                    int s = (int)(secondsLeft % 60);
                    _refreshTimerLabel.text = $"Resets in {h:D2}:{m:D2}:{s:D2}";
                }
                yield return new WaitForSecondsRealtime(1f);
            }
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
                if (r.coins > 0)
                    ToastService.ShowCoin(r.coins);
                else if (r.gems > 0)
                    ToastService.ShowGem(r.gems);

                if (ServiceLocator.TryGet<IAudioService>(out var audio))
                    audio.Play(SoundId.Reward);

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
                if (r.coins > 0)
                    ToastService.ShowCoin(r.coins);
                else if (r.gems > 0)
                    ToastService.ShowGem(r.gems);

                if (ServiceLocator.TryGet<IAudioService>(out var audio))
                    audio.Play(SoundId.Reward);

                RewardRevealUI.Show(r.coins, r.gems, r.powerUpType ?? KingSmash.PowerUps.PowerUpType.None, r.powerUpCount);
            }
            RefreshAchievements();
        }

        private void ShowTab(bool achievements)
        {
            _dailyPanel?.SetActive(!achievements);
            _achievementsPanel?.SetActive(achievements);

            if (ServiceLocator.TryGet<IAudioService>(out var audio))
                audio.Play(SoundId.ButtonClick);
        }

        private void OnDailyTabClicked()        => ShowTab(false);
        private void OnAchievementsTabClicked() => ShowTab(true);

        private void OnBackClicked()
        {
            if (ServiceLocator.TryGet<IAudioService>(out var audio)) audio.Play(SoundId.Back);
            ScreenManager.Instance.Back();
        }

        private void OnDestroy()
        {
            _backButton?.onClick.RemoveListener(OnBackClicked);
            _dailyTab?.onClick.RemoveListener(OnDailyTabClicked);
            _achievementsTab?.onClick.RemoveListener(OnAchievementsTabClicked);
        }
    }
}
