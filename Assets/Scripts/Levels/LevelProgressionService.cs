using UnityEngine;
using KingSmash.Core;
using KingSmash.Economy;
using KingSmash.Progression;
using KingSmash.Services;
using KingSmash.UI;

namespace KingSmash.Levels
{
    public class LevelProgressionService : MonoBehaviour
    {
        public static LevelResult LastResult { get; private set; }

        public static void CommitResult(LevelResult result)
        {
            LastResult = result;

            if (!ServiceLocator.TryGet<ISaveService>(out var save)) return;
            var data = save.Current;

            if (result.IsVictory)
            {
                int prev = data.GetStarsForLevel(result.LevelIndex);
                if (result.Stars > prev)
                    data.SetStarsForLevel(result.LevelIndex, result.Stars);

                int nextLevel = result.LevelIndex + 1;
                if (nextLevel > data.currentLevel)
                    data.currentLevel = nextLevel;

                if (data.IsRewardProcessed(result.LevelIndex))
                {
                    Debug.LogWarning($"[LevelProgressionService] Reward for level {result.LevelIndex} already processed — skipping coin/XP awards.");
                }
                else
                {
                    if (ServiceLocator.TryGet<RewardService>(out var rewardService))
                    {
                        var reward = rewardService.ClaimLevelReward(result);
                        result.CoinsEarned = reward.coins;
                        result.XPEarned    = reward.xp;
                    }
                    else
                    {
                        if (ServiceLocator.TryGet<CurrencyService>(out var currency))
                            currency.TryAdd(result.CoinsEarned);
                        else
                            data.coins += result.CoinsEarned;

                        if (ServiceLocator.TryGet<KingProgressionService>(out var progression))
                        {
                            var upgradeConfig = Resources.Load<KingUpgradeConfig>("KingUpgradeConfig");
                            long xp = XPRewardCalculator.Calculate(upgradeConfig, result);
                            result.XPEarned = xp;
                            progression.AddXP(xp);
                        }

                        data.MarkRewardProcessed(result.LevelIndex);
                    }
                }
            }

            save.Save();

            GameLogger.Info("LevelProgressionService",
                $"Level {result.LevelIndex + 1} committed. Stars={result.Stars}, Coins+={result.CoinsEarned}, NextUnlocked={data.currentLevel + 1}");

            if (ServiceLocator.TryGet<IAnalyticsService>(out var analytics))
            {
                string eventName = result.IsVictory ? AnalyticsEvents.LevelComplete : AnalyticsEvents.LevelFailed;
                analytics.LogEvent(eventName,
                    ("level_index",        result.LevelIndex),
                    ("stars",              result.Stars),
                    ("destruction_pct",    (int)(result.DestructionRatio * 100)),
                    ("enemies_defeated",   result.EnemiesDefeated),
                    ("queen_rescued",      result.QueenRescued ? 1 : 0),
                    ("attempts_remaining", result.AttemptsRemaining));
            }
        }
    }
}
