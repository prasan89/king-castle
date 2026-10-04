// IConfigService — contract for Remote Config + Feature Flags.
// M14: added IsFeatureEnabled, GetConfigVersion, OnConfigFetched.

using System.Threading.Tasks;
using KingSmash.Config;

namespace KingSmash.Services
{
    public interface IConfigService
    {
        // ── Lifecycle ────────────────────────────────────────────────────────

        void       Initialize();
        Task       FetchAsync();

        // ── Value accessors ──────────────────────────────────────────────────

        string     GetString(string key, string defaultValue = "");
        int        GetInt   (string key, int    defaultValue = 0);
        float      GetFloat (string key, float  defaultValue = 0f);
        bool       GetBool  (string key, bool   defaultValue = false);
        string     GetJson  (string key, string defaultJson  = "{}");

        // ── Feature Flags (M14) ──────────────────────────────────────────────

        /// <summary>
        /// Returns whether the given feature is currently enabled.
        /// Maps <see cref="FeatureFlag"/> values to their <see cref="KingSmash.Config.RemoteConfigKeys"/> keys.
        /// </summary>
        bool       IsFeatureEnabled(FeatureFlag feature);

        // ── Versioning (M14) ─────────────────────────────────────────────────

        /// <summary>Returns the current config bundle version string.</summary>
        string     GetConfigVersion();

        // ── Events (M14) ─────────────────────────────────────────────────────

        /// <summary>Fires on the calling thread after a successful fetch-and-activate.</summary>
        event System.Action OnConfigFetched;
    }
}
