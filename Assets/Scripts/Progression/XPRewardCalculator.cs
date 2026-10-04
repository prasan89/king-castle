using UnityEngine;
using KingSmash.UI;

namespace KingSmash.Progression
{
    public static class XPRewardCalculator
    {
        public static long Calculate(KingUpgradeConfig config, LevelResult result)
        {
            if (config == null || !result.IsVictory)
                return 0L;

            long total = config.xpPerLevelComplete;

            total += config.xpPerEnemyKilled * result.EnemiesDefeated;

            if (result.QueenRescued)
                total += config.xpPerQueenRescued;

            switch (result.Stars)
            {
                case 1:
                    total += config.xpBonusOneStar;
                    break;
                case 2:
                    total += config.xpBonusTwoStar;
                    break;
                case 3:
                    total += config.xpBonusThreeStar;
                    break;
            }

            total += config.xpDestructionPercentBonus * (int)(result.DestructionRatio * 100f);

            return total;
        }
    }
}