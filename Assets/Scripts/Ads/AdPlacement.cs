namespace KingSmash.Ads
{
    public enum AdPlacement
    {
        LevelCompleteDoubleReward,
        LevelFailedExtraAttempt,
        FreePowerUp,
        DailyBonusSlot,
        InterstitialLevelTransition
    }

    public enum AdType
    {
        Rewarded,
        Interstitial,
        Banner
    }

    public enum AdAvailability
    {
        Available,
        Loading,
        Unavailable,
        RemoveAdsPurchased,
        Offline,
        Disabled
    }
}
