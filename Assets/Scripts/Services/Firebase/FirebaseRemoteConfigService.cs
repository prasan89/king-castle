// FirebaseRemoteConfigService — implements IConfigService.
// Real implementation gated on #if FIREBASE_ENABLED.
// Stub compiles without the Firebase SDK.

using System.Collections.Generic;
using System.Threading.Tasks;
using KingSmash.Core;

namespace KingSmash.Services.Firebase
{
    public class FirebaseRemoteConfigService : IConfigService
    {
        private readonly Dictionary<string, string> _defaults = new()
        {
            // ── RemoteConfigServiceMock keys ────────────────────────────────
            ["ads_enabled"]            = "true",
            ["daily_reward_enabled"]   = "true",
            ["coins_multiplier"]       = "1.0",
            ["new_content_available"]  = "false",

            // ── M10 keys ────────────────────────────────────────────────────
            ["interstitial_min_interval"]      = "120",
            ["rewarded_ad_cooldown"]           = "30",
            ["mission_refresh_interval_hours"] = "24",
            ["achievement_event_batch_size"]   = "10",

            // ── Ad policy keys ──────────────────────────────────────────────
            ["ad_interstitial_min_levels"]          = "3",
            ["ad_max_interstitials_per_session"]     = "5",
            ["ad_interstitial_min_session_seconds"]  = "60",

            // ── M11 keys ────────────────────────────────────────────────────
            ["cloud_save_enabled"]           = "true",
            ["cloud_sync_interval_seconds"]  = "300",
            ["google_signin_enabled"]        = "true",
            ["conflict_resolution_strategy"] = "Merged",
        };

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
                        GameLogger.Error("FirebaseRemoteConfig", $"SetDefaults failed: {t.Exception?.Message}");
                    else
                        GameLogger.Info("FirebaseRemoteConfig", "Defaults applied.");
                });
#else
            GameLogger.Info("FirebaseRemoteConfig", "Initialized with local defaults (Firebase disabled).");
#endif
        }

        public async Task FetchAsync()
        {
#if FIREBASE_ENABLED
            try
            {
                await global::Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance
                    .FetchAndActivateAsync();
                GameLogger.Info("FirebaseRemoteConfig", "Fetch-and-activate complete.");
            }
            catch (System.Exception ex)
            {
                GameLogger.Error("FirebaseRemoteConfig", $"FetchAsync failed: {ex.Message}");
            }
#else
            GameLogger.Info("FirebaseRemoteConfig", "Fetch skipped (Firebase disabled).");
            await Task.CompletedTask;
#endif
        }

        public string GetString(string key, string defaultValue = "")
        {
#if FIREBASE_ENABLED
            var value = global::Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.GetValue(key);
            return string.IsNullOrEmpty(value.StringValue) ? defaultValue : value.StringValue;
#else
            return _defaults.TryGetValue(key, out var v) ? v : defaultValue;
#endif
        }

        public int GetInt(string key, int defaultValue = 0)
            => int.TryParse(GetString(key, defaultValue.ToString()), out var v) ? v : defaultValue;

        public float GetFloat(string key, float defaultValue = 0f)
            => float.TryParse(
                GetString(key, defaultValue.ToString(System.Globalization.CultureInfo.InvariantCulture)),
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out var v) ? v : defaultValue;

        public bool GetBool(string key, bool defaultValue = false)
            => bool.TryParse(GetString(key, defaultValue.ToString()), out var v) ? v : defaultValue;

        public string GetJson(string key, string defaultJson = "{}")
            => GetString(key, defaultJson);
    }
}
