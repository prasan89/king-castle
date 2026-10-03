using KingSmash.Save;

namespace KingSmash.Economy
{
    public static class EconomyCalculator
    {
        public static long CalculateStructureDestroyedReward(EconomyConfig config, int structureCount)
            => config.coinsPerStructureDestroyed * structureCount;

        public static long CalculateEnemyKillReward(EconomyConfig config, int enemyCount)
            => config.coinsPerEnemyKilled * enemyCount;

        public static long CalculateTotalLevelReward(EconomyConfig config, int stars,
            int structuresDestroyed, int enemiesKilled)
        {
            return config.CalculateLevelReward(stars)
                + CalculateStructureDestroyedReward(config, structuresDestroyed)
                + CalculateEnemyKillReward(config, enemiesKilled);
        }

        public static bool CanAfford(SaveData data, long coinCost, int gemCost = 0)
            => data.coins >= coinCost && data.gems >= gemCost;
    }
}
