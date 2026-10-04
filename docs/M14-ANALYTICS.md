# King Smash M14 — Analytics & Observability Reference

## Overview

Milestone 14 (M14) introduces a comprehensive analytics and observability layer for King Smash. Every significant player action — from launching a king to completing a purchase — is captured as a typed, parameter-safe Firebase Analytics event. The system is layered: low-level `IAnalyticsService` implementations handle SDK dispatch and rate limiting, while bridge MonoBehaviours (`LevelAnalyticsBridge`, `GameplayAnalyticsBridge`, `AdAnalyticsBridge`, etc.) translate domain events into analytics calls without embedding tracking logic inside gameplay classes. A parallel `ICrashReportingService` / Firebase Crashlytics layer captures non-fatal errors and fatal crashes, enriched with structured player-context keys set at each session boundary.

All Firebase calls are gated behind `#if FIREBASE_ENABLED`. Without that scripting define symbol, the project compiles cleanly in CI and editor builds that lack the Firebase SDK — a `AnalyticsServiceMock` / `MockCrashReportingService` pair handles every call via `GameLogger.Debug` output instead. Remote Config is abstracted behind `IConfigService` with `RemoteConfigServiceMock` for development and `FirebaseRemoteConfigService` for production; feature flags are resolved via `IConfigService.GetBool(RemoteConfigKeys.*)` and surfaced through the `FeatureFlag` enum helper.

---

## Analytics Events

All event names are `snake_case` to match Firebase Analytics convention. Parameters use the compile-time constants in `AnalyticsParameters`.

### App & Session

| Event Name | Purpose | Parameters | When Triggered |
|---|---|---|---|
| `app_open` | Cold or warm app launch | _(none)_ | `GameBootstrap.Initialize()` |
| `session_start` | Active gameplay session begins | _(none)_ | App foreground / session boundary |
| `session_end` | Active gameplay session ends | _(none)_ | App background / session boundary |

### Onboarding & Account

| Event Name | Purpose | Parameters | When Triggered |
|---|---|---|---|
| `tutorial_start` | Player enters the tutorial | _(none)_ | First boot, tutorial screen |
| `tutorial_complete` | Player completes the tutorial | _(none)_ | Tutorial success callback |
| `onboarding_started` | Full onboarding flow begins | _(none)_ | First launch after install |
| `onboarding_completed` | Full onboarding flow completed | _(none)_ | Onboarding success callback |
| `account_created` | New account registered | _(none)_ | Auth service account creation |
| `account_linked` | Guest account linked to Google | _(none)_ | Auth service sign-in success |

### World

| Event Name | Purpose | Parameters | When Triggered |
|---|---|---|---|
| `world_viewed` | Player opens a world map | `world_id` | World selection screen |
| `world_unlocked` | Player unlocks a new world | `world_id` | Progression unlock |
| `world_completed` | Player finishes all levels in a world | `world_id` | Final level complete |

### Level Lifecycle

| Event Name | Purpose | Parameters | When Triggered |
|---|---|---|---|
| `level_start` | Level gameplay begins | `level_id`, `world_id`, `attempt_number` | `LevelAnalyticsBridge.HandleLevelStateChanged` |
| `level_complete` | Level finished with stars ≥ 1 | `level_id`, `world_id`, `stars`, `attempt_number`, `completion_time`, `remaining_kings` | `LevelAnalyticsBridge.HandleLevelEnded` |
| `level_failed` | Level ended with zero stars | `level_id`, `world_id`, `attempt_number`, `source` (reason) | `LevelAnalyticsBridge.HandleLevelEnded` |
| `level_retried` | Player retries failed level | `level_id`, `world_id`, `attempt_number` | Retry button handler |
| `level_abandoned` | Player quits mid-level | `level_id`, `world_id`, `attempt_number` | Quit/back button handler |

### Gameplay

