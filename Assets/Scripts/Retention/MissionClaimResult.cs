using KingSmash.Economy;

namespace KingSmash.Retention
{
    public class MissionClaimResult
    {
        public bool         Success;
        public string       MissionId;
        public RewardResult Reward;
        public string       FailReason;
    }
}
