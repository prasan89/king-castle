using System;
using UnityEngine;

namespace KingSmash.Retention
{
    [Serializable]
    public class MissionDefinition
    {
        public string            missionId;
        public string            displayName;
        public string            description;
        public MissionType       type;
        public int               target;
        public long              coinReward;
        public int               gemReward;
        public string            powerUpTypeId;
        public int               powerUpCount;
        public MissionResetPeriod resetPeriod;
        public Sprite            icon;
    }
}
