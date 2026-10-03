using KingSmash.UI;
using KingSmash.Economy;
namespace KingSmash.Levels
{
    public static class LevelRewardCalculator
    {
        public static LevelResult Build(
            int levelIndex,
            int score,
            float destructionRatio,
            int enemiesDefeated,
            int totalEnemies,
            bool queenRescued,
            int attemptsRemaining,
            bool isVictory,
            string failReason,
            StarThresholds starThresholds,
            EconomyConfig economy)
        {
            float enemyRatio = totalEnemies > 0 ? (float)enemiesDefeated / totalEnemies : 1f;
            int stars = isVictory
                ? StarCalculator.Calculate(starThresholds, destructionRatio, enemyRatio, queenRescued, attemptsRemaining)
                : 0;

            long coinsEarned = 0L;
            if (isVictory && economy != null)
            {
                coinsEarned  = economy.CalculateLevelReward(stars);
                coinsEarned += (long)(destructionRatio * 100f) * economy.coinsPerStructureDestroyed;
                coinsEarned += enemiesDefeated * economy.coinsPerEnemyKilled;
            }

            return new LevelResult
            {
                LevelIndex         = levelIndex,
                Stars              = stars,
                Score              = score,
                DestructionRatio   = destructionRatio,
                EnemiesDefeated    = enemiesDefeated,
                TotalEnemies       = totalEnemies,
                QueenRescued       = queenRescued,
                CoinsEarned        = coinsEarned,
                IsVictory          = isVictory,
                FailReason         = failReason,
                AttemptsRemaining  = attemptsRemaining,
            };
        }
    }
}
