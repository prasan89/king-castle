namespace KingSmash
{
    public static class AnalyticsEvents
    {
        // ── App ──────────────────────────────────────────────────────────────
        public const string AppOpen             = "app_open";

        // ── Session ──────────────────────────────────────────────────────────
        public const string SessionStart        = "session_start";
        public const string SessionEnd          = "session_end";

        // ── Onboarding / Tutorial ────────────────────────────────────────────
        public const string TutorialStart       = "tutorial_start";
        public const string TutorialComplete    = "tutorial_complete";
        public const string OnboardingStarted   = "onboarding_started";
        public const string OnboardingCompleted = "onboarding_completed";
        public const string AccountCreated      = "account_created";
        public const string AccountLinked       = "account_linked";

        // ── World ────────────────────────────────────────────────────────────
        public const string WorldViewed         = "world_viewed";
        public const string WorldUnlocked       = "world_unlocked";
        public const string WorldCompleted      = "world_completed";

        // ── Level ────────────────────────────────────────────────────────────
        public const string LevelStart          = "level_start";
        public const string LevelComplete       = "level_complete";
        public const string LevelFailed         = "level_failed";
        public const string LevelRetried        = "level_retried";
        public const string LevelAbandoned      = "level_abandoned";

        // ── Gameplay ─────────────────────────────────────────────────────────
        public const string KingLaunched        = "king_launched";
        public const string KingImpact          = "king_impact";
        public const string CastleDestroyed     = "castle_destroyed";
        public const string StructureDestroyed  = "structure_destroyed";
        public const string EnemyDefeated       = "enemy_defeated";
        public const string BossStarted         = "boss_started";
        public const string BossHit             = "boss_hit";
        public const string BossDefeated        = "boss_defeated";
        public const string QueenRescued        = "queen_rescued";

        // ── Power-Ups ────────────────────────────────────────────────────────
        public const string PowerupUsed         = "powerup_used";
        public const string PowerUpActivated    = "powerup_activated";
        public const string PowerUpPurchased    = "powerup_purchased";
        public const string PowerUpAwarded      = "powerup_awarded";
        public const string PowerUpViewed       = "powerup_viewed";
        public const string PowerUpSelected     = "powerup_selected";

        // ── Progression ──────────────────────────────────────────────────────
        public const string KingLevelUp         = "king_level_up";
        public const string KingStatUpgrade     = "king_stat_upgrade";
        public const string UpgradePurchase     = "upgrade_purchase";
        public const string KingUpgradePurchased = "king_upgrade_purchased";
        public const string XpEarned            = "xp_earned";
        public const string XpGained            = "xp_gained";
        public const string KingUpgradeStarted  = "king_upgrade_started";
        public const string StatUpgraded        = "stat_upgraded";

        // ── Economy ──────────────────────────────────────────────────────────
        public const string CoinsEarned         = "coins_earned";
        public const string CoinsSpent          = "coins_spent";
        public const string CurrencyEarned      = "currency_earned";
        public const string CurrencySpent       = "currency_spent";
        public const string RewardClaimed       = "reward_claimed";

        // ── Shop / UI ────────────────────────────────────────────────────────
        public const string ShopOpened                  = "shop_opened";
        public const string ProductViewed               = "product_viewed";
        public const string PurchaseStarted             = "purchase_started";
        public const string PurchasePending             = "purchase_pending";
        public const string PurchaseCompleted           = "purchase_completed";
        public const string PurchaseFailed              = "purchase_failed";
        public const string PurchaseCancelled           = "purchase_cancelled";
        public const string PurchaseRestored            = "purchase_restored";
        public const string UpgradeScreenOpened         = "upgrade_screen_opened";
        public const string PowerupShopOpened           = "powerup_shop_opened";
        public const string PreLevelSelectionOpened     = "pre_level_selection_opened";
        public const string PreLevelSelectionConfirmed  = "pre_level_selection_confirmed";
        public const string MainMenuOpened              = "main_menu_opened";
        public const string SettingsOpened              = "settings_opened";

        // ── Ads ──────────────────────────────────────────────────────────────
        public const string AdRequested                   = "ad_requested";
        public const string AdLoaded                      = "ad_loaded";
        public const string AdStarted                     = "ad_started";
        public const string AdCompleted                   = "ad_completed";
        public const string AdFailed                      = "ad_failed";
        public const string AdSkipped                     = "ad_skipped";
        public const string RewardedAdStarted             = "rewarded_ad_started";
        public const string RewardedAdCompleted           = "rewarded_ad_completed";
        public const string RewardedAdRewardGranted       = "rewarded_ad_reward_granted";
        public const string InterstitialShown             = "interstitial_shown";
        public const string InterstitialFailed            = "interstitial_failed";
        public const string InterstitialFrequencyBlocked  = "interstitial_frequency_blocked";
        public const string AdImpression                  = "ad_impression";
        public const string AdRevenue                     = "ad_revenue";
        public const string IapStarted                    = "iap_started";
        public const string IapCompleted                  = "iap_completed";

        // ── Network / Cloud ──────────────────────────────────────────────────
        public const string CloudSaveStarted    = "cloud_save_started";
        public const string CloudSaveSuccess    = "cloud_save_success";
        public const string CloudSaveFailed     = "cloud_save_failed";
        public const string CloudLoadStarted    = "cloud_load_started";
        public const string CloudLoadSuccess    = "cloud_load_success";
        public const string CloudLoadFailed     = "cloud_load_failed";

        // ── Retention / Daily / Missions / Achievements ──────────────────────
        public const string DailyRewardClaimed     = "daily_reward_claimed";
        public const string DailyRewardViewed      = "daily_reward_viewed";
        public const string DailyRewardMissed      = "daily_reward_missed";
        public const string DailyRewardStreakDay   = "daily_reward_streak_day";
        public const string MissionViewed          = "mission_viewed";
        public const string MissionProgressed      = "mission_progressed";
        public const string MissionCompleted       = "mission_completed";
        public const string MissionClaimed         = "mission_claimed";
        public const string AchievementViewed      = "achievement_viewed";
        public const string AchievementUnlocked    = "achievement_unlocked";
        public const string AchievementProgressed  = "achievement_progressed";
        public const string AchievementTierClaimed = "achievement_tier_claimed";
        public const string RetentionDayOpen       = "retention_day_open";
        public const string FirstLaunch            = "first_launch";
        public const string FirstLevelComplete     = "first_level_complete";
        public const string FirstPurchase          = "first_purchase";
        public const string FirstRewardedAd        = "first_rewarded_ad";
        public const string FirstUpgrade           = "first_upgrade";
    }
}
