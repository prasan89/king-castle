using UnityEngine;
using KingSmash.Core;
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
                // Update stars (never downgrade)
                int prev = data.GetStarsForLevel(result.LevelIndex);
                if (result.Stars > prev)
                    data.SetStarsForLevel(result.LevelIndex, result.Stars);

                // Unlock next level
                int nextLevel = result.LevelIndex + 1;
                if (nextLevel > data.currentLevel)
                    data.currentLevel = nextLevel;

                // Award coins
                data.coins += result.CoinsEarned;
            }

            save.Save();

            GameLogger.Info("LevelProgressionService",
                $"Level {result.LevelIndex + 1} committed. Stars={result.Stars}, Coins+={result.CoinsEarned}, NextUnlocked={data.currentLevel + 1}");

            // Analytics
            if (ServiceLocator.TryGet<IAnalyticsService>(out var analytics))
            {
                string eventName = result.IsVictory ? AnalyticsEvents.LevelComplete : AnalyticsEvents.LevelFailed;
                analytics.LogEvent(eventName,
                    ("level_index",       result.LevelIndex),
                    ("stars",             result.Stars),
                    ("destruction_pct",   (int)(result.DestructionRatio * 100)),
                    ("enemies_defeated",  result.EnemiesDefeated),
                    ("queen_rescued",     result.QueenRescued ? 1 : 0),
                    ("attempts_remaining", result.AttemptsRemaining));
            }
        }
    }
}
