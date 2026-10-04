using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KingSmash.Core;
using KingSmash.Services;
using KingSmash.Retention;
using KingSmash.UI.Widgets;
using KingSmash.Audio;

namespace KingSmash.UI.Screens
{
    public class DailyRewardsScreen : UIScreen
    {
        [Header("Navigation")]
        [SerializeField] private Button _backButton;

        [Header("Day Widgets")]
        [SerializeField] private List<DayRewardWidget> _dayWidgets;

        [Header("Claim")]
        [SerializeField] private Button _claimButton;
        [SerializeField] private TextMeshProUGUI _claimButtonLabel;
        [SerializeField] private TextMeshProUGUI _statusLabel;
        [SerializeField] private TextMeshProUGUI _timerLabel;
        [SerializeField] private GameObject _claimButtonRoot;
        [SerializeField] private CanvasGroup _screenCanvasGroup;

        [Header("M13 Theme & Animation")]
        [SerializeField] private KingSmashTheme _themeConfig;
        [SerializeField] private ParticleSystem _claimParticles;
        [SerializeField] private RectTransform _calendarPanel;
        [SerializeField] private CanvasGroup _claimButtonCg;

        private bool _claiming = false;
        private Coroutine _pulseCoroutine;
        private Coroutine _timerCoroutine;

        protected override void Awake()
        {
            base.Awake();
            _backButton?.onClick.AddListener(OnBackClicked);
            _claimButton?.onClick.AddListener(OnClaimClicked);
        }

        protected override void OnShow()
        {
            if (ServiceLocator.TryGet<DailyRewardService>(out var dailyService))
                dailyService.CheckAndResetIfNewDay();

            RefreshUI();
            StartCoroutine(PlayShowAnimation());
            StartTimerCoroutine();
        }

        protected override void OnHide()
        {
            if (_pulseCoroutine != null) { StopCoroutine(_pulseCoroutine); _pulseCoroutine = null; }
            if (_timerCoroutine != null) { StopCoroutine(_timerCoroutine); _timerCoroutine = null; }
        }

        private IEnumerator PlayShowAnimation()
        {
            if (_calendarPanel != null)
            {
                Vector2 orig = _calendarPanel.anchoredPosition;
                _calendarPanel.anchoredPosition = orig + Vector2.down * 80f;
                yield return UIAnimationController.SlideIn(_calendarPanel, 80f, 0.28f);
            }

            for (int i = 0; i < _dayWidgets.Count; i++)
            {
                if (_dayWidgets[i] == null) continue;
                int captured = i;
                StartCoroutine(DelayedBounceReveal(_dayWidgets[captured].transform, captured * 0.04f));
            }
        }

        private static IEnumerator DelayedBounceReveal(Transform target, float delay)
        {
            if (delay > 0f) yield return new WaitForSecondsRealtime(delay);
            yield return UIAnimationController.BounceReveal(target, 0.30f);
        }

        private void StartClaimButtonPulse()
        {
            if (_pulseCoroutine != null) StopCoroutine(_pulseCoroutine);
            if (_claimButton != null)
                _pulseCoroutine = StartCoroutine(PulseClaimButton());
        }

        private IEnumerator PulseClaimButton()
        {
            if (_claimButton == null) yield break;
            Transform t = _claimButton.transform;
            Vector3 originalScale = t.localScale;
            float pulseInterval = 2f;

            while (true)
            {
                yield return new WaitForSecondsRealtime(pulseInterval);
                float elapsed = 0f;
                float halfDur = 0.15f;
                while (elapsed < halfDur)
                {
                    elapsed += Time.unscaledDeltaTime;
                    t.localScale = Vector3.Lerp(originalScale, originalScale * 1.08f, elapsed / halfDur);
                    yield return null;
                }
                elapsed = 0f;
                while (elapsed < halfDur)
                {
                    elapsed += Time.unscaledDeltaTime;
                    t.localScale = Vector3.Lerp(originalScale * 1.08f, originalScale, elapsed / halfDur);
                    yield return null;
                }
                t.localScale = originalScale;
            }
        }

        private void StartTimerCoroutine()
        {
            if (_timerCoroutine != null) StopCoroutine(_timerCoroutine);
            _timerCoroutine = StartCoroutine(UpdateTimer());
        }

