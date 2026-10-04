using System;
using KingSmash.PowerUps;

namespace KingSmash.Ads
{
    [Serializable]
    public class AdRewardResult
    {
        public bool WasRewarded;
        public AdPlacement Placement;
        public string RewardClaimId;
        public long CoinsGranted;
        public int GemsGranted;
        public PowerUpType PowerUpType;
        public int PowerUpCount;

        public static AdRewardResult Unavailable(AdPlacement placement) => new AdRewardResult
        {
            WasRewarded = false,
            Placement = placement,
            RewardClaimId = string.Empty,
            CoinsGranted = 0,
            GemsGranted = 0,
            PowerUpType = PowerUpType.None,
            PowerUpCount = 0
        };

        public static AdRewardResult Declined(AdPlacement placement) => new AdRewardResult
        {
            WasRewarded = false,
            Placement = placement,
            RewardClaimId = string.Empty,
            CoinsGranted = 0,
            GemsGranted = 0,
            PowerUpType = PowerUpType.None,
            PowerUpCount = 0
        };
    }
}
