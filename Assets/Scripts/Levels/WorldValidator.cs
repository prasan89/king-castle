using System.Collections.Generic;
namespace KingSmash.Levels
{
    public static class WorldValidator
    {
        public static bool ValidateAll()
        {
            bool ok = true;

            if (WorldRegistry.All.Count != 5)
            {
                KingSmash.Core.GameLogger.Error("WorldValidator", $"Expected 5 worlds, found {WorldRegistry.All.Count}.");
                return false;
            }

            // Each world must have exactly 20 levels, with a boss at the end
            foreach (var w in WorldRegistry.All)
            {
                if (w.LevelsPerWorld != 20)
                {
                    KingSmash.Core.GameLogger.Error("WorldValidator",
                        $"World {w.WorldIndex} ({w.DisplayName}): expected 20 levels, has {w.LevelsPerWorld}.");
                    ok = false;
                }
                if (w.BossLevelIndex != w.LevelEnd)
                {
                    KingSmash.Core.GameLogger.Error("WorldValidator",
                        $"World {w.WorldIndex}: BossLevelIndex={w.BossLevelIndex} should equal LevelEnd={w.LevelEnd}.");
                    ok = false;
                }
            }

            // All 100 level slots covered with no gaps
            var covered = new bool[100];
            foreach (var def in LevelConfigFactory.All)
            {
                if (def.LevelIndex >= 0 && def.LevelIndex < 100)
                    covered[def.LevelIndex] = true;
            }
            for (int i = 0; i < 100; i++)
            {
                if (!covered[i])
                {
                    KingSmash.Core.GameLogger.Error("WorldValidator", $"Level index {i} (Level {i+1}) has no definition.");
                    ok = false;
                }
            }

            // Each level's WorldIndex must match the world it falls in
            foreach (var def in LevelConfigFactory.All)
            {
                var world = WorldRegistry.GetWorldForLevel(def.LevelIndex);
                if (world == null)
                {
                    KingSmash.Core.GameLogger.Error("WorldValidator", $"Level {def.LevelIndex+1}: no world found.");
                    ok = false;
                }
                else if (world.WorldIndex != def.WorldIndex)
                {
                    KingSmash.Core.GameLogger.Error("WorldValidator",
                        $"Level {def.LevelIndex+1}: WorldIndex={def.WorldIndex} but registry says world {world.WorldIndex}.");
                    ok = false;
                }
            }

            // Boss levels: one per world, at LevelEnd
            int bossCount = 0;
            foreach (var def in LevelConfigFactory.All)
                if (def.IsBossLevel) bossCount++;
            if (bossCount != 5)
            {
                KingSmash.Core.GameLogger.Warning("WorldValidator", $"Expected 5 boss levels, found {bossCount}.");
            }

            if (ok)
                KingSmash.Core.GameLogger.Info("WorldValidator", "All 5 worlds validated successfully.");
            return ok;
        }
    }
}