| Event Name | Purpose | Parameters | When Triggered |
|---|---|---|---|
| `king_launched` | King projectile launched | `level_id`, `world_id`, `power` | `GameplayAnalyticsBridge` / `LaunchController` |
| `king_impact` | King collides with structure | `level_id`, `world_id`, `material` | Impact physics callback |
| `castle_destroyed` | Player destroys the enemy castle | `level_id`, `world_id` | Castle health reaches zero |
| `structure_destroyed` | A destructible structure collapses | `level_id`, `world_id`, `material` | Destruction controller |
| `enemy_defeated` | An enemy unit is defeated | `level_id`, `world_id`, `enemy_type` | Enemy health reaches zero |
| `boss_started` | Boss encounter begins | `level_id`, `world_id` | Boss intro sequence |
| `boss_hit` | Boss takes damage | `level_id`, `world_id` | Boss health callback |
| `boss_defeated` | Boss is defeated | `level_id`, `world_id` | Boss death callback |
| `queen_rescued` | Queen character is rescued | `level_id`, `world_id` | `QueenController.OnQueenRescued` |

### Power-Ups

| Event Name | Purpose | Parameters | When Triggered |
|---|---|---|---|
| `powerup_used` | Power-up consumed (pre-level) | `powerup_id`, `level_id`, `world_id` | Pre-level selection confirm |
| `powerup_activated` | Power-up effect activates in game | `powerup_id`, `level_id`, `world_id`, `quantity_before` | `PowerUpController.OnEffectStarted` |
| `powerup_purchased` | Power-up bought with coins/gems | `powerup_id`, `cost`, `source` | Shop / IAP purchase flow |
| `powerup_awarded` | Power-up granted as reward | `powerup_id`, `source`, `reward_type` | Daily reward / ad reward |
| `powerup_viewed` | Power-up details screen opened | `powerup_id` | Powerup detail screen |
| `powerup_selected` | Power-up chosen in pre-level screen | `powerup_id`, `level_id` | Pre-level selection screen |

### Progression

| Event Name | Purpose | Parameters | When Triggered |
|---|---|---|---|
| `king_level_up` | King reaches a new level | `player_level`, `level_id` | `KingProgressionService` |
| `king_stat_upgrade` | King stat upgraded | `stat_name`, `old_value`, `new_value`, `cost`, `player_level` | `KingUpgradeService` |
| `upgrade_purchase` | Generic upgrade purchased | `stat_name`, `cost` | Upgrade screen confirm |
| `king_upgrade_purchased` | King-specific upgrade confirmed | `stat_name`, `old_value`, `new_value`, `cost`, `player_level` | Upgrade service post-commit |
| `xp_earned` | XP added to player account | `amount`, `source` | Reward flow |
| `xp_gained` | Alias — XP credited post-level | `amount`, `level_id` | Level complete reward |
| `king_upgrade_started` | Upgrade flow initiated | `stat_name`, `player_level` | Upgrade screen open |
| `stat_upgraded` | Any stat upgraded (generic) | `stat_name`, `old_value`, `new_value` | Upgrade service |

### Economy

| Event Name | Purpose | Parameters | When Triggered |
|---|---|---|---|
| `coins_earned` | Coins credited to wallet | `currency`, `amount`, `source` | `CurrencyService` earn path |
| `coins_spent` | Coins deducted from wallet | `currency`, `amount`, `source` | `CurrencyService` spend path |
| `currency_earned` | Generic currency earn | `currency`, `amount`, `source` | `EconomyAnalyticsBridge.TrackCurrencyEarned` |
| `currency_spent` | Generic currency spend | `currency`, `amount`, `source` | `EconomyAnalyticsBridge.TrackCurrencySpent` |
| `reward_claimed` | Reward collected by player | `reward_type`, `reward_amount`, `source` | `RewardService` |

### Shop & IAP

