using KingSmash.PowerUps;

namespace KingSmash.UI
{
    public class LevelResult
    {
        public int   LevelIndex;
        public int   Stars;
        public int   Score;
        public float DestructionRatio;     // 0..1
        public int   EnemiesDefeated;
        public int   TotalEnemies;
        public bool  QueenRescued;
        public long  CoinsEarned;
        public long  XPEarned;
        public bool  IsVictory;
        public string FailReason;
        public int   AttemptsRemaining;
        public PowerUpType[] PowerUpsUsed;
        public int   PowerUpActivationCount;
    }
}
