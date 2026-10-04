// FirebaseRemoteConfigService — implements IConfigService.
// Real implementation gated on #if FIREBASE_ENABLED.
// Stub compiles cleanly without the Firebase SDK.
// M14: added IsFeatureEnabled, GetConfigVersion, OnConfigFetched,
//      timeout handling, and RemoteConfigValidator on all numeric getters.

using System.Collections.Generic;
using System.Threading.Tasks;
using KingSmash.Config;
using KingSmash.Core;

namespace KingSmash.Services.Firebase
{
    public class FirebaseRemoteConfigService : IConfigService
    {
        // ── Fetch timeout ────────────────────────────────────────────────────
        private const float FetchTimeoutSeconds = 10f;

        // ── Defaults ─────────────────────────────────────────────────────────
        // Start from the full M14 defaults, then layer any legacy / extra keys
        // that were present before M14 but are not yet in RemoteConfigKeys.
        private readonly Dictionary<string, string> _defaults;

        public FirebaseRemoteConfigService()
        {
            _defaults = RemoteConfigDefaults.GetAll();

            // ── Legacy / extra M9–M11 keys not covered by RemoteConfigKeys ───
            // These remain here for backward compatibility with older Firebase
            // console entries. Add to RemoteConfigKeys in a future milestone
            // when they are fully migrated.
            _defaults["coins_multiplier"]              = "1.0";   // legacy alias
            _defaults["new_content_available"]         = "false";
            _defaults["interstitial_min_interval"]     = "120";
            _defaults["achievement_event_batch_size"]  = "10";
            _defaults["cloud_sync_interval_seconds"]   = "300";
            _defaults["conflict_resolution_strategy"]  = "Merged";
        }

        // ── IConfigService: OnConfigFetched ───────────────────────────────────
        public event System.Action OnConfigFetched;

        // ── IConfigService: Lifecycle ─────────────────────────────────────────

        public void Initialize()
        {
#if FIREBASE_ENABLED
            var defaultObjects = new Dictionary<string, object>();
            foreach (var kv in _defaults)
                defaultObjects[kv.Key] = kv.Value;

            global::Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance
                .SetDefaultsAsync(defaultObjects)
                .ContinueWith(t =>
                {
                    if (t.IsFaulted)
                        GameLogger.Error("FirebaseRemoteConfig",
                            $"SetDefaults failed: {t.Exception?.Message}");
                    else
                        GameLogger.Info("FirebaseRemoteConfig", "Defaults applied.");
                });
#else
            GameLogger.Info("FirebaseRemoteConfig",
                "Initialized with local defaults (Firebase disabled).");
#endif
        }

        public async Task FetchAsync()
        {
#if FIREBASE_ENABLED
            try
            {
                var fetchTask = global::Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance
                    .FetchAndActivateAsync();

                var timeoutTask = Task.Delay((int)(FetchTimeoutSeconds * 1000));
                var completed   = await Task.WhenAny(fetchTask, timeoutTask);

                if (completed == timeoutTask)
                {
                    GameLogger.LogWarning("FirebaseRemoteConfig",
                        $"FetchAsync timed out after {FetchTimeoutSeconds}s — using cached/default values.");
                    return;
                }

                // Propagate any exception from the fetch task.
                await fetchTask;

                GameLogger.Info("FirebaseRemoteConfig", "Fetch-and-activate complete.");
                OnConfigFetched?.Invoke();
            }
            catch (System.Exception ex)
            {
                GameLogger.Error("FirebaseRemoteConfig", $"FetchAsync failed: {ex.Message}");
            }
#else
            GameLogger.Info("FirebaseRemoteConfig", "Fetch skipped (Firebase disabled).");
            OnConfigFetched?.Invoke();
            await Task.CompletedTask;
#endif
        }

        // ── IConfigService: Value accessors ───────────────────────────────────

        public string GetString(string key, string defaultValue = "")
        {
#if FIREBASE_ENABLED
            var value = global::Firebase.RemoteConfig.FirebaseRemoteConfig
                            .DefaultInstance.GetValue(key);
            return string.IsNullOrEmpty(value.StringValue) ? defaultValue : value.StringValue;
#else
            return _defaults.TryGetValue(key, out var v) ? v : defaultValue;
#endif
        }

        public int GetInt(string key, int defaultValue = 0)
        {
            int raw = int.TryParse(
                GetString(key, defaultValue.ToString()),
                out var parsed) ? parsed : defaultValue;

            return RemoteConfigValidator.ValidateInt(
                key, raw, defaultValue, min: int.MinValue + 1, max: int.MaxValue - 1);
        }

        public float GetFloat(string key, float defaultValue = 0f)
        {
            float raw = float.TryParse(
                GetString(key, defaultValue.ToString(System.Globalization.CultureInfo.InvariantCulture)),
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture,
                out var parsed) ? parsed : defaultValue;

            return RemoteConfigValidator.ValidateFloat(
                key, raw, defaultValue, min: float.MinValue / 2f, max: float.MaxValue / 2f).Value;
        }

        public bool GetBool(string key, bool defaultValue = false)
        {
            string raw = GetString(key, defaultValue.ToString());
            return RemoteConfigValidator.ValidateBool(key, raw, defaultValue);
        }

        public string GetJson(string key, string defaultJson = "{}")
            => GetString(key, defaultJson);

        // ── IConfigService: Feature Flags (M14) ───────────────────────────────

        public bool IsFeatureEnabled(FeatureFlag feature)
        {
            string key = FeatureFlagToKey(feature);
            return GetBool(key, defaultValue: true);
        }

        // ── IConfigService: Versioning (M14) ──────────────────────────────────

        public string GetConfigVersion()
            => GetString(RemoteConfigKeys.ConfigVersion, "1");

        // ── Helpers ───────────────────────────────────────────────────────────

        private static string FeatureFlagToKey(FeatureFlag feature) => feature switch
        {
            FeatureFlag.PowerUps          => RemoteConfigKeys.PowerupsEnabled,
            FeatureFlag.DailyRewards      => RemoteConfigKeys.DailyRewardEnabled,
            FeatureFlag.Missions          => RemoteConfigKeys.MissionEnabled,
            FeatureFlag.Achievements      => RemoteConfigKeys.AchievementEnabled,
            FeatureFlag.Shop              => RemoteConfigKeys.ShopEnabled,
            FeatureFlag.RewardedContinue  => RemoteConfigKeys.RewardedContinueEnabled,
            FeatureFlag.InterstitialAds   => RemoteConfigKeys.InterstitialEnabled,
            FeatureFlag.Tutorial          => RemoteConfigKeys.NewUserTutorialEnabled,
            FeatureFlag.CloudSave         => RemoteConfigKeys.CloudSaveEnabled,
            FeatureFlag.GoogleSignIn      => RemoteConfigKeys.GoogleSignInEnabled,
            _                             => RemoteConfigKeys.AdsEnabled, // safe fallback
        };
    }
}
