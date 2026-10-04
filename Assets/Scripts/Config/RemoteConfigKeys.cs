// RemoteConfigKeys — all Remote Config key constants, organised by category.
// M14 Remote Config + Feature Flags system.

namespace KingSmash.Config
{
    public static class RemoteConfigKeys
    {
        // ── GAMEPLAY ─────────────────────────────────────────────────────────
        public const string StartingKings          = "starting_kings";
        public const string MaxKings               = "max_kings";
        public const string KingLaunchPower        = "king_launch_power";
        public const string GravityMultiplier      = "gravity_multiplier";
        public const string DestructionMultiplier  = "destruction_multiplier";

        // ── ECONOMY ──────────────────────────────────────────────────────────
        public const string CoinRewardMultiplier   = "coin_reward_multiplier";
        public const string GemRewardMultiplier    = "gem_reward_multiplier";
        public const string UpgradeCostMultiplier  = "upgrade_cost_multiplier";

        // ── ADS ──────────────────────────────────────────────────────────────
        public const string RewardedAdEnabled                = "rewarded_ad_enabled";
        public const string InterstitialEnabled              = "interstitial_enabled";
        public const string InterstitialFrequency            = "interstitial_frequency";
        public const string RewardedContinueEnabled          = "rewarded_continue_enabled";
        public const string AdInterstitialMinLevels          = "ad_interstitial_min_levels";
        public const string AdMaxInterstitialsPerSession     = "ad_max_interstitials_per_session";
        public const string AdInterstitialMinSessionSeconds  = "ad_interstitial_min_session_seconds";
        public const string RewardedAdCooldown               = "rewarded_ad_cooldown";

        // ── PROGRESSION ──────────────────────────────────────────────────────
        public const string XpMultiplier              = "xp_multiplier";
        public const string LevelUnlockRequirements   = "level_unlock_requirements";

        // ── POWERUPS ─────────────────────────────────────────────────────────
        public const string PowerupCostMultiplier    = "powerup_cost_multiplier";
        public const string PowerupRewardMultiplier  = "powerup_reward_multiplier";

        // ── RETENTION ────────────────────────────────────────────────────────
        public const string DailyRewardEnabled           = "daily_reward_enabled";
        public const string MissionEnabled               = "mission_enabled";
        public const string AchievementEnabled           = "achievement_enabled";
        public const string MissionRefreshIntervalHours  = "mission_refresh_interval_hours";

        // ── FEATURE FLAGS ────────────────────────────────────────────────────
        public const string ShopEnabled              = "shop_enabled";
        public const string PowerupsEnabled          = "powerups_enabled";
        public const string NewUserTutorialEnabled   = "new_user_tutorial_enabled";
        public const string AdsEnabled               = "ads_enabled";
        public const string CloudSaveEnabled         = "cloud_save_enabled";
        public const string GoogleSignInEnabled      = "google_signin_enabled";

        // ── VERSIONING ───────────────────────────────────────────────────────
        public const string ConfigVersion = "config_version";
    }
}
