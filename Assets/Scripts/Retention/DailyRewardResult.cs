using KingSmash.PowerUps;

namespace KingSmash.Retention
{
    public class DailyRewardResult
    {
        public bool        Success;
        public int         Day;
        public long        CoinsGranted;
        public int         GemsGranted;
        public PowerUpType PowerUpGranted;
        public int         PowerUpCount;
        public bool        IsTreasureChest;
        public string      ClaimId;
        public string      FailReason;
    }
}
