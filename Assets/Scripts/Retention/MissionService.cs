using System;
using KingSmash.Characters;
using KingSmash.Economy;
using KingSmash.Gameplay;
using KingSmash.Levels;
using KingSmash.PowerUps;
using KingSmash.Save;
using KingSmash.Services;

namespace KingSmash.Retention
{
    public sealed class MissionService
    {
        public static event Action<string>                       OnMissionProgressed;
        public static event Action<string, MissionClaimResult>   OnMissionClaimed;
        public static event Action<string, int>                  OnAchievementProgressed;
        public static event Action<string, int, RewardResult>    OnAchievementTierClaimed;

        private readonly ISaveService       _save;
        private readonly MissionConfig      _config;
        private readonly AchievementConfig  _achConfig;
        private readonly RewardService      _rewardService;
        private readonly PowerUpService     _powerUpService;
        private readonly CurrencyService    _currencyService;

        private bool _allCastlesDestroyedThisLevel;

        public MissionService(
            ISaveService      save,
            MissionConfig     config,
            AchievementConfig achConfig,
            RewardService     rewardService,
            PowerUpService    powerUpService,
            CurrencyService   currencyService)
        {
            _save            = save            ?? throw new ArgumentNullException(nameof(save));
            _config          = config          ?? throw new ArgumentNullException(nameof(config));
            _achConfig       = achConfig       ?? throw new ArgumentNullException(nameof(achConfig));
            _rewardService   = rewardService   ?? throw new ArgumentNullException(nameof(rewardService));
            _powerUpService  = powerUpService  ?? throw new ArgumentNullException(nameof(powerUpService));
            _currencyService = currencyService ?? throw new ArgumentNullException(nameof(currencyService));
        }

        public void Initialize()
        {
            LevelController.OnLevelEnded                  += HandleLevelEnded;
            QueenController.OnQueenRescued                += HandleQueenRescued;
            EnemyHealth.OnEnemyDefeated                   += HandleEnemyDefeated;
            PowerUpService.OnPowerUpActivated             += HandlePowerUpActivated;
            DestructionController.OnAllCastlesDestroyed   += HandleAllCastlesDestroyed;
        }

        public void Dispose()
        {
            LevelController.OnLevelEnded                  -= HandleLevelEnded;
            QueenController.OnQueenRescued                -= HandleQueenRescued;
            EnemyHealth.OnEnemyDefeated                   -= HandleEnemyDefeated;
            PowerUpService.OnPowerUpActivated             -= HandlePowerUpActivated;
            DestructionController.OnAllCastlesDestroyed   -= HandleAllCastlesDestroyed;
        }

        private void HandleAllCastlesDestroyed()
        {
            _allCastlesDestroyedThisLevel = true;
        }

        private void HandleLevelEnded(int score, int stars, float destructionRatio)
        {
            IncrementMissionProgress(MissionType.LEVELS_PLAYED, 1);

            var result = LevelProgressionService.LastResult;
            if (result != null && result.IsVictory)
            {
                IncrementMissionProgress(MissionType.LEVELS_COMPLETED, 1);
                if (stars > 0)
                    IncrementMissionProgress(MissionType.STARS_EARNED, stars);
            }

            if (_allCastlesDestroyedThisLevel)
                IncrementMissionProgress(MissionType.CASTLES_DESTROYED, 1);

            _allCastlesDestroyedThisLevel = false;
        }

        private void HandleQueenRescued(QueenController q)
        {
            IncrementMissionProgress(MissionType.QUEENS_RESCUED, 1);
        }

        private void HandleEnemyDefeated(EnemyHealth e)
        {
            IncrementMissionProgress(MissionType.ENEMIES_DEFEATED, 1);
        }

        private void HandlePowerUpActivated(PowerUpType p)
        {
            IncrementMissionProgress(MissionType.POWERUPS_USED, 1);
        }

        public void IncrementMissionProgress(MissionType type, int amount = 1)
        {
            bool anyChanged = false;

            foreach (var def in _config.dailyMissions)
            {
                if (def.type != type) continue;
                var mp = _save.Current.GetOrCreateMissionProgress(def.missionId);
                if (mp.claimed) continue;
                mp.progress += amount;
                OnMissionProgressed?.Invoke(def.missionId);
                anyChanged = true;
            }

            foreach (var def in _config.permanentMissions)
            {
                if (def.type != type) continue;
                var mp = _save.Current.GetOrCreateMissionProgress(def.missionId);
                if (mp.claimed) continue;
                mp.progress += amount;
                OnMissionProgressed?.Invoke(def.missionId);
                anyChanged = true;
            }

            foreach (var ach in _achConfig.achievements)
            {
                if (ach.trackingType != type) continue;
                var ap = _save.Current.GetOrCreateAchievementProgress(ach.achievementId);
                ap.progress += amount;
                OnAchievementProgressed?.Invoke(ach.achievementId, ap.progress);
                anyChanged = true;
            }

            if (anyChanged)
                _save.Save();
        }

