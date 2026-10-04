// RemoteConfigServiceMock — full in-editor / test implementation of IConfigService.
// M14: expanded to cover all RemoteConfigDefaults keys, plus feature-flag and
//      versioning helpers.

using System.Collections.Generic;
using System.Threading.Tasks;
using KingSmash.Config;
using KingSmash.Core;

namespace KingSmash.Services
{
    public class RemoteConfigServiceMock : IConfigService
    {
        // ── IConfigService: OnConfigFetched ───────────────────────────────────
        public event System.Action OnConfigFetched;

        // ── Internal store ────────────────────────────────────────────────────
        // Initialised from the canonical defaults so the mock always mirrors
        // what a freshly-fetched Firebase response would look like.
        private readonly Dictionary<string, string> _values;

        public RemoteConfigServiceMock()
        {
            _values = RemoteConfigDefaults.GetAll();

            // Legacy keys kept for backward compatibility with older test code.
            _values["coins_multiplier"]             = "1.0";
            _values["new_content_available"]        = "false";
            _values["interstitial_min_interval"]    = "120";
            _values["achievement_event_batch_size"] = "10";
            _values["cloud_sync_interval_seconds"]  = "300";
            _values["conflict_resolution_strategy"] = "Merged";
        }

        // ── IConfigService: Lifecycle ─────────────────────────────────────────

        public void Initialize()
            => GameLogger.Info("RemoteConfigMock", "Initialized with defaults.");

        public Task FetchAsync()
        {
            GameLogger.Info("RemoteConfigMock", "Fetch complete (mock).");
            OnConfigFetched?.Invoke();
            return Task.CompletedTask;
        }

        // ── IConfigService: Value accessors ───────────────────────────────────

        public string GetString(string key, string defaultValue = "")
            => _values.TryGetValue(key, out var v) ? v : defaultValue;

        public int GetInt(string key, int defaultValue = 0)
            => int.TryParse(GetString(key, defaultValue.ToString()), out var v) ? v : defaultValue;

        public float GetFloat(string key, float defaultValue = 0f)
            => float.TryParse(
                GetString(key, defaultValue.ToString(System.Globalization.CultureInfo.InvariantCulture)),
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture,
                out var v) ? v : defaultValue;

        public bool GetBool(string key, bool defaultValue = false)
            => bool.TryParse(GetString(key, defaultValue.ToString()), out var v) ? v : defaultValue;

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

        // ── Test helpers ──────────────────────────────────────────────────────

        /// <summary>
        /// Overrides a single key in the mock's value store.
        /// Useful for writing unit tests that exercise specific config branches.
        /// </summary>
        public void SetValue(string key, string value)
            => _values[key] = value;

        /// <summary>Convenience overload for bool feature-flag overrides.</summary>
        public void SetFeatureEnabled(FeatureFlag feature, bool enabled)
            => SetValue(FeatureFlagToKey(feature), enabled.ToString().ToLowerInvariant());

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
            _                             => RemoteConfigKeys.AdsEnabled,
        };
    }
}
