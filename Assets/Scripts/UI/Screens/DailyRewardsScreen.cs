using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KingSmash.Core;
using KingSmash.Services;
using KingSmash.Save;
namespace KingSmash.UI.Screens
{
    [Serializable]
    public class DailyRewardDef
    {
        public int day;
        public long coinsReward;
        public int gemsReward;
        public string displayText;
    }

    public class DailyRewardsScreen : UIScreen
    {
        [Header("Navigation")]
        [SerializeField] private Button _backButton;

        [Header("Day Widgets")]
        [SerializeField] private List<DayRewardWidget> _dayWidgets;   // 7 widgets

        [Header("Claim")]
        [SerializeField] private Button _claimButton;
        [SerializeField] private TextMeshProUGUI _claimButtonLabel;
        [SerializeField] private TextMeshProUGUI _statusLabel;

        private static readonly DailyRewardDef[] Rewards =
        {
            new DailyRewardDef { day = 1, coinsReward = 100,  displayText = "100" },
            new DailyRewardDef { day = 2, coinsReward = 200,  displayText = "200" },
            new DailyRewardDef { day = 3, gemsReward  = 10,   displayText = "10 Gems" },
            new DailyRewardDef { day = 4, coinsReward = 300,  displayText = "300" },
            new DailyRewardDef { day = 5, gemsReward  = 1,    displayText = "1 Gem" },
            new DailyRewardDef { day = 6, coinsReward = 500,  displayText = "500" },
            new DailyRewardDef { day = 7, coinsReward = 1000, gemsReward = 5, displayText = "Special!" },
        };

        protected override void Awake()
        {
            base.Awake();
            _backButton?.onClick.AddListener(OnBackClicked);
            _claimButton?.onClick.AddListener(OnClaimClicked);
        }

        protected override void OnShow()
        {
            RefreshUI();
            if (ServiceLocator.TryGet<IAnalyticsService>(out var analytics))
                analytics.LogEvent(AnalyticsEvents.DailyRewardClaimed); // view event
        }

        private void RefreshUI()
        {
            if (!ServiceLocator.TryGet<ISaveService>(out var save)) return;
            var state = save.Current.dailyRewardState;
            int currentDay = Mathf.Clamp(state.currentStreakDay, 1, 7);
            bool canClaim  = !state.claimedToday;

            for (int i = 0; i < _dayWidgets.Count && i < Rewards.Length; i++)
            {
                var def = Rewards[i];
                bool isCurrent  = (i + 1) == currentDay;
                bool isClaimed  = (i + 1) < currentDay || (isCurrent && !canClaim);
                _dayWidgets[i]?.Setup(def.day, def.displayText, isCurrent, isClaimed, i == 6);
            }

            if (_claimButton != null) _claimButton.interactable = canClaim;
            if (_claimButtonLabel != null) _claimButtonLabel.text = canClaim ? "Claim" : "Come Back Tomorrow";
            if (_statusLabel != null) _statusLabel.text = canClaim ? $"Day {currentDay} Reward!" : "Claimed today!";
        }

        private void OnClaimClicked()
        {
            if (!ServiceLocator.TryGet<ISaveService>(out var save)) return;
            var state = save.Current.dailyRewardState;
            if (state.claimedToday) return;

            int day = Mathf.Clamp(state.currentStreakDay, 1, 7);
            var reward = Rewards[day - 1];

            save.Current.coins += reward.coinsReward;
            save.Current.gems  += reward.gemsReward;
            state.claimedToday = true;
            state.lastClaimedTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            state.currentStreakDay = day < 7 ? day + 1 : 1;
            save.Save();

            if (ServiceLocator.TryGet<IAnalyticsService>(out var analytics))
                analytics.LogEvent(AnalyticsEvents.DailyRewardClaimed, ("day", day));

            RefreshUI();
            StartCoroutine(UIAnimationController.BounceReveal(_claimButton.transform, 0.3f));
        }

        private void OnBackClicked() => ScreenManager.Instance.Back();

        private void OnDestroy()
        {
            _backButton?.onClick.RemoveListener(OnBackClicked);
            _claimButton?.onClick.RemoveListener(OnClaimClicked);
        }
    }
}
