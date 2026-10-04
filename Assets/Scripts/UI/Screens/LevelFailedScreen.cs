using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KingSmash.Ads;
using KingSmash.Audio;
using KingSmash.Core;
using KingSmash.Levels;
using KingSmash.Services;
using KingSmash.UI;

namespace KingSmash.UI.Screens
{
    public class LevelFailedScreen : UIScreen
    {
        // ── Original fields ──────────────────────────────────────────────────
        [Header("Info")]
        [SerializeField] private TextMeshProUGUI _reasonLabel;
        [SerializeField] private TextMeshProUGUI _attemptsLabel;

        [Header("Buttons")]
        [SerializeField] private Button _retryButton;
        [SerializeField] private Button _homeButton;

        [Header("Ad Extra Attempt")]
        [SerializeField] private ExtraAttemptPanel _extraAttemptPanel;

        // ── M13 Polish fields ────────────────────────────────────────────────
        [Header("M13 Polish")]
        [SerializeField] private KingSmashTheme  _themeConfig;
        [SerializeField] private CanvasGroup     _screenCg;
        [SerializeField] private RectTransform   _failPanel;
        [SerializeField] private TextMeshProUGUI _tipLabel;

        // ── Private state ────────────────────────────────────────────────────
        private LevelResult _result;
        private bool        _extraAttemptDecided;
        private bool        _extraAttemptAccepted;

        private static readonly string[] s_Tips =
        {
            "Try using a power-up!",
            "Aim for the weak points!",
            "Upgrade your King for more power!",
            "Chain explosions for better results!"
        };

        // ── Lifecycle ────────────────────────────────────────────────────────

        protected override void Awake()
        {
            base.Awake();
            _retryButton?.onClick.AddListener(OnRetryClicked);
            _homeButton?.onClick.AddListener(OnHomeClicked);
        }

        private void OnEnable()  => LevelController.OnLevelEnded += HandleLevelEnded;
        private void OnDisable() => LevelController.OnLevelEnded -= HandleLevelEnded;

        // ── Event handlers ───────────────────────────────────────────────────

        private void HandleLevelEnded(int score, int stars, float destructionRatio)
        {
            if (!GameManager.Instance || GameManager.Instance.CurrentState != GameState.LevelFailed) return;
            _result = new LevelResult
            {
                Stars      = stars,
                Score      = score,
                IsVictory  = false,
                FailReason = "No launches remaining"
            };
            Show();
        }

        // ── Show ─────────────────────────────────────────────────────────────

        protected override void OnShow()
        {
            if (_result == null) return;

            if (_reasonLabel   != null) _reasonLabel.text   = _result.FailReason ?? "No launches remaining!";
            if (_attemptsLabel != null) _attemptsLabel.text = _result.AttemptsRemaining > 0
                ? $"{_result.AttemptsRemaining} attempts remaining"
                : "No attempts remaining!";

            if (_tipLabel != null)
                _tipLabel.text = s_Tips[UnityEngine.Random.Range(0, s_Tips.Length)];

            StartCoroutine(FailSequence());
        }

        private IEnumerator FailSequence()
        {
            // 1. Fade in screen (0.15s)
            if (_screenCg != null)
            {
                _screenCg.alpha = 0f;
                yield return StartCoroutine(UIAnimationController.Fade(_screenCg, 0f, 1f, 0.15f));
            }

            if (ServiceLocator.TryGet<IAudioService>(out var audio))
                audio.Play(SoundId.LevelFail);

            // 2. Slide fail panel from top
            if (_failPanel != null)
            {
                Vector2 end   = _failPanel.anchoredPosition;
                Vector2 start = end + Vector2.up * 80f;
                _failPanel.anchoredPosition = start;
                float elapsed = 0f;
                const float dur = 0.25f;
                while (elapsed < dur)
                {
                    elapsed += Time.unscaledDeltaTime;
                    float t = Mathf.Clamp01(elapsed / dur);
                    t = 1f - (1f - t) * (1f - t); // ease out quad
                    _failPanel.anchoredPosition = Vector2.Lerp(start, end, t);
                    yield return null;
                }
                _failPanel.anchoredPosition = end;
            }

            yield return new WaitForSecondsRealtime(0.15f);

            // 3. Bounce reveal tip label
            if (_tipLabel != null)
                yield return StartCoroutine(UIAnimationController.BounceReveal(_tipLabel.transform, 0.25f));

            yield return new WaitForSecondsRealtime(0.1f);

            // 4. Bounce reveal buttons
            if (_retryButton != null)
                StartCoroutine(UIAnimationController.BounceReveal(_retryButton.transform, 0.3f));
            if (_homeButton != null)
            {
                yield return new WaitForSecondsRealtime(0.07f);
                StartCoroutine(UIAnimationController.BounceReveal(_homeButton.transform, 0.3f));
            }

            // 5. Show extra attempt panel
            if (_extraAttemptPanel != null)
                StartCoroutine(ShowExtraAttemptCoroutine());
        }

