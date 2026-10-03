using System;
namespace KingSmash.Levels
{
    [Serializable]
    public class StarThresholds
    {
        // Each threshold is a minimum normalized value (0..1) required to EARN that dimension.
        // All three-star dimensions must be met for 3 stars, etc.
        public float oneStar_destructionMin  = 0.30f;
        public bool  oneStar_mustRescueQueen = true;

        public float twoStar_destructionMin  = 0.60f;
        public bool  twoStar_mustRescueQueen = true;
        public float twoStar_enemyRatioMin   = 0.50f;  // enemies defeated / total enemies

        public float threeStar_destructionMin = 0.90f;
        public bool  threeStar_mustRescueQueen = true;
        public float threeStar_enemyRatioMin   = 1.00f;
        public int   threeStar_attemptsRemaining = 1;  // must have >= this many attempts left
    }

    public static class StarCalculator
    {
        public static int Calculate(
            StarThresholds thresholds,
            float destructionRatio,
            float enemyRatio,
            bool queenRescued,
            int attemptsRemaining)
        {
            if (thresholds == null) return 0;

            // 3-star: all conditions met
            if (destructionRatio    >= thresholds.threeStar_destructionMin  &&
                (!thresholds.threeStar_mustRescueQueen || queenRescued)      &&
                enemyRatio          >= thresholds.threeStar_enemyRatioMin    &&
                attemptsRemaining   >= thresholds.threeStar_attemptsRemaining)
                return 3;

            // 2-star
            if (destructionRatio >= thresholds.twoStar_destructionMin  &&
                (!thresholds.twoStar_mustRescueQueen || queenRescued)   &&
                enemyRatio       >= thresholds.twoStar_enemyRatioMin)
                return 2;

            // 1-star
            if (destructionRatio >= thresholds.oneStar_destructionMin &&
                (!thresholds.oneStar_mustRescueQueen || queenRescued))
                return 1;

            return 0;
        }
    }
}
