using KingSmash.Save;
using KingSmash.Services;

namespace KingSmash.Retention
{
    public static class NotificationBadgeService
    {
        public static bool HasDailyRewardAvailable(
            ISaveService       save,
            DailyRewardConfig  config,
            DailyRewardService dailyService)
            => dailyService.CanClaimToday();

        public static bool HasClaimableMission(ISaveService save, MissionConfig config)
        {
            var data = save.Current;
            foreach (var def in config.dailyMissions)
            {
                var mp = data.GetOrCreateMissionProgress(def.missionId);
                if (mp.progress >= def.target && !mp.claimed) return true;
            }
            foreach (var def in config.permanentMissions)
            {
                var mp = data.GetOrCreateMissionProgress(def.missionId);
                if (mp.progress >= def.target && !mp.claimed) return true;
            }
            return false;
        }

        public static bool HasClaimableAchievement(ISaveService save, AchievementConfig config)
        {
            var data = save.Current;
            foreach (var ach in config.achievements)
            {
                var ap = data.GetOrCreateAchievementProgress(ach.achievementId);
                for (int i = ap.claimedTierCount; i < ach.tiers.Count; i++)
                {
                    if (ap.progress >= ach.tiers[i].targetCount) return true;
                }
            }
            return false;
        }

        public static int GetTotalBadgeCount(ISaveService save, MissionConfig missionConfig)
        {
            int count = 0;
            var data  = save.Current;
            foreach (var def in missionConfig.dailyMissions)
            {
                var mp = data.GetOrCreateMissionProgress(def.missionId);
                if (mp.progress >= def.target && !mp.claimed) count++;
            }
            foreach (var def in missionConfig.permanentMissions)
            {
                var mp = data.GetOrCreateMissionProgress(def.missionId);
                if (mp.progress >= def.target && !mp.claimed) count++;
            }
            return count;
        }
    }
}
