using KingSmash.Core;

namespace KingSmash.Services
{
    public class AnalyticsServiceMock : IAnalyticsService
    {
        public void LogEvent(string eventName, params (string key, object value)[] parameters)
        {
            if (parameters.Length == 0)
            {
                GameLogger.Debug("Analytics", $"Event: {eventName}");
                return;
            }
            var paramStr = string.Join(", ", System.Array.ConvertAll(parameters, p => $"{p.key}={p.value}"));
            GameLogger.Debug("Analytics", $"Event: {eventName} | {paramStr}");
        }

        public void SetUserProperty(string key, string value)
            => GameLogger.Debug("Analytics", $"SetUserProperty: {key}={value}");

        public void SetUserId(string userId)
            => GameLogger.Debug("Analytics", $"SetUserId: {userId}");
    }
}