| Event Name | Purpose | Parameters | When Triggered |
|---|---|---|---|
| `shop_opened` | Shop screen opened | _(none)_ | UI navigation |
| `product_viewed` | IAP product detail viewed | `product_id`, `product_type` | Shop product tap |
| `purchase_started` | Purchase flow initiated | `product_id`, `product_type` | Buy button tap |
| `purchase_pending` | OS purchase pending confirmation | `product_id`, `product_type` | Store service callback |
| `purchase_completed` | Purchase confirmed and entitlement granted | `product_id`, `product_type` | Receipt verification success |
| `purchase_failed` | Purchase failed or denied | `product_id`, `product_type` | Store service error callback |
| `purchase_cancelled` | Player cancelled purchase | `product_id`, `product_type` | Store cancel callback |
| `purchase_restored` | Existing purchase restored | `product_id`, `product_type` | Restore purchases flow |
| `upgrade_screen_opened` | King upgrade screen opened | _(none)_ | UI navigation |
| `powerup_shop_opened` | Power-up shop tab opened | _(none)_ | UI navigation |
| `pre_level_selection_opened` | Pre-level power-up screen opened | `level_id`, `world_id` | Level start flow |
| `pre_level_selection_confirmed` | Pre-level selection confirmed | `level_id`, `world_id` | Confirm button |
| `main_menu_opened` | Main menu loaded | _(none)_ | Scene transition |
| `settings_opened` | Settings screen opened | _(none)_ | UI navigation |
| `iap_started` | In-app purchase flow begins | `product_id`, `product_type` | `PurchaseAnalyticsBridge` |
| `iap_completed` | In-app purchase completed | `product_id`, `product_type` | `PurchaseAnalyticsBridge` |

### Ads

| Event Name | Purpose | Parameters | When Triggered |
|---|---|---|---|
| `ad_requested` | Ad request sent to network | `ad_type`, `placement` | Ad service load call |
| `ad_loaded` | Ad loaded and ready | `ad_type`, `placement` | Load callback success |
| `ad_started` | Ad began showing | `ad_type`, `placement` | Show callback |
| `ad_completed` | Ad watched to end | `ad_type`, `placement` | Completion callback |
| `ad_failed` | Ad failed to load or show | `ad_type`, `placement` | Error callback |
| `ad_skipped` | Player skipped ad (if skippable) | `ad_type`, `placement` | Skip callback |
| `rewarded_ad_started` | Rewarded ad started | `ad_type`, `placement` | Rewarded flow start |
| `rewarded_ad_completed` | Rewarded ad completed | `ad_type`, `placement` | Rewarded flow complete |
| `rewarded_ad_reward_granted` | Reward delivered after ad | `placement`, `reward_type` | `AdAnalyticsBridge.TrackAdRewardGranted` |
| `interstitial_shown` | Interstitial ad shown | `ad_type`, `placement` | Interstitial service show |
| `interstitial_failed` | Interstitial failed | `ad_type`, `placement` | Interstitial service error |
| `interstitial_frequency_blocked` | Interstitial suppressed by frequency cap | `placement` | `AdMobInterstitialService` policy check |
| `ad_impression` | Ad impression recorded | `ad_type`, `placement` | Impression callback |
| `ad_revenue` | Ad revenue event (MMP) | `ad_type`, `placement`, `amount` | Revenue callback |

### Network & Cloud

| Event Name | Purpose | Parameters | When Triggered |
|---|---|---|---|
| `cloud_save_started` | Cloud save operation started | `operation`, `is_online` | `CloudAnalyticsBridge` — SyncStatus.Syncing |
| `cloud_save_success` | Cloud save completed successfully | `operation`, `is_online` | `CloudAnalyticsBridge` — SyncStatus.Synced |
| `cloud_save_failed` | Cloud save failed | `operation`, `is_online`, `error_category` | `CloudAnalyticsBridge` — SyncStatus.Failed |
| `cloud_load_started` | Cloud load operation started | `operation`, `is_online` | `CloudAnalyticsBridge` — SyncStatus.Syncing |
| `cloud_load_success` | Cloud load completed successfully | `operation`, `is_online` | `CloudAnalyticsBridge` — SyncStatus.Synced |
| `cloud_load_failed` | Cloud load failed | `operation`, `is_online`, `error_category` | `CloudAnalyticsBridge` — SyncStatus.Failed |

