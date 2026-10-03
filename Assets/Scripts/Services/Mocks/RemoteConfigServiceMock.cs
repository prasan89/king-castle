using System.Collections.Generic;
using System.Threading.Tasks;
using KingSmash.Core;

namespace KingSmash.Services
{
    public class RemoteConfigServiceMock : IConfigService
    {
        private readonly Dictionary<string, string> _values = new()
        {
            ["ads_enabled"] = "true",
            ["daily_reward_enabled"] = "true",
            ["coins_multiplier"] = "1.0",
            ["new_content_available"] = "false",
        };

        public void Initialize() => GameLogger.Info("RemoteConfigMock", "Initialized with defaults.");

        public Task FetchAsync()
        {
            GameLogger.Info("RemoteConfigMock", "Fetch complete (mock).");
            return Task.CompletedTask;
        }

        public string GetString(string key, string defaultValue = "") =>
            _values.TryGetValue(key, out var v) ? v : defaultValue;

        public int GetInt(string key, int defaultValue = 0) =>
            int.TryParse(GetString(key), out var v) ? v : defaultValue;

        public float GetFloat(string key, float defaultValue = 0f) =>
            float.TryParse(GetString(key), out var v) ? v : defaultValue;

        public bool GetBool(string key, bool defaultValue = false) =>
            bool.TryParse(GetString(key), out var v) ? v : defaultValue;

        public string GetJson(string key, string defaultJson = "{}") =>
            GetString(key, defaultJson);
    }
}
