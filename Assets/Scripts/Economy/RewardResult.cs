using System;
using KingSmash.PowerUps;

namespace KingSmash.Economy
{
    [Serializable]
    public class RewardResult
    {
        public long         coins;
        public int          gems;
        public long         xp;
        public RewardSource source;
        public PowerUpType? powerUpType;
        public int          powerUpCount;

        public bool HasPowerUp => powerUpCount > 0;

        public static RewardResult Empty => new RewardResult();
    }
}
