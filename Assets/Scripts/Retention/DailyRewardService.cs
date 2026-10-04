using System;
using KingSmash.Economy;
using KingSmash.PowerUps;
using KingSmash.Services;

namespace KingSmash.Retention
{
    public sealed class DailyRewardService
    {
        public static event Action<DailyRewardResult> OnDailyClaimed;

        private readonly ISaveService      _save;
        private readonly RewardService     _rewardService;
        private readonly DailyRewardConfig _config;
        private readonly PowerUpService    _powerUpService;

        public DailyRewardService(
            ISaveService      save,
            RewardService     rewardService,
            DailyRewardConfig config,
            PowerUpService    powerUpService)
        {
            _save           = save           ?? throw new ArgumentNullException(nameof(save));
            _rewardService  = rewardService  ?? throw new ArgumentNullException(nameof(rewardService));
            _config         = config         ?? throw new ArgumentNullException(nameof(config));
            _powerUpService = powerUpService ?? throw new ArgumentNullException(nameof(powerUpService));
        }

        public int CurrentDay
        {
            get
            {
                int day = _save.Current.dailyRewardState.currentStreakDay;
                return Math.Max(1, day == 0 ? 1 : ((day - 1) % _config.CycleLength) + 1);
            }
        }

        public bool CanClaimToday()
        {
            var state = _save.Current.dailyRewardState;
            if (!state.claimedToday) return true;

            // Already claimed — check if a new UTC day has started since last claim
            bool isNewDay = !TimeService.IsSameUtcDay(state.lastClaimedTimestamp, TimeService.UtcNow);
            if (isNewDay)
            {
                state.claimedToday = false;
                _save.Save();
                return true;
            }
            return false;
        }

        public void CheckAndResetIfNewDay()
        {
            var state = _save.Current.dailyRewardState;
            if (!state.claimedToday) return;

            bool isNewDay = !TimeService.IsSameUtcDay(state.lastClaimedTimestamp, TimeService.UtcNow);
            if (isNewDay)
            {
                state.claimedToday = false;
                _save.Save();
            }
        }

        public DailyRewardResult ClaimTodayReward()
        {
            if (!CanClaimToday())
                return new DailyRewardResult { Success = false, FailReason = "already_claimed" };

            string claimId = Guid.NewGuid().ToString();
            var    state   = _save.Current.dailyRewardState;
            int    day     = CurrentDay;
            var    entry   = _config.GetEntry(day);

            if (entry == null)
                return new DailyRewardResult { Success = false, FailReason = "no_entry" };

            if (entry.coinsReward > 0)
                _save.Current.coins += entry.coinsReward;

            if (entry.gemsReward > 0)
                _save.AddGems(entry.gemsReward);

            PowerUpType puType = PowerUpType.None;
            if (!string.IsNullOrEmpty(entry.powerUpTypeId) && entry.powerUpCount > 0)
            {
                puType = KingSmash.PowerUps.PowerUpTypeExtensions.FromId(entry.powerUpTypeId);
                if (puType != PowerUpType.None)
                    _powerUpService.Award(puType, entry.powerUpCount);
            }

            state.claimedToday          = true;
            state.lastClaimedTimestamp  = TimeService.UtcNow;

            // Advance streak day, cycling at CycleLength
            if (state.currentStreakDay < _config.CycleLength)
                state.currentStreakDay++;
            else
                state.currentStreakDay = 1;

            _save.Save();

            var result = new DailyRewardResult
            {
                Success         = true,
                Day             = day,
                CoinsGranted    = entry.coinsReward,
                GemsGranted     = entry.gemsReward,
                PowerUpGranted  = puType,
                PowerUpCount    = entry.powerUpCount,
                IsTreasureChest = entry.isTreasureChest,
                ClaimId         = claimId
            };

            OnDailyClaimed?.Invoke(result);
            return result;
        }
    }
}
