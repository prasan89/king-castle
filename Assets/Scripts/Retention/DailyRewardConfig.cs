using System;
using System.Collections.Generic;
using KingSmash.PowerUps;
using UnityEngine;

namespace KingSmash.Retention
{
    [Serializable]
    public class DailyRewardEntry
    {
        public int    day;
        public long   coinsReward;
        public int    gemsReward;
        public string powerUpTypeId;
        public int    powerUpCount;
        public bool   isTreasureChest;
        public string displayName;
    }

    [CreateAssetMenu(fileName = "DailyRewardConfig", menuName = "KingSmash/Config/DailyRewardConfig")]
    public class DailyRewardConfig : ScriptableObject
    {
        public List<DailyRewardEntry> rewards = new();

        public int CycleLength => rewards.Count;

        public DailyRewardEntry GetEntry(int day1Based)
        {
            if (rewards == null || rewards.Count == 0) return null;
            int idx = ((day1Based - 1) % rewards.Count + rewards.Count) % rewards.Count;
            return rewards[idx];
        }

        public void InitializeDefaults()
        {
            rewards = new List<DailyRewardEntry>
            {
                new DailyRewardEntry { day = 1, coinsReward = 100,  gemsReward = 0, powerUpTypeId = "",             powerUpCount = 0, isTreasureChest = false, displayName = "Day 1" },
                new DailyRewardEntry { day = 2, coinsReward = 200,  gemsReward = 0, powerUpTypeId = "",             powerUpCount = 0, isTreasureChest = false, displayName = "Day 2" },
                new DailyRewardEntry { day = 3, coinsReward = 0,    gemsReward = 10, powerUpTypeId = "",            powerUpCount = 0, isTreasureChest = false, displayName = "Day 3" },
                new DailyRewardEntry { day = 4, coinsReward = 300,  gemsReward = 0, powerUpTypeId = "",             powerUpCount = 0, isTreasureChest = false, displayName = "Day 4" },
                new DailyRewardEntry { day = 5, coinsReward = 0,    gemsReward = 0, powerUpTypeId = "powerup_bomb", powerUpCount = 1, isTreasureChest = false, displayName = "Day 5" },
                new DailyRewardEntry { day = 6, coinsReward = 500,  gemsReward = 0, powerUpTypeId = "",             powerUpCount = 0, isTreasureChest = false, displayName = "Day 6" },
                new DailyRewardEntry { day = 7, coinsReward = 1000, gemsReward = 5, powerUpTypeId = "",             powerUpCount = 0, isTreasureChest = true,  displayName = "Day 7" }
            };
        }
    }
}
