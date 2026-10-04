// RemoteConfigDefaults — safe default values for every Remote Config key.
// M14 Remote Config + Feature Flags system.

using System.Collections.Generic;

namespace KingSmash.Config
{
    public static class RemoteConfigDefaults
    {
        /// <summary>
        /// Returns a dictionary containing safe default values for every
        /// Remote Config key defined in <see cref="RemoteConfigKeys"/>.
        /// All values are stored as strings to match Firebase's wire format.
        /// </summary>
        public static Dictionary<string, string> GetAll() => new()
        {
            // ── GAMEPLAY ─────────────────────────────────────────────────────
            [RemoteConfigKeys.StartingKings]         = "3",
            [RemoteConfigKeys.MaxKings]              = "3",
            [RemoteConfigKeys.KingLaunchPower]       = "1.0",
            [RemoteConfigKeys.GravityMultiplier]     = "1.0",
            [RemoteConfigKeys.DestructionMultiplier] = "1.0",

            // ── ECONOMY ──────────────────────────────────────────────────────
            [RemoteConfigKeys.CoinRewardMultiplier]  = "1.0",
            [RemoteConfigKeys.GemRewardMultiplier]   = "1.0",
            [RemoteConfigKeys.UpgradeCostMultiplier] = "1.0",

            // ── ADS ──────────────────────────────────────────────────────────
            [RemoteConfigKeys.RewardedAdEnabled]               = "true",
            [RemoteConfigKeys.InterstitialEnabled]             = "true",
            [RemoteConfigKeys.InterstitialFrequency]           = "3",
            [RemoteConfigKeys.RewardedContinueEnabled]         = "true",
            [RemoteConfigKeys.AdInterstitialMinLevels]         = "3",
            [RemoteConfigKeys.AdMaxInterstitialsPerSession]    = "5",
            [RemoteConfigKeys.AdInterstitialMinSessionSeconds] = "60",
            [RemoteConfigKeys.RewardedAdCooldown]              = "30",

            // ── PROGRESSION ──────────────────────────────────────────────────
            [RemoteConfigKeys.XpMultiplier]             = "1.0",
            [RemoteConfigKeys.LevelUnlockRequirements]  = "stars",

            // ── POWERUPS ─────────────────────────────────────────────────────
            [RemoteConfigKeys.PowerupCostMultiplier]   = "1.0",
            [RemoteConfigKeys.PowerupRewardMultiplier] = "1.0",

            // ── RETENTION ────────────────────────────────────────────────────
            [RemoteConfigKeys.DailyRewardEnabled]          = "true",
            [RemoteConfigKeys.MissionEnabled]              = "true",
            [RemoteConfigKeys.AchievementEnabled]          = "true",
            [RemoteConfigKeys.MissionRefreshIntervalHours] = "24",

            // ── FEATURE FLAGS ────────────────────────────────────────────────
            [RemoteConfigKeys.ShopEnabled]            = "true",
            [RemoteConfigKeys.PowerupsEnabled]        = "true",
            [RemoteConfigKeys.NewUserTutorialEnabled] = "true",
            [RemoteConfigKeys.AdsEnabled]             = "true",
            [RemoteConfigKeys.CloudSaveEnabled]       = "true",
            [RemoteConfigKeys.GoogleSignInEnabled]    = "true",

            // ── VERSIONING ───────────────────────────────────────────────────
            [RemoteConfigKeys.ConfigVersion] = "1",
        };
    }
}