        private IEnumerator UpdateTimer()
        {
            while (true)
            {
                UpdateTimerLabel();
                yield return new WaitForSecondsRealtime(1f);
            }
        }

        private void UpdateTimerLabel()
        {
            if (_timerLabel == null) return;
            if (!ServiceLocator.TryGet<DailyRewardService>(out var dailyService)) return;

            if (dailyService.CanClaimToday())
            {
                _timerLabel.text = "";
                return;
            }

            var now          = System.DateTimeOffset.UtcNow;
            var midnight     = now.Date.AddDays(1);
            long secondsLeft = (long)(midnight - now.DateTime).TotalSeconds;
            int  hours       = (int)(secondsLeft / 3600);
            int  minutes     = (int)((secondsLeft % 3600) / 60);
            int  secs        = (int)(secondsLeft % 60);
            _timerLabel.text = $"Next reward in: {hours:D2}:{minutes:D2}:{secs:D2}";
        }

        private void RefreshUI()
        {
            if (!ServiceLocator.TryGet<DailyRewardService>(out var dailyService)) return;
            if (!ServiceLocator.TryGet<DailyRewardConfig>(out var config)) return;

            bool canClaim   = dailyService.CanClaimToday();
            int  currentDay = dailyService.CurrentDay;

            for (int i = 0; i < _dayWidgets.Count && i < config.CycleLength; i++)
            {
                var entry = config.GetEntry(i + 1);
                DayState state;
                if (i + 1 < currentDay)
                    state = DayState.Claimed;
                else if (i + 1 == currentDay && canClaim)
                    state = DayState.CurrentAvailable;
                else if (i + 1 == currentDay && !canClaim)
                    state = DayState.CurrentClaimed;
                else
                    state = DayState.Upcoming;
                _dayWidgets[i]?.Setup(entry, state);
            }

            if (_claimButton != null)      _claimButton.interactable  = canClaim && !_claiming;
            if (_claimButtonLabel != null)  _claimButtonLabel.text    = canClaim ? "CLAIM REWARD!" : "Come Back Tomorrow";
            if (_statusLabel != null)       _statusLabel.text         = canClaim ? $"Day {currentDay} Available!" : "Reward claimed today";

            if (canClaim)
                StartClaimButtonPulse();
            else if (_pulseCoroutine != null)
            {
                StopCoroutine(_pulseCoroutine);
                _pulseCoroutine = null;
            }
        }

        private void OnClaimClicked()
        {
            if (_claiming) return;
            if (!ServiceLocator.TryGet<DailyRewardService>(out var dailyService)) return;
            _claiming = true;

            var result = dailyService.ClaimTodayReward();
            if (result.Success)
            {
                StartCoroutine(ShowRewardAnimation(result));
            }
            else
            {
                _claiming = false;
            }

            RefreshUI();
        }

        private IEnumerator ShowRewardAnimation(DailyRewardResult result)
        {
            if (_claimButton != null)
                yield return UIAnimationController.ButtonPress(_claimButton.transform, 0.9f, 0.15f);

            if (_claimParticles != null)
                _claimParticles.Play();

            if (result.CoinsGranted > 0)
                ToastService.ShowCoin(result.CoinsGranted);
            else if (result.GemsGranted > 0)
                ToastService.ShowGem(result.GemsGranted);

            if (ServiceLocator.TryGet<IAudioService>(out var audio))
                audio.Play(SoundId.Reward);

            if (_statusLabel != null && result.CoinsGranted > 0)
                StartCoroutine(UIAnimationController.CountUp(_statusLabel, 0L, result.CoinsGranted, 0.6f, "+", " Coins!"));

            RewardRevealUI.Show(result.CoinsGranted, result.GemsGranted, result.PowerUpGranted, result.PowerUpCount);
            yield return null;
            _claiming = false;
        }

        private void OnBackClicked()
        {
            if (ServiceLocator.TryGet<IAudioService>(out var audio)) audio.Play(SoundId.Back);
            ScreenManager.Instance.Back();
        }

        private void OnDestroy()
        {
            _backButton?.onClick.RemoveListener(OnBackClicked);
            _claimButton?.onClick.RemoveListener(OnClaimClicked);
        }
    }
}