### Retention & Milestones

| Event Name | Purpose | Parameters | When Triggered |
|---|---|---|---|
| `retention_day_open` | App opened on a given day-since-install bucket | `amount` (bucketed day) | `EconomyAnalyticsBridge.TrackRetentionOpen` |
| `first_launch` | Very first app launch tracked | _(none)_ | `ProgressionAnalyticsBridge` |
| `first_level_complete` | First level ever completed | _(none)_ | `ProgressionAnalyticsBridge` |
| `first_purchase` | First real-money purchase | _(none)_ | `PurchaseAnalyticsBridge` |
| `first_rewarded_ad` | First rewarded ad watched | _(none)_ | `AdAnalyticsBridge` |
| `first_upgrade` | First king stat upgraded | _(none)_ | `ProgressionAnalyticsBridge` |
| `daily_reward_claimed` | Daily login reward collected | `reward_type`, `reward_amount`, `streak` | `EconomyAnalyticsBridge.HandleDailyClaimed` |
| `daily_reward_viewed` | Daily reward screen opened | _(none)_ | UI navigation |
| `daily_reward_missed` | Day streak broken | `streak` | `DailyRewardService` |
| `daily_reward_streak_day` | Streak-day milestone reached | `streak` | `DailyRewardService` |
| `mission_viewed` | Mission list opened | _(none)_ | UI navigation |
| `mission_progressed` | Mission progress updated | `source` (mission id) | `MissionService` |
| `mission_completed` | Mission objectives met | `source` (mission id) | `MissionService` |
| `mission_claimed` | Mission reward collected | `reward_type`, `reward_amount`, `source` | `EconomyAnalyticsBridge.HandleMissionClaimed` |
| `achievement_viewed` | Achievements screen opened | _(none)_ | UI navigation |
| `achievement_unlocked` | Achievement earned | `source` (achievement id) | `MissionService` |
| `achievement_progressed` | Achievement progress updated | `source` (achievement id) | `MissionService` |
| `achievement_tier_claimed` | Multi-tier achievement tier collected | `source`, `amount` | `MissionService` |

---

## Crashlytics

King Smash uses `ICrashReportingService` (backed by `FirebaseCrashlyticsService` in production, `MockCrashReportingService` in development) to capture non-fatal errors and fatal crashes enriched with player context.

### Custom Keys

| Key | Value | Set When |
|---|---|---|
| `player_id` | Hashed player ID (16 hex chars) | `SetPlayerContext` / auth flow |
| `level` | Current level ID (integer) | Level start via `LevelAnalyticsBridge` |
| `world` | Current world ID (integer) | Level start via `LevelAnalyticsBridge` |
| `attempt` | Current attempt number | Level start via `LevelAnalyticsBridge` |
| `player_context` | Free-form context string | `SetPlayerContext("level=X world=Y attempt=Z")` |
| `environment` | `development` / `staging` / `production` | `GameBootstrap.Initialize` |
| `config_version` | Remote Config version string | `GameBootstrap.Initialize` |
| `app_version` | Unity `Application.version` | `FirebaseCrashlyticsService` constructor |
| `session_id` | Unique per-session GUID | `FirebaseCrashlyticsService` constructor |

### Error Categories

The `ErrorCategory` enum classifies every recorded error for dashboard filtering:

| Category | Use Case |
|---|---|
| `Network` | HTTP timeouts, DNS failures, unreachable endpoints |
| `Auth` | Sign-in failures, token refresh errors |
| `Backend` | Cloud Run / GCP API errors (4xx / 5xx) |
| `Firebase` | Firebase SDK internal errors |
| `Billing` | IAP store errors, receipt validation failures |
| `Ads` | Ad network load / show failures |
| `Asset` | Missing ScriptableObject resources, bundle load failures |
| `Save` | Local save read/write errors, cloud sync failures |
| `Configuration` | Remote Config fetch failures, missing keys |
| `Gameplay` | Unexpected game-state errors (null objects, state machine faults) |
| `Unknown` | Uncategorised / catch-all |