        public bool CanClaimMission(string missionId)
        {
            var def = _config.GetMission(missionId);
            if (def == null) return false;
            var mp = _save.Current.GetOrCreateMissionProgress(missionId);
            return mp.progress >= def.target && !mp.claimed;
        }

        public MissionClaimResult ClaimMission(string missionId)
        {
            if (!CanClaimMission(missionId))
                return new MissionClaimResult { Success = false, MissionId = missionId, FailReason = "not_claimable" };

            var def    = _config.GetMission(missionId);
            var mp     = _save.Current.GetOrCreateMissionProgress(missionId);
            mp.claimed = true;

            var reward = new RewardResult
            {
                coins  = def.coinReward,
                gems   = def.gemReward,
                source = RewardSource.Mission
            };

            if (def.coinReward > 0)
                _currencyService.TryAdd(def.coinReward, $"mission_{missionId}", out _);

            if (def.gemReward > 0)
                _save.AddGems(def.gemReward);

            if (!string.IsNullOrEmpty(def.powerUpTypeId) && def.powerUpCount > 0)
            {
                var puType = PowerUpTypeExtensions.FromId(def.powerUpTypeId);
                if (puType != PowerUpType.None)
                {
                    _powerUpService.Award(puType, def.powerUpCount);
                    reward.powerUpType  = puType;
                    reward.powerUpCount = def.powerUpCount;
                }
            }

            _save.Save();

            var result = new MissionClaimResult { Success = true, MissionId = missionId, Reward = reward };
            OnMissionClaimed?.Invoke(missionId, result);
            return result;
        }

        public MissionClaimResult ClaimAchievementTier(string achievementId)
        {
            var def = _achConfig.GetAchievement(achievementId);
            if (def == null)
                return new MissionClaimResult { Success = false, MissionId = achievementId, FailReason = "not_found" };

            var ap        = _save.Current.GetOrCreateAchievementProgress(achievementId);
            int tierIndex = GetNextClaimableTierIndex(def);

            if (tierIndex < 0)
                return new MissionClaimResult { Success = false, MissionId = achievementId, FailReason = "no_claimable_tier" };

            var tier = def.tiers[tierIndex];

            var reward = new RewardResult
            {
                coins  = tier.coinReward,
                gems   = tier.gemReward,
                source = RewardSource.Achievement
            };

            if (tier.coinReward > 0)
                _currencyService.TryAdd(tier.coinReward, $"achievement_{achievementId}_tier{tierIndex}", out _);

            if (tier.gemReward > 0)
                _save.AddGems(tier.gemReward);

            ap.claimedTierCount++;
            _save.Save();

            OnAchievementTierClaimed?.Invoke(achievementId, tierIndex, reward);

            return new MissionClaimResult { Success = true, MissionId = achievementId, Reward = reward };
        }

        public void ResetDailyMissions()
        {
            long now = TimeService.UtcNow;
            foreach (var def in _config.dailyMissions)
            {
                if (def.resetPeriod != MissionResetPeriod.Daily) continue;
                var mp = _save.Current.GetOrCreateMissionProgress(def.missionId);
                if (TimeService.IsSameUtcDay(mp.lastResetTimestamp, now)) continue;
                mp.progress           = 0;
                mp.claimed            = false;
                mp.lastResetTimestamp = now;
            }
            _save.Save();
        }

        public MissionProgress GetMissionProgress(string missionId)
            => _save.Current.GetOrCreateMissionProgress(missionId);

        public AchievementProgress GetAchievementProgress(string achievementId)
            => _save.Current.GetOrCreateAchievementProgress(achievementId);

        public int GetNextClaimableTierIndex(AchievementDefinition def)
        {
            var ap = _save.Current.GetOrCreateAchievementProgress(def.achievementId);
            for (int i = ap.claimedTierCount; i < def.tiers.Count; i++)
            {
                if (ap.progress >= def.tiers[i].targetCount)
                    return i;
            }
            return -1;
        }
    }
}