        // ── Extra attempt ────────────────────────────────────────────────────

        private IEnumerator ShowExtraAttemptCoroutine()
        {
            AdAvailability availability = AdAvailability.Unavailable;
            if (ServiceLocator.TryGet<IRewardedAdService>(out var adService))
                availability = adService.CheckAvailability(AdPlacement.LevelFailedExtraAttempt);

            _extraAttemptDecided  = false;
            _extraAttemptAccepted = false;

            _extraAttemptPanel.Show(availability);

            StartCoroutine(PulsePanel(_extraAttemptPanel.transform));

            ExtraAttemptPanel.OnExtraAttemptAccepted += HandleExtraAttemptAccepted;
            ExtraAttemptPanel.OnExtraAttemptDeclined += HandleExtraAttemptDeclined;

            float elapsed = 0f;
            const float timeoutSeconds = 30f;
            yield return new WaitUntil(() =>
            {
                elapsed += Time.unscaledDeltaTime;
                return _extraAttemptDecided || elapsed >= timeoutSeconds;
            });

            ExtraAttemptPanel.OnExtraAttemptAccepted -= HandleExtraAttemptAccepted;
            ExtraAttemptPanel.OnExtraAttemptDeclined -= HandleExtraAttemptDeclined;

            if (_extraAttemptAccepted && ServiceLocator.TryGet<RewardedAdFlowService>(out var flowService))
            {
                bool taskDone = false;
                AdRewardResult adResult = null;

                flowService.RequestExtraAttemptAsync().ContinueWith(t =>
                {
                    adResult = t.Result;
                    taskDone = true;
                }, System.Threading.Tasks.TaskScheduler.FromCurrentSynchronizationContext());

                yield return new WaitUntil(() => taskDone);

                if (adResult != null && adResult.WasRewarded)
                {
                    _extraAttemptPanel.Hide();
                    SceneLoader.LoadScene(SceneNames.Level);
                    yield break;
                }
            }

            _extraAttemptPanel.Hide();
        }

        private IEnumerator PulsePanel(Transform target)
        {
            if (target == null) yield break;
            const float pulseScale = 1.04f;
            const float halfDur    = 0.55f;
            for (int i = 0; i < 3; i++)
            {
                float e = 0f;
                Vector3 orig = target.localScale;
                while (e < halfDur)
                {
                    e += Time.unscaledDeltaTime;
                    float t = Mathf.PingPong(e / halfDur, 1f);
                    target.localScale = orig * Mathf.Lerp(1f, pulseScale, t);
                    yield return null;
                }
                target.localScale = orig;
                yield return new WaitForSecondsRealtime(0.3f);
            }
        }

        private void HandleExtraAttemptAccepted()
        {
            _extraAttemptAccepted = true;
            _extraAttemptDecided  = true;
        }

        private void HandleExtraAttemptDeclined()
        {
            _extraAttemptAccepted = false;
            _extraAttemptDecided  = true;
        }

        // ── Button handlers ──────────────────────────────────────────────────

        private void OnRetryClicked()
        {
            if (ServiceLocator.TryGet<IAudioService>(out var audio))
                audio.Play(SoundId.ButtonConfirm);
            SceneLoader.LoadScene(SceneNames.Level);
        }

        private void OnHomeClicked() => SceneLoader.LoadScene(SceneNames.MainMenu);

        private void OnDestroy()
        {
            _retryButton?.onClick.RemoveListener(OnRetryClicked);
            _homeButton?.onClick.RemoveListener(OnHomeClicked);
        }
    }
}
