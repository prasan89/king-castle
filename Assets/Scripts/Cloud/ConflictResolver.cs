using System.Collections.Generic;
using KingSmash.Save;

namespace KingSmash.Cloud
{
    public static class ConflictResolver
    {
        public static SaveData Resolve(
            SaveData local,
            SaveData cloud,
            ConflictResolutionStrategy defaultStrategy = ConflictResolutionStrategy.Merged)
        {
            if (local == null) return cloud;
            if (cloud == null) return local;

            var merged = new SaveData();

            // currentLevel: max
            merged.currentLevel = System.Math.Max(local.currentLevel, cloud.currentLevel);

            // completedLevels: union
            merged.completedLevels = new List<int>(local.completedLevels);
            foreach (var lvl in cloud.completedLevels)
                if (!merged.completedLevels.Contains(lvl))
                    merged.completedLevels.Add(lvl);

            // starsPerLevel: per-level max
            merged.starsPerLevel = new List<LevelStarEntry>(local.starsPerLevel);
            foreach (var cloudEntry in cloud.starsPerLevel)
            {
                bool found = false;
                foreach (var mergedEntry in merged.starsPerLevel)
                {
                    if (mergedEntry.levelIndex == cloudEntry.levelIndex)
                    {
                        if (cloudEntry.stars > mergedEntry.stars)
                            mergedEntry.stars = cloudEntry.stars;
                        found = true;
                        break;
                    }
                }
                if (!found)
                    merged.starsPerLevel.Add(new LevelStarEntry { levelIndex = cloudEntry.levelIndex, stars = cloudEntry.stars });
            }

            // coins/gems: cloud wins (server authoritative)
            merged.coins = cloud.coins;
            merged.gems  = cloud.gems;

            // ownedProducts: union
            merged.ownedProducts = new List<string>(local.ownedProducts);
            foreach (var p in cloud.ownedProducts)
                if (!merged.ownedProducts.Contains(p))
                    merged.ownedProducts.Add(p);

            // kingProgression: field-by-field max
            merged.kingLevel       = System.Math.Max(local.kingLevel, cloud.kingLevel);
            merged.kingPower       = System.Math.Max(local.kingPower, cloud.kingPower);
            merged.kingSpeed       = System.Math.Max(local.kingSpeed, cloud.kingSpeed);
            merged.kingSmashRadius = System.Math.Max(local.kingSmashRadius, cloud.kingSmashRadius);
            merged.kingArmor       = System.Math.Max(local.kingArmor, cloud.kingArmor);

            if (local.kingProgression != null && cloud.kingProgression != null)
            {
                var lk = local.kingProgression;
                var ck = cloud.kingProgression;
                merged.kingProgression = new KingSmash.Progression.KingProgression
                {
                    KingLevel        = System.Math.Max(lk.KingLevel, ck.KingLevel),
                    PowerLevel       = System.Math.Max(lk.PowerLevel, ck.PowerLevel),
                    SpeedLevel       = System.Math.Max(lk.SpeedLevel, ck.SpeedLevel),
                    SmashRadiusLevel = System.Math.Max(lk.SmashRadiusLevel, ck.SmashRadiusLevel),
                    ArmorLevel       = System.Math.Max(lk.ArmorLevel, ck.ArmorLevel)
                };
            }
            else
            {
                merged.kingProgression = local.kingProgression ?? cloud.kingProgression ?? new KingSmash.Progression.KingProgression();
            }

            // powerUpInventory: cloud wins
            merged.ownedPowerUps    = cloud.ownedPowerUps ?? local.ownedPowerUps;
            merged.powerUpInventory = cloud.powerUpInventory ?? local.powerUpInventory;

            // missionProgress: cloud wins
            merged.missionProgress = cloud.missionProgress ?? local.missionProgress;

            // achievementProgress: cloud wins
            merged.achievementProgress = cloud.achievementProgress ?? local.achievementProgress;

            // dailyRewardState: cloud wins
            merged.dailyRewardState = cloud.dailyRewardState ?? local.dailyRewardState;

            // adSessionStats: local wins (session-only)
            merged.adSessionStats = local.adSessionStats;

            // currentWorld + additive lists
            merged.currentWorld = System.Math.Max(local.currentWorld, cloud.currentWorld);

            merged.completedWorlds = new List<int>(local.completedWorlds);
            foreach (var w in cloud.completedWorlds)
                if (!merged.completedWorlds.Contains(w))
                    merged.completedWorlds.Add(w);

            merged.defeatedBosses = new List<int>(local.defeatedBosses);
            foreach (var b in cloud.defeatedBosses)
                if (!merged.defeatedBosses.Contains(b))
                    merged.defeatedBosses.Add(b);

            merged.processedLevelRewards = new List<int>(local.processedLevelRewards);
            foreach (var r in cloud.processedLevelRewards)
                if (!merged.processedLevelRewards.Contains(r))
                    merged.processedLevelRewards.Add(r);

            merged.economyVersion      = System.Math.Max(local.economyVersion, cloud.economyVersion);
            merged.lastKnownServerTime = System.Math.Max(local.lastKnownServerTime, cloud.lastKnownServerTime);

            // cloud revision and playerId
            merged.cloudRevision     = System.Math.Max(local.cloudRevision, cloud.cloudRevision);
            merged.cloudPlayerId     = !string.IsNullOrEmpty(cloud.cloudPlayerId) ? cloud.cloudPlayerId : local.cloudPlayerId;
            merged.playerId          = !string.IsNullOrEmpty(cloud.playerId) ? cloud.playerId : local.playerId;
            merged.lastSavedTimestamp = System.Math.Max(local.lastSavedTimestamp, cloud.lastSavedTimestamp);
            merged.lastSyncTimestamp  = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            merged.version = SaveData.CurrentVersion;
            return merged;
        }
    }
}
