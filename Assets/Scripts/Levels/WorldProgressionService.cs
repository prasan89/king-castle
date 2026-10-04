using System;
using UnityEngine;
using KingSmash.Core;
using KingSmash.Services;
using KingSmash.UI;
namespace KingSmash.Levels
{
    public static class WorldProgressionService
    {
        public static event Action<int> OnWorldUnlocked;     // worldIndex
        public static event Action<int> OnWorldCompleted;    // worldIndex
        public static event Action<LevelResult, WorldDefinition> OnBossDefeated;

        public static void EvaluateAfterLevel(LevelResult result)
        {
            if (!result.IsVictory) return;

            var world = WorldRegistry.GetWorldForLevel(result.LevelIndex);
            if (world == null) return;

            if (!ServiceLocator.TryGet<ISaveService>(out var save)) return;
            var data = save.Current;
            int totalStars = data.GetTotalStars();

            // Boss defeated?
            if (WorldRegistry.IsBossLevel(result.LevelIndex))
            {
                GameLogger.Info("WorldProgression", $"Boss level {result.LevelIndex + 1} defeated!");
                OnBossDefeated?.Invoke(result, world);
            }

            // World completed?
            if (result.LevelIndex == world.LevelEnd && result.Stars > 0)
            {
                GameLogger.Info("WorldProgression", $"World {world.WorldIndex} ({world.DisplayName}) completed!");
                OnWorldCompleted?.Invoke(world.WorldIndex);
            }

            // Next world unlocked?
            int nextWorldIdx = world.WorldIndex + 1;
            var nextWorld = WorldRegistry.GetWorld(nextWorldIdx);
            if (nextWorld != null && totalStars >= nextWorld.RequiredStarsToUnlock)
            {
                int prevHighest = data.currentLevel;
                if (nextWorld.LevelStart > prevHighest)
                {
                    GameLogger.Info("WorldProgression", $"World {nextWorldIdx} ({nextWorld.DisplayName}) unlocked!");
                    OnWorldUnlocked?.Invoke(nextWorldIdx);
                }
            }
        }
    }
}
