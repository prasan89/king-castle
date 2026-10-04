using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KingSmash.Core;
using KingSmash.Services;
using KingSmash.Retention;
using KingSmash.UI.Widgets;

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

        private bool _claiming = false;

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
        }

        private void RefreshUI()
        {
            if (!ServiceLocator.TryGet<DailyRewardService>(out var dailyService)) return;
            if (!ServiceLocator.TryGet<DailyRewardConfig>(out var config)) return;

            bool canClaim = dailyService.CanClaimToday();
            int currentDay = dailyService.CurrentDay;

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

            if (_claimButton != null) _claimButton.interactable = canClaim && !_claiming;
            if (_claimButtonLabel != null) _claimButtonLabel.text = canClaim ? "CLAIM REWARD!" : "Come Back Tomorrow";
            if (_statusLabel != null) _statusLabel.text = canClaim ? $"Day {currentDay} Available!" : "Reward claimed today";
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
            RewardRevealUI.Show(result.CoinsGranted, result.GemsGranted, result.PowerUpGranted, result.PowerUpCount);
            yield return null;
            _claiming = false;
        }

        private void OnBackClicked() => ScreenManager.Instance.Back();

        private void OnDestroy()
        {
            _backButton?.onClick.RemoveListener(OnBackClicked);
            _claimButton?.onClick.RemoveListener(OnClaimClicked);
        }
    }
}
