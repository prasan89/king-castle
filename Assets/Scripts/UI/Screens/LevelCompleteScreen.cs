using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KingSmash.Ads;
using KingSmash.Audio;
using KingSmash.Core;
using KingSmash.Economy;
using KingSmash.Levels;
using KingSmash.Progression;
using KingSmash.Services;
using KingSmash.UI;

namespace KingSmash.UI.Screens
{
    public class LevelCompleteScreen : UIScreen
    {
        // ── Original fields ──────────────────────────────────────────────────
        [Header("Stars")]
        [SerializeField] private List<GameObject> _starObjects;

        [Header("Stats")]
        [SerializeField] private TextMeshProUGUI _destructionLabel;
        [SerializeField] private TextMeshProUGUI _enemiesLabel;
        [SerializeField] private TextMeshProUGUI _queenLabel;
        [SerializeField] private TextMeshProUGUI _coinsEarnedLabel;

        [Header("XP / King Level-Up")]
        [SerializeField] private TextMeshProUGUI _xpEarnedLabel;
        [SerializeField] private GameObject      _levelUpPanel;
        [SerializeField] private TextMeshProUGUI _levelUpLabel;

        [Header("Buttons")]
        [SerializeField] private Button _homeButton;
        [SerializeField] private Button _nextLevelButton;
        [SerializeField] private Button _replayButton;

        [Header("Ad Reward")]
        [SerializeField] private DoubleRewardPanel _doubleRewardPanel;

        // ── M13 Polish fields ────────────────────────────────────────────────
        [Header("M13 Polish")]
        [SerializeField] private KingSmashTheme     _themeConfig;
        [SerializeField] private CanvasGroup        _screenCg;
        [SerializeField] private RectTransform      _resultsPanel;
        [SerializeField] private ParticleSystem     _celebrationParticles;
        [SerializeField] private TextMeshProUGUI    _levelNumberLabel;
        [SerializeField] private Animator           _kingCelebrationAnimator;

        // ── Private state ────────────────────────────────────────────────────
        private LevelResult _result;
        private int         _currentLevelIndex;

        private bool _leveledUpThisSession;
        private int  _levelUpFromLevel;
        private int  _levelUpToLevel;

        private bool _doubleRewardDecided;
        private bool _doubleRewardAccepted;

        // ── Lifecycle ────────────────────────────────────────────────────────

        protected override void Awake()
        {
            base.Awake();
            _homeButton?.onClick.AddListener(OnHomeClicked);
            _nextLevelButton?.onClick.AddListener(OnNextLevelClicked);
            _replayButton?.onClick.AddListener(OnReplayClicked);
        }

        private void OnEnable()
        {
            LevelController.OnLevelEnded        += HandleLevelEnded;
            KingProgressionService.OnKingLevelUp += HandleKingLevelUp;
        }

        private void OnDisable()
        {
            LevelController.OnLevelEnded        -= HandleLevelEnded;
            KingProgressionService.OnKingLevelUp -= HandleKingLevelUp;
        }

        // ── Event handlers ───────────────────────────────────────────────────

        private void HandleLevelEnded(int score, int stars, float destructionRatio)
        {
            if (!GameManager.Instance || GameManager.Instance.CurrentState != GameState.LevelComplete) return;

            _leveledUpThisSession = false;
            _levelUpFromLevel     = 0;
            _levelUpToLevel       = 0;

            LevelResult lastResult = LevelProgressionService.LastResult;
            bool resultIsFresh = lastResult != null
                                 && lastResult.Stars == stars
                                 && lastResult.Score == score;

            if (resultIsFresh)
            {
                _result = lastResult;
            }
            else
            {
                _result = new LevelResult
                {
                    Stars            = stars,
                    Score            = score,
                    DestructionRatio = destructionRatio,
                    IsVictory        = true,
                    CoinsEarned      = stars * 25L
                };
            }

            Show();
        }

