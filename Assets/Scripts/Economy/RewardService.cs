using System;
using KingSmash.Progression;
using KingSmash.PowerUps;
using KingSmash.Save;
using KingSmash.Services;
using KingSmash.UI;
using UnityEngine;

namespace KingSmash.Economy
{
    public sealed class RewardService
    {
        public static event Action<LevelResult, RewardResult> OnLevelRewardClaimed;
        public static event Action<int, RewardResult>         OnWorldRewardClaimed;

        private readonly ISaveService           _save;
        private readonly CurrencyService        _currency;
        private readonly KingProgressionService _progression;
        private readonly EconomyConfig          _config;

        public RewardService(
            ISaveService           save,
            CurrencyService        currency,
            KingProgressionService progression,
            EconomyConfig          config)
        {
            _save        = save        ?? throw new ArgumentNullException(nameof(save));
            _currency    = currency    ?? throw new ArgumentNullException(nameof(currency));
            _progression = progression ?? throw new ArgumentNullException(nameof(progression));
            _config      = config;
        }

        public RewardResult ClaimLevelReward(LevelResult result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));

            var data = _save.Current;

            if (result.IsVictory)
            {
                int prev = data.GetStarsForLevel(result.LevelIndex);
                if (result.Stars > prev)
                    data.SetStarsForLevel(result.LevelIndex, result.Stars);

                int nextLevel = result.LevelIndex + 1;
                if (nextLevel > data.currentLevel)
                    data.currentLevel = nextLevel;
            }

            if (!result.IsVictory || data.IsRewardProcessed(result.LevelIndex))
            {
                _save.Save();
                return RewardResult.Empty;
            }

            long coins = CalculateLevelCoins(result);
            long xp    = CalculateLevelXP(result);

            _currency.TryAdd(coins, "level_reward", out _);
            _progression.AddXP(xp);

            data.MarkRewardProcessed(result.LevelIndex);
            _save.Save();

            result.CoinsEarned = coins;
            result.XPEarned    = xp;

            var reward = new RewardResult
            {
                coins  = coins,
                gems   = 0,
                xp     = xp,
                source = RewardSource.LevelComplete
            };

            OnLevelRewardClaimed?.Invoke(result, reward);

            if (ServiceLocator.TryGet<IAnalyticsService>(out var analytics))
            {
                analytics.LogEvent(AnalyticsEvents.RewardClaimed,
                    ("source",      reward.source.ToString()),
                    ("coins",       (int)Mathf.Min(coins, int.MaxValue)),
                    ("xp",          (int)Mathf.Min(xp,    int.MaxValue)),
                    ("level_index", result.LevelIndex));
            }

            return reward;
        }

        public RewardResult ClaimWorldReward(int worldIndex, EconomyConfig config)
        {
            var data = _save.Current;

            if (data.IsWorldCompleted(worldIndex))
                return RewardResult.Empty;

            var cfg         = config ?? _config;
            var worldReward = cfg?.GetWorldReward(worldIndex);

            long coins = worldReward?.coins ?? 0L;
            int  gems  = worldReward?.gems  ?? 0;

            if (coins > 0)
                _currency.TryAdd(coins, "world_completion", out _);

            if (gems > 0)
                data.gems += gems;

            data.RecordWorldCompleted(worldIndex);
            _save.Save();

            PowerUpType? puType  = null;
            int          puCount = 0;

            if (worldReward != null && !string.IsNullOrEmpty(worldReward.powerUpTypeId) && worldReward.powerUpCount > 0)
            {
                puType  = PowerUpTypeExtensions.FromId(worldReward.powerUpTypeId);
                puCount = worldReward.powerUpCount;
            }

            var reward = new RewardResult
            {
                coins        = coins,
                gems         = gems,
                xp           = 0,
                source       = RewardSource.WorldCompletion,
                powerUpType  = puType,
                powerUpCount = puCount
            };

            OnWorldRewardClaimed?.Invoke(worldIndex, reward);

            if (ServiceLocator.TryGet<IAnalyticsService>(out var analytics))
            {
                analytics.LogEvent(AnalyticsEvents.RewardClaimed,
                    ("source",      reward.source.ToString()),
                    ("world_index", worldIndex),
                    ("coins",       (int)Mathf.Min(coins, int.MaxValue)),
                    ("gems",        gems));
            }

            return reward;
        }

        private long CalculateLevelCoins(LevelResult result)
        {
            if (_config == null)
                return result.CoinsEarned > 0 ? result.CoinsEarned : 0L;

            float t     = Mathf.Clamp01((float)result.LevelIndex / Mathf.Max(1f, 50f));
            long  base_ = (long)Mathf.Lerp(_config.baseLevelCoinsMin, _config.baseLevelCoinsMax, t);
            base_ += (result.LevelIndex / 10) * _config.levelCoinScalePer10Levels;

            long destruction = _config.GetDestructionBonus(result.DestructionRatio);
            long enemyBonus  = result.EnemiesDefeated * _config.coinsPerEnemyKilled;

            long starBonus = result.Stars switch
            {
                1 => _config.oneStarBonus,
                2 => _config.twoStarBonus,
                3 => _config.threeStarBonus,
                _ => 0L
            };

            long queenBonus = result.QueenRescued ? _config.queenRescueBonus : 0L;

            return base_ + destruction + enemyBonus + starBonus + queenBonus;
        }

        private long CalculateLevelXP(LevelResult result)
        {
            var upgradeConfig = Resources.Load<KingSmash.Progression.KingUpgradeConfig>("KingUpgradeConfig");
            return upgradeConfig != null ? XPRewardCalculator.Calculate(upgradeConfig, result) : 0L;
        }
    }
}
