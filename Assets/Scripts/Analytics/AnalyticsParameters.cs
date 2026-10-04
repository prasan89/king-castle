namespace KingSmash.Analytics
{
    /// <summary>
    /// Compile-time constants for all analytics parameter keys.
    /// Use these instead of raw strings to prevent typos and enable refactoring.
    /// All keys are snake_case to match Firebase Analytics convention.
    /// </summary>
    public static class AnalyticsParameters
    {
        // ── Level / World ─────────────────────────────────────────────────────
        public const string LevelId           = "level_id";
        public const string WorldId           = "world_id";
        public const string AttemptNumber     = "attempt_number";
        public const string Stars             = "stars";
        public const string Score             = "score";
        public const string CompletionTime    = "completion_time";
        public const string RemainingKings    = "remaining_kings";
        public const string DestructionRatio  = "destruction_ratio";

        // ── Gameplay ──────────────────────────────────────────────────────────
        public const string EnemyType         = "enemy_type";
        public const string Material          = "material";
        public const string Power             = "power";

        // ── Power-Ups ─────────────────────────────────────────────────────────
        public const string PowerupId         = "powerup_id";
        public const string Source            = "source";
        public const string QuantityBefore    = "quantity_before";

        // ── Progression / Upgrades ────────────────────────────────────────────
        public const string StatName          = "stat_name";
        public const string OldValue          = "old_value";
        public const string NewValue          = "new_value";
        public const string Cost              = "cost";
        public const string PlayerLevel       = "player_level";

        // ── Economy ───────────────────────────────────────────────────────────
        public const string Currency          = "currency";
        public const string Amount            = "amount";
        public const string RewardType        = "reward_type";
        public const string RewardAmount      = "reward_amount";
        public const string Streak            = "streak";

        // ── Shop / IAP ────────────────────────────────────────────────────────
        public const string ProductId         = "product_id";
        public const string ProductType       = "product_type";

        // ── Ads ───────────────────────────────────────────────────────────────
        public const string Placement         = "placement";
        public const string AdType            = "ad_type";

        // ── Network / Cloud ───────────────────────────────────────────────────
        public const string Operation         = "operation";
        public const string ErrorCategory     = "error_category";
        public const string RetryCount        = "retry_count";
        public const string IsOnline          = "is_online";

        // ── App / Config ──────────────────────────────────────────────────────
        public const string AppVersion        = "app_version";
        public const string ConfigVersion     = "config_version";
        public const string Environment       = "environment";
    }
}
