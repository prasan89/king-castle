using System;
using System.Collections.Generic;
namespace KingSmash.Levels
{
    public enum WorldEnvironment { Forest, Desert, Ice, Dark, Final }
    public enum WorldStatus { Locked, Available, InProgress, Completed }

    [Serializable]
    public class WorldDefinition
    {
        public int    WorldIndex;
        public string DisplayName;
        public string Description;
        public int    LevelStart;           // inclusive, 0-based
        public int    LevelEnd;             // inclusive, 0-based
        public int    LevelsPerWorld => LevelEnd - LevelStart + 1;
        public int    BossLevelIndex;       // 0-based index of the boss level
        public int    RequiredStarsToUnlock;

        public WorldEnvironment Environment;
        public string BackgroundKey;
        public string MusicKey;
        public string CastleThemeKey;

        // Materials available in this world (display names)
        public List<string> AvailableMaterials;
        // Enemy types available ("Guard","ShieldGuard","Archer","EnemyKing")
        public List<string> AvailableEnemies;
        // New mechanics introduced in this world
        public List<string> NewMechanics;

        public int DifficultyMin;
        public int DifficultyMax;

        // Computed helpers
        public bool ContainsLevel(int levelIndex0Based) =>
            levelIndex0Based >= LevelStart && levelIndex0Based <= LevelEnd;

        public WorldStatus GetStatus(int highestUnlockedLevel, int totalStars)
        {
            if (totalStars < RequiredStarsToUnlock && WorldIndex > 0)
                return WorldStatus.Locked;
            if (highestUnlockedLevel > LevelEnd)
                return WorldStatus.Completed;
            if (highestUnlockedLevel >= LevelStart)
                return WorldStatus.InProgress;
            return WorldStatus.Available;
        }
    }
}
