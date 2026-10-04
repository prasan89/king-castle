using System.Collections.Generic;
using UnityEngine;

namespace KingSmash.Retention
{
    [CreateAssetMenu(fileName = "MissionConfig", menuName = "KingSmash/Config/MissionConfig")]
    public class MissionConfig : ScriptableObject
    {
        public List<MissionDefinition> dailyMissions     = new();
        public List<MissionDefinition> permanentMissions = new();

        public MissionDefinition GetMission(string missionId)
        {
            foreach (var m in dailyMissions)
                if (m.missionId == missionId) return m;
            foreach (var m in permanentMissions)
                if (m.missionId == missionId) return m;
            return null;
        }

        public void InitializeDefaults()
        {
            dailyMissions = new List<MissionDefinition>
            {
                new MissionDefinition { missionId = "play_5_levels",      displayName = "Play 5 Levels",         description = "Play 5 levels",           type = MissionType.LEVELS_PLAYED,    target = 5,  coinReward = 200, resetPeriod = MissionResetPeriod.Daily },
                new MissionDefinition { missionId = "complete_3_levels",  displayName = "Complete 3 Levels",     description = "Complete 3 levels",       type = MissionType.LEVELS_COMPLETED, target = 3,  coinReward = 300, resetPeriod = MissionResetPeriod.Daily },
                new MissionDefinition { missionId = "rescue_queen_3",     displayName = "Rescue the Queen x3",   description = "Rescue the queen 3 times", type = MissionType.QUEENS_RESCUED,   target = 3,  coinReward = 300, resetPeriod = MissionResetPeriod.Daily },
                new MissionDefinition { missionId = "destroy_10_castles", displayName = "Destroy 10 Castles",   description = "Destroy 10 castles",      type = MissionType.CASTLES_DESTROYED,target = 10, coinReward = 200, resetPeriod = MissionResetPeriod.Daily },
                new MissionDefinition { missionId = "defeat_50_enemies",  displayName = "Defeat 50 Enemies",    description = "Defeat 50 enemies",       type = MissionType.ENEMIES_DEFEATED, target = 50, coinReward = 300, resetPeriod = MissionResetPeriod.Daily },
                new MissionDefinition { missionId = "use_3_powerups",     displayName = "Use 3 Power-Ups",      description = "Use 3 power-ups",         type = MissionType.POWERUPS_USED,    target = 3,  coinReward = 250, resetPeriod = MissionResetPeriod.Daily }
            };
        }
    }
}