### Non-Fatal vs Fatal

- **Non-fatal** (`RecordError`): Use for recoverable, unexpected failures — cloud sync failed, ad network error, backend 5xx. Do NOT record expected conditions such as `SyncStatus.Offline` (no internet is normal).
- **Fatal** (`ForceCrash`): Use only during development / QA to validate the Crashlytics pipeline. Never call `ForceCrash` in response to a runtime error in production code.

---

## Remote Config Parameters

All keys are defined as compile-time constants in `RemoteConfigKeys`. Defaults are in `RemoteConfigDefaults.GetAll()`. Validation rules are enforced by `RemoteConfigValidator`.

| Key | Default | Min | Max | Description |
|---|---|---|---|---|
| `starting_kings` | `3` | `1` | `10` | Number of king projectiles available at level start |
| `max_kings` | `5` | `1` | `20` | Maximum kings a player can carry into a level |
| `king_launch_power` | `10.0` | `1.0` | `50.0` | Base launch force multiplier |
| `coin_reward_multiplier` | `1.0` | `0.1` | `10.0` | Multiplier applied to all coin rewards |
| `gem_reward_multiplier` | `1.0` | `0.1` | `10.0` | Multiplier applied to all gem rewards |
| `shop_enabled` | `true` | — | — | Master switch for the shop screen |
| `powerups_enabled` | `true` | — | — | Master switch for power-up system |
| `daily_reward_enabled` | `true` | — | — | Master switch for daily login rewards |
| `missions_enabled` | `true` | — | — | Master switch for mission system |
| `achievements_enabled` | `true` | — | — | Master switch for achievements |
| `ads_enabled` | `true` | — | — | Master switch for the ad system |
| `rewarded_continue_enabled` | `true` | — | — | Enable rewarded-ad continue-after-fail flow |
| `interstitial_ads_enabled` | `true` | — | — | Enable interstitial ads |
| `cloud_save_enabled` | `true` | — | — | Enable cloud save / restore |
| `google_signin_enabled` | `true` | — | — | Enable Google Sign-In |
| `interstitial_min_interval` | `120` | `30` | `600` | Minimum seconds between interstitial ads |
| `rewarded_ad_cooldown` | `30` | `0` | `300` | Cooldown seconds between rewarded ad offers |
| `ad_interstitial_min_levels` | `3` | `1` | `20` | Levels played before first interstitial |
| `ad_max_interstitials_per_session` | `5` | `1` | `20` | Maximum interstitials shown per session |
| `ad_interstitial_min_session_seconds` | `60` | `0` | `600` | Minimum session duration before first interstitial |
| `mission_refresh_interval_hours` | `24` | `1` | `168` | Hours between mission pool refresh |
| `achievement_event_batch_size` | `10` | `1` | `100` | Events to batch before flushing achievement progress |
| `cloud_sync_interval_seconds` | `300` | `60` | `3600` | Seconds between automatic cloud sync attempts |
| `conflict_resolution_strategy` | `Merged` | — | — | Cloud conflict resolution: `Local`, `Remote`, or `Merged` |
| `new_content_available` | `false` | — | — | Shows a content-update badge on the main menu |

---

## Feature Flags

Feature flags are thin wrappers over `IConfigService.GetBool(key, default)`. The `FeatureFlag` enum maps each flag to its Remote Config key for type-safe access.

| Flag | Key | Default | Description |
|---|---|---|---|
| `PowerUps` | `powerups_enabled` | `true` | Enable / disable the power-up system |
| `DailyRewards` | `daily_reward_enabled` | `true` | Enable / disable daily login rewards |
| `Missions` | `missions_enabled` | `true` | Enable / disable mission system |
| `Achievements` | `achievements_enabled` | `true` | Enable / disable achievements |
| `Shop` | `shop_enabled` | `true` | Enable / disable the shop screen |
| `RewardedContinue` | `rewarded_continue_enabled` | `true` | Enable rewarded-ad continue after fail |
| `InterstitialAds` | `interstitial_ads_enabled` | `true` | Enable interstitial ad placements |
| `CloudSave` | `cloud_save_enabled` | `true` | Enable cloud save and restore |
| `GoogleSignIn` | `google_signin_enabled` | `true` | Enable Google Sign-In auth flow |

