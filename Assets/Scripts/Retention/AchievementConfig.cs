using System.Collections.Generic;
using UnityEngine;

namespace KingSmash.Retention
{
    [CreateAssetMenu(fileName = "AchievementConfig", menuName = "KingSmash/Config/AchievementConfig")]
    public class AchievementConfig : ScriptableObject
    {
        public List<AchievementDefinition> achievements = new();

        public AchievementDefinition GetAchievement(string id)
        {
            foreach (var a in achievements)
                if (a.achievementId == id) return a;
            return null;
        }

        public void InitializeDefaults()
        {
            achievements = new List<AchievementDefinition>
            {
                new AchievementDefinition
                {
                    achievementId = "first_smash",
                    displayName   = "First Smash",
                    description   = "Play your first level",
                    trackingType  = MissionType.LEVELS_PLAYED,
                    tiers         = new List<AchievementTier>
                    {
                        new AchievementTier { targetCount = 1, coinReward = 100, displayName = "First Smash" }
                    }
                },
                new AchievementDefinition
                {
                    achievementId = "level_milestones",
                    displayName   = "Level Master",
                    description   = "Complete levels to become a master",
                    trackingType  = MissionType.LEVELS_COMPLETED,
                    tiers         = new List<AchievementTier>
                    {
                        new AchievementTier { targetCount = 10,  coinReward = 500,  displayName = "Novice" },
                        new AchievementTier { targetCount = 25,  coinReward = 750,  displayName = "Apprentice" },
                        new AchievementTier { targetCount = 50,  coinReward = 1000, displayName = "Expert" },
                        new AchievementTier { targetCount = 100, coinReward = 2000, displayName = "Master" }
                    }
                },
                new AchievementDefinition
                {
                    achievementId = "queen_rescuer",
                    displayName   = "Queen Rescuer",
                    description   = "Rescue the queen across multiple levels",
                    trackingType  = MissionType.QUEENS_RESCUED,
                    tiers         = new List<AchievementTier>
                    {
                        new AchievementTier { targetCount = 1,   coinReward = 200,  displayName = "First Rescue" },
                        new AchievementTier { targetCount = 10,  coinReward = 500,  displayName = "Royal Guard" },
                        new AchievementTier { targetCount = 25,  coinReward = 750,  displayName = "Champion" },
                        new AchievementTier { targetCount = 100, coinReward = 1500, displayName = "Legendary Rescuer" }
                    }
                },
                new AchievementDefinition
                {
                    achievementId = "castle_destroyer",
                    displayName   = "Castle Destroyer",
                    description   = "Demolish castles across the kingdom",
                    trackingType  = MissionType.CASTLES_DESTROYED,
                    tiers         = new List<AchievementTier>
                    {
                        new AchievementTier { targetCount = 10,  coinReward = 300,  displayName = "Demolisher" },
                        new AchievementTier { targetCount = 25,  coinReward = 500,  displayName = "Wrecker" },
                        new AchievementTier { targetCount = 50,  coinReward = 750,  displayName = "Destroyer" },
                        new AchievementTier { targetCount = 100, coinReward = 1000, displayName = "Annihilator" }
                    }
                },
                new AchievementDefinition
                {
                    achievementId = "enemy_slayer",
                    displayName   = "Enemy Slayer",
                    description   = "Defeat enemies to earn glory",
                    trackingType  = MissionType.ENEMIES_DEFEATED,
                    tiers         = new List<AchievementTier>
                    {
                        new AchievementTier { targetCount = 50,   coinReward = 300,  displayName = "Brawler" },
                        new AchievementTier { targetCount = 200,  coinReward = 500,  displayName = "Warrior" },
                        new AchievementTier { targetCount = 500,  coinReward = 750,  displayName = "Slayer" },
                        new AchievementTier { targetCount = 1000, coinReward = 1500, displayName = "Legend" }
                    }
                },
                new AchievementDefinition
                {
                    achievementId = "powerup_master",
                    displayName   = "Power-Up Master",
                    description   = "Use power-ups to dominate the battlefield",
                    trackingType  = MissionType.POWERUPS_USED,
                    tiers         = new List<AchievementTier>
                    {
                        new AchievementTier { targetCount = 3,  coinReward = 200, displayName = "Initiate" },
                        new AchievementTier { targetCount = 10, coinReward = 400, displayName = "Adept" },
                        new AchievementTier { targetCount = 25, coinReward = 600, displayName = "Master" }
                    }
                },
                new AchievementDefinition
                {
                    achievementId = "three_star",
                    displayName   = "Star Collector",
                    description   = "Earn stars by completing levels with excellence",
                    trackingType  = MissionType.STARS_EARNED,
                    tiers         = new List<AchievementTier>
                    {
                        new AchievementTier { targetCount = 3,  coinReward = 200, displayName = "Rising Star" },
                        new AchievementTier { targetCount = 15, coinReward = 500, displayName = "Star Collector" },
                        new AchievementTier { targetCount = 30, coinReward = 750, displayName = "Star Champion" }
                    }
                }
            };
        }
    }
}
