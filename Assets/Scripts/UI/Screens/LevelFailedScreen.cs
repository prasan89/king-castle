using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KingSmash.Ads;
using KingSmash.Core;
using KingSmash.Levels;
using KingSmash.UI;

namespace KingSmash.UI.Screens
{
    public class LevelFailedScreen : UIScreen
    {
        [Header("Info")]
        [SerializeField] private TextMeshProUGUI _reasonLabel;
        [SerializeField] private TextMeshProUGUI _attemptsLabel;

        [Header("Buttons")]
        [SerializeField] private Button _retryButton;
        [SerializeField] private Button _homeButton;

        [Header("Ad Extra Attempt")]
        [SerializeField] private ExtraAttemptPanel _extraAttemptPanel;

        private LevelResult _result;
        private bool        _extraAttemptDecided;
        private bool        _extraAttemptAccepted;

        protected override void Awake()
        {
            base.Awake();
            _retryButton?.onClick.AddListener(OnRetryClicked);
            _homeButton?.onClick.AddListener(OnHomeClicked);
        }

        private void OnEnable()  => LevelController.OnLevelEnded += HandleLevelEnded;
        private void OnDisable() => LevelController.OnLevelEnded -= HandleLevelEnded;

        private void HandleLevelEnded(int score, int stars, float destructionRatio)
        {
            if (!GameManager.Instance || GameManager.Instance.CurrentState != GameState.LevelFailed) return;
            _result = new LevelResult { Stars = stars, Score = score, IsVictory = false, FailReason = "No launches remaining" };
            Show();
        }

        protected override void OnShow()
        {
            if (_result == null) return;

            if (_reasonLabel   != null) _reasonLabel.text   = _result.FailReason ?? "No launches remaining!";
            if (_attemptsLabel != null) _attemptsLabel.text = _result.AttemptsRemaining > 0
                ? $"{_result.AttemptsRemaining} attempts remaining"
                : "No attempts remaining!";

            if (_extraAttemptPanel != null)
                StartCoroutine(ShowExtraAttemptCoroutine());
        }

        private IEnumerator ShowExtraAttemptCoroutine()
        {
            AdAvailability availability = AdAvailability.Unavailable;
            if (ServiceLocator.TryGet<IRewardedAdService>(out var adService))
                availability = adService.CheckAvailability(AdPlacement.LevelFailedExtraAttempt);

            _extraAttemptDecided  = false;
            _extraAttemptAccepted = false;

            _extraAttemptPanel.Show(availability);

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

        private void OnRetryClicked() => SceneLoader.LoadScene(SceneNames.Level);
        private void OnHomeClicked()  => SceneLoader.LoadScene(SceneNames.MainMenu);

        private void OnDestroy()
        {
            _retryButton?.onClick.RemoveListener(OnRetryClicked);
            _homeButton?.onClick.RemoveListener(OnHomeClicked);
        }
    }
}