        private void HandleKingLevelUp(int fromLevel, int toLevel, KingStats stats)
        {
            _leveledUpThisSession = true;
            _levelUpFromLevel     = fromLevel;
            _levelUpToLevel       = toLevel;
        }

        // ── Show / reveal sequence ───────────────────────────────────────────

        protected override void OnShow()
        {
            if (_result == null) return;
            if (_levelUpPanel != null) _levelUpPanel.SetActive(false);
            StartCoroutine(ShowSequence());
        }

        private IEnumerator ShowSequence()
        {
            // 1. Fade in screen
            if (_screenCg != null)
            {
                _screenCg.alpha = 0f;
                yield return StartCoroutine(UIAnimationController.Fade(_screenCg, 0f, 1f, 0.2f));
            }

            // 2. Play LevelComplete sound
            if (ServiceLocator.TryGet<IAudioService>(out var audio))
                audio.Play(SoundId.LevelComplete);

            // 3. Slide in results panel from bottom
            if (_resultsPanel != null)
                yield return StartCoroutine(UIAnimationController.SlideIn(_resultsPanel, 80f, 0.3f));

            if (_levelNumberLabel != null)
                _levelNumberLabel.text = $"Level {_currentLevelIndex + 1}";

            // 4. Wait
            yield return new WaitForSecondsRealtime(0.3f);

            // 5. Stars — BounceReveal with stagger
            foreach (var s in _starObjects) if (s != null) s.SetActive(false);

            for (int i = 0; i < _starObjects.Count; i++)
            {
                if (i < _result.Stars && _starObjects[i] != null)
                {
                    _starObjects[i].SetActive(true);
                    if (ServiceLocator.TryGet<IAudioService>(out var sfx))
                        sfx.Play(SoundId.Stars);
                    yield return StartCoroutine(UIAnimationController.BounceReveal(_starObjects[i].transform, 0.3f));
                    yield return new WaitForSecondsRealtime(0.1f);
                }
            }

            if (_destructionLabel != null)
                _destructionLabel.text = $"{_result.DestructionRatio * 100f:F0}%";
            if (_enemiesLabel != null)
                _enemiesLabel.text = $"{_result.EnemiesDefeated}/{_result.TotalEnemies}";
            if (_queenLabel != null)
                _queenLabel.text = _result.QueenRescued ? "✓" : "✗";

            // 6. Wait
            yield return new WaitForSecondsRealtime(0.3f);

            // 7. CountUp coins
            if (_coinsEarnedLabel != null)
            {
                if (ServiceLocator.TryGet<IAudioService>(out var coinSfx))
                    coinSfx.Play(SoundId.Coins);
                yield return StartCoroutine(UIAnimationController.CountUp(_coinsEarnedLabel, 0, _result.CoinsEarned, 0.8f));
            }

            long xpEarned = _result.XPEarned;
            if (_xpEarnedLabel != null)
            {
                _xpEarnedLabel.text = "+0 XP";
                yield return StartCoroutine(UIAnimationController.CountUp(_xpEarnedLabel, 0, xpEarned, 1.0f,
                    prefix: "+", suffix: " XP"));
            }

            yield return StartCoroutine(ShowDoubleRewardIfAvailableCoroutine());

            // 8. If leveled up
            if (_leveledUpThisSession && _levelUpPanel != null)
            {
                if (_levelUpLabel != null)
                    _levelUpLabel.text = $"King Lv {_levelUpFromLevel} → Lv {_levelUpToLevel}";

                _levelUpPanel.SetActive(true);
                if (_resultsPanel != null)
                    yield return StartCoroutine(UIAnimationController.SlideIn(_levelUpPanel.transform as RectTransform, 40f, 0.3f));
                yield return StartCoroutine(UIAnimationController.BounceReveal(_levelUpPanel.transform, 0.4f));

                if (_kingCelebrationAnimator != null)
                    _kingCelebrationAnimator.SetTrigger("Celebrate");

                if (ServiceLocator.TryGet<IAudioService>(out var upgradeSfx))
                    upgradeSfx.Play(SoundId.Upgrade);
            }

            // 9. Wait
            yield return new WaitForSecondsRealtime(0.2f);

            // 10. BounceReveal buttons
            if (_homeButton != null)
                StartCoroutine(UIAnimationController.BounceReveal(_homeButton.transform, 0.3f));
            if (_nextLevelButton != null)
            {
                yield return new WaitForSecondsRealtime(0.07f);
                StartCoroutine(UIAnimationController.BounceReveal(_nextLevelButton.transform, 0.3f));
            }
            if (_replayButton != null)
            {
                yield return new WaitForSecondsRealtime(0.07f);
                StartCoroutine(UIAnimationController.BounceReveal(_replayButton.transform, 0.3f));
            }

            // 11. Celebration particles
            if (_celebrationParticles != null && !_celebrationParticles.isPlaying)
                _celebrationParticles.Play();
        }

