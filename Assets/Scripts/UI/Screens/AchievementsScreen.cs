using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KingSmash.Core;
using KingSmash.Services;
using KingSmash.Retention;
using KingSmash.Audio;
using KingSmash.UI.Components;
using KingSmash.UI.Widgets;

namespace KingSmash.UI.Screens
{
    public class AchievementsScreen : UIScreen
    {
        [Header("Navigation")]
        [SerializeField] private Button _backButton;

        [Header("Achievement List")]
        [SerializeField] private Transform _achievementListContainer;
        [SerializeField] private AchievementCardWidget _cardPrefab;

        [Header("Header Stats")]
        [SerializeField] private TextMeshProUGUI _totalPointsLabel;
        [SerializeField] private KSProgressBar _overallProgressBar;

        [Header("M13 Theme & Animation")]
        [SerializeField] private KingSmashTheme _themeConfig;
        [SerializeField] private CanvasGroup _screenCg;

        private readonly List<AchievementCardWidget> _spawnedCards = new();

        protected override void Awake()
        {
            base.Awake();
            _backButton?.onClick.AddListener(OnBackClicked);
        }

        protected override void OnShow()
        {
            PopulateAchievements();
            StartCoroutine(PlayShowAnimation());

            if (ServiceLocator.TryGet<IAnalyticsService>(out var analytics))
                analytics.LogEvent(AnalyticsEvents.AchievementViewed);
        }

        private IEnumerator PlayShowAnimation()
        {
            if (_screenCg != null)
            {
                _screenCg.alpha = 0f;
                yield return UIAnimationController.Fade(_screenCg, 0f, 1f, 0.2f);
            }

            for (int i = 0; i < _spawnedCards.Count; i++)
            {
                if (_spawnedCards[i] == null) continue;
                int captured = i;
                StartCoroutine(DelayedBounceReveal(_spawnedCards[captured].transform, captured * 0.04f));
            }

            _overallProgressBar?.SetProgress(ComputeOverallProgress(), animated: true, duration: 0.6f);
        }

        private static IEnumerator DelayedBounceReveal(Transform target, float delay)
        {
            if (delay > 0f) yield return new WaitForSecondsRealtime(delay);
            yield return UIAnimationController.BounceReveal(target, 0.30f);
        }

        private void PopulateAchievements()
        {
            foreach (var c in _spawnedCards)
                if (c != null) Destroy(c.gameObject);
            _spawnedCards.Clear();

            if (_achievementListContainer == null || _cardPrefab == null) return;
            if (!ServiceLocator.TryGet<AchievementConfig>(out var achConfig)) return;
            if (!ServiceLocator.TryGet<MissionService>(out var missionService)) return;

            int totalPoints = 0;

            foreach (var def in achConfig.achievements)
            {
                var card = Instantiate(_cardPrefab, _achievementListContainer);
                var progress = missionService.GetAchievementProgress(def.achievementId);
                card.Setup(def, progress);
                card.OnClaimPressed += OnAchievementClaimPressed;
                _spawnedCards.Add(card);

                for (int t = 0; t < progress.claimedTierCount && t < def.tiers.Count; t++)
                    totalPoints += def.tiers[t].coinReward;
            }

            if (_totalPointsLabel != null)
                _totalPointsLabel.text = $"{totalPoints:N0} pts";
        }

        private float ComputeOverallProgress()
        {
            if (!ServiceLocator.TryGet<AchievementConfig>(out var achConfig)) return 0f;
            if (!ServiceLocator.TryGet<MissionService>(out var missionService)) return 0f;

            int totalTiers   = 0;
            int claimedTiers = 0;

            foreach (var def in achConfig.achievements)
            {
                totalTiers += def.tiers.Count;
                var progress = missionService.GetAchievementProgress(def.achievementId);
                claimedTiers += progress.claimedTierCount;
            }

            return totalTiers > 0 ? Mathf.Clamp01((float)claimedTiers / totalTiers) : 0f;
        }

        private void OnAchievementClaimPressed(string achievementId)
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

            PopulateAchievements();
        }

        private void OnBackClicked()
        {
            if (ServiceLocator.TryGet<IAudioService>(out var audio)) audio.Play(SoundId.Back);
            ScreenManager.Instance.Back();
        }

        private void OnDestroy()
        {
            _backButton?.onClick.RemoveListener(OnBackClicked);
        }
    }
}