---

## Privacy

### What Is NOT Collected

- No full names, email addresses, phone numbers, or device identifiers (IMEI, MAC, advertising ID is not explicitly read).
- No gameplay input sequences, touch coordinates, or device sensor data beyond what Firebase Analytics collects automatically.
- No financial data beyond product IDs and product type strings (no card details, no transaction amounts beyond what the OS store provides).
- No crash stack frames that contain personally identifiable information — log messages are sanitised before recording.

### User ID Hashing

Before any user ID is sent to Firebase Analytics, `FirebaseAnalyticsService.SetUserId` applies SHA-256 and truncates to the first 16 lowercase hexadecimal characters (64 bits of entropy). The raw Firebase Auth UID never leaves the device. The hashed ID is used only for funnel deduplication and cannot be reversed.

### No PII Policy

- `AnalyticsParameters` contains no keys named `token`, `password`, `credential`, `email`, `name`, or `phone`.
- All parameter values must be non-PII scalars (integers, floats, booleans) or short classification strings (e.g. `"rewarded"`, `"coins"`).
- `GameLogger` strips user-controlled strings from release builds (`LogLevel.Warning` minimum in non-editor, non-development builds).

---

## Environment Configuration

The active environment is set at build time via Unity Scripting Define Symbols:

| Symbol | Environment | Firebase Project | Use Case |
|---|---|---|---|
| `KING_SMASH_DEV` | `development` | `king-smash-dev` | Local development, feature branches |
| `KING_SMASH_STAGING` | `staging` | `king-smash-staging` | QA, pre-release validation |
| _(none)_ | `production` | `king-smash-prod` | App Store / Play Store release builds |

The environment string is written to Firebase Analytics as a user property (`environment`) and as a Crashlytics custom key on every session. It is also passed to `IAnalyticsService.SetEnvironment` from `GameBootstrap.RegisterServices`.

Swap `google-services.json` (Android) or `GoogleService-Info.plist` (iOS) in your CI/CD pipeline before each build to match the target environment. Never commit a production `google-services.json` to source control.

`FIREBASE_ENABLED` must also be added to Scripting Define Symbols to activate real Firebase SDK calls in `FirebaseAnalyticsService` and `FirebaseCrashlyticsService`.

---

## Debug Mode

### AnalyticsDebugOverlay

`AnalyticsDebugOverlay` is a `MonoBehaviour` that renders a semi-transparent 300 × 400 px box in the top-right corner of the screen showing the last 20 tracked analytics events.

- **Compilation guard**: the entire class is wrapped in `#if UNITY_EDITOR || KING_SMASH_DEV`. It is completely absent from production builds.
- **Static event**: `AnalyticsDebugOverlay.OnEventTracked(string eventName, string parameters)` — analytics service implementations call `AnalyticsDebugOverlay.NotifyEventTracked(name, paramsString)` after each event dispatch.
- **Usage**: attach to any scene GameObject. Enable or disable the overlay at runtime via the `_enabled` Inspector toggle.
- **Clear button**: clears the event queue in the overlay without affecting the analytics service.

### AnalyticsCrashTest

`AnalyticsCrashTest` is a `MonoBehaviour` that allows developers to trigger a forced crash to validate the Crashlytics pipeline.

- **Compilation guard**: wrapped in `#if UNITY_EDITOR || KING_SMASH_DEV`.
- **Activation**: set `_enableCrashTest = true` in the Inspector. Pressing Left Shift + C calls `ICrashReportingService.ForceCrash()`.
- **Visual warning**: when enabled, a bold red `CRASH TEST ENABLED — Shift+C to force crash` label is rendered at the bottom-left of the screen.
- **Never ship** with `_enableCrashTest = true`. The field defaults to `false`.
