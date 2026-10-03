namespace KingSmash.Services
{
    public interface IAnalyticsService
    {
        void LogEvent(string eventName, params (string key, object value)[] parameters);
        void SetUserProperty(string key, string value);
        void SetUserId(string userId);
    }
}
