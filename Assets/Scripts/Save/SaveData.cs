using System;
using System.Collections.Generic;

namespace KingSmash.Save
{
    // JsonUtility cannot serialize Dictionary — use a flat list of key-value pairs instead.
    [Serializable]
    public class LevelStarEntry
    {
        public int levelIndex;
        public int stars;
    }

    [Serializable]
    public class SaveData
    {
        public const int CurrentVersion = 1;

        public int version = CurrentVersion;
        public string playerId = "";
        public int currentLevel = 0;
        public List<int> completedLevels = new();
        public List<LevelStarEntry> starsPerLevel = new();
        public long coins = 0;
        public int gems = 0;
        public int kingLevel = 1;
        public float kingPower = 1f;
        public float kingSpeed = 1f;
        public float kingSmashRadius = 1f;
        public float kingArmor = 1f;
        public List<string> ownedPowerUps = new();
        public DailyRewardState dailyRewardState = new();
        public long lastSavedTimestamp = 0;

        public int GetStarsForLevel(int levelIndex)
        {
            foreach (var e in starsPerLevel)
                if (e.levelIndex == levelIndex) return e.stars;
            return 0;
        }

        public void SetStarsForLevel(int levelIndex, int stars)
        {
            foreach (var e in starsPerLevel)
            {
                if (e.levelIndex == levelIndex) { e.stars = stars; return; }
            }
            starsPerLevel.Add(new LevelStarEntry { levelIndex = levelIndex, stars = stars });
        }

        public int GetTotalStars()
        {
            int total = 0;
            foreach (var e in starsPerLevel) total += e.stars;
            return total;
        }

        public static SaveData CreateNew()
        {
            return new SaveData
            {
                playerId = Guid.NewGuid().ToString(),
                lastSavedTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };
        }
    }

    [Serializable]
    public class DailyRewardState
    {
        public int currentStreakDay = 0;
        public long lastClaimedTimestamp = 0;
        public bool claimedToday = false;
    }
}
