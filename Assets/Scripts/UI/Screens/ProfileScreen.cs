using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KingSmash.Core;
using KingSmash.Services;
using KingSmash.Progression;
using KingSmash.Audio;
using KingSmash.UI.Components;

namespace KingSmash.UI.Screens
{
    public class ProfileScreen : UIScreen
    {
        [Header("Navigation")]
        [SerializeField] private Button _backButton;

        [Header("Player Stats")]
        [SerializeField] private TextMeshProUGUI _playerLevelLabel;
        [SerializeField] private TextMeshProUGUI _totalStarsLabel;
        [SerializeField] private TextMeshProUGUI _levelsCompletedLabel;
        [SerializeField] private TextMeshProUGUI _worldsUnlockedLabel;
        [SerializeField] private TextMeshProUGUI _achievementsLabel;

        [Header("XP Progress")]
        [SerializeField] private KSProgressBar _totalXpBar;

        [Header("M13 Theme & Animation")]
        [SerializeField] private KingSmashTheme _themeConfig;
        [SerializeField] private CanvasGroup _screenCg;

        private KingProgressionService _progressionService;

        protected override void Awake()
        {
            base.Awake();
            _backButton?.onClick.AddListener(OnBackClicked);
        }

        protected override void OnShow()
        {
            ServiceLocator.TryGet<KingProgressionService>(out _progressionService);
            PopulateStats();
            StartCoroutine(PlayShowAnimation());

            if (ServiceLocator.TryGet<IAnalyticsService>(out var analytics))
                analytics.LogEvent(AnalyticsEvents.MainMenuOpened);
        }

        private void PopulateStats()
        {
            if (!ServiceLocator.TryGet<ISaveService>(out var save)) return;
            var data = save.Current;

            int kingLevel       = _progressionService?.KingLevel      ?? data.kingLevel;
            int totalStars      = data.GetTotalStars();
            int levelsCompleted = data.completedLevels.Count;
            int worldsUnlocked  = data.completedWorlds.Count + 1;

            if (_playerLevelLabel     != null) _playerLevelLabel.text     = $"King Level {kingLevel}";
            if (_totalStarsLabel      != null) _totalStarsLabel.text      = totalStars.ToString("N0");
            if (_levelsCompletedLabel != null) _levelsCompletedLabel.text = levelsCompleted.ToString("N0");
            if (_worldsUnlockedLabel  != null) _worldsUnlockedLabel.text  = worldsUnlocked.ToString();

            if (_achievementsLabel != null)
            {
                int claimedTiers = 0;
                if (ServiceLocator.TryGet<KingSmash.Retention.AchievementConfig>(out var achConfig)
                    && ServiceLocator.TryGet<KingSmash.Retention.MissionService>(out var missionService))
                {
                    foreach (var def in achConfig.achievements)
                        claimedTiers += missionService.GetAchievementProgress(def.achievementId).claimedTierCount;
                }
                _achievementsLabel.text = claimedTiers.ToString();
            }

            if (_progressionService != null)
            {
                long currentXP = _progressionService.KingXP;
                long xpForNext = _progressionService.XPRequiredForNextLevel(kingLevel);
                float fraction = xpForNext > 0 ? Mathf.Clamp01((float)currentXP / xpForNext) : 1f;
                _totalXpBar?.SetProgress(0f, animated: false);
                _totalXpBar?.SetProgress(fraction, animated: true, duration: 0.7f);
                _totalXpBar?.SetLabel($"{currentXP:N0} / {xpForNext:N0} XP");
            }
        }

        private IEnumerator PlayShowAnimation()
        {
            if (_screenCg != null)
            {
                _screenCg.alpha = 0f;
                yield return UIAnimationController.Fade(_screenCg, 0f, 1f, 0.2f);
            }

            TextMeshProUGUI[] statLabels = {
                _playerLevelLabel,
                _totalStarsLabel,
                _levelsCompletedLabel,
                _worldsUnlockedLabel,
                _achievementsLabel
            };

            for (int i = 0; i < statLabels.Length; i++)
            {
                if (statLabels[i] == null) continue;
                int captured = i;
                StartCoroutine(DelayedBounceReveal(statLabels[captured].transform, captured * 0.07f));
            }
        }

        private static IEnumerator DelayedBounceReveal(Transform target, float delay)
        {
            if (delay > 0f) yield return new WaitForSecondsRealtime(delay);
            yield return UIAnimationController.BounceReveal(target, 0.25f);
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