        // ── Double reward ────────────────────────────────────────────────────

        private IEnumerator ShowDoubleRewardIfAvailableCoroutine()
        {
            if (_doubleRewardPanel == null) yield break;
            if (!ServiceLocator.TryGet<RewardedAdFlowService>(out var flowService)) yield break;

            AdAvailability availability = AdAvailability.Unavailable;
            if (ServiceLocator.TryGet<IRewardedAdService>(out var adService))
                availability = adService.CheckAvailability(AdPlacement.LevelCompleteDoubleReward);

            _doubleRewardDecided  = false;
            _doubleRewardAccepted = false;

            _doubleRewardPanel.Show(_result.CoinsEarned, availability);

            DoubleRewardPanel.OnDoubleRewardAccepted += HandleDoubleAccepted;
            DoubleRewardPanel.OnDoubleRewardDeclined += HandleDoubleDeclined;

            float elapsed = 0f;
            const float timeoutSeconds = 30f;
            yield return new WaitUntil(() =>
            {
                elapsed += Time.unscaledDeltaTime;
                return _doubleRewardDecided || elapsed >= timeoutSeconds;
            });

            DoubleRewardPanel.OnDoubleRewardAccepted -= HandleDoubleAccepted;
            DoubleRewardPanel.OnDoubleRewardDeclined -= HandleDoubleDeclined;

            if (_doubleRewardAccepted)
            {
                bool taskDone = false;
                AdRewardResult adResult = null;

                flowService.RequestDoubleRewardAsync(_result).ContinueWith(t =>
                {
                    adResult = t.Result;
                    taskDone = true;
                }, System.Threading.Tasks.TaskScheduler.FromCurrentSynchronizationContext());

                yield return new WaitUntil(() => taskDone);

                if (adResult != null && adResult.WasRewarded)
                {
                    long newTotal = _result.CoinsEarned + adResult.CoinsGranted;
                    if (_coinsEarnedLabel != null)
                        yield return StartCoroutine(UIAnimationController.CountUp(
                            _coinsEarnedLabel, _result.CoinsEarned, newTotal, 0.8f));
                    _result.CoinsEarned = newTotal;
                }
            }

            _doubleRewardPanel.Hide();
        }

        private void HandleDoubleAccepted()
        {
            _doubleRewardAccepted = true;
            _doubleRewardDecided  = true;
        }

        private void HandleDoubleDeclined()
        {
            _doubleRewardAccepted = false;
            _doubleRewardDecided  = true;
        }

        // ── Button handlers ──────────────────────────────────────────────────

        private void OnHomeClicked()      => SceneLoader.LoadScene(SceneNames.MainMenu);
        private void OnNextLevelClicked() => SceneLoader.LoadScene(SceneNames.Level);
        private void OnReplayClicked()    => SceneLoader.LoadScene(SceneNames.Level);

        private void OnDestroy()
        {
            _homeButton?.onClick.RemoveListener(OnHomeClicked);
            _nextLevelButton?.onClick.RemoveListener(OnNextLevelClicked);
            _replayButton?.onClick.RemoveListener(OnReplayClicked);
        }
    }
}
