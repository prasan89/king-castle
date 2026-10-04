using System;
using System.Collections.Generic;
using UnityEngine;

namespace KingSmash.Retention
{
    [Serializable]
    public class AchievementTier
    {
        public int    targetCount;
        public long   coinReward;
        public int    gemReward;
        public string displayName;
    }

    [Serializable]
    public class AchievementDefinition
    {
        public string                  achievementId;
        public string                  displayName;
        public string                  description;
        public MissionType             trackingType;
        public List<AchievementTier>   tiers;
        public Sprite                  icon;
    }
}
