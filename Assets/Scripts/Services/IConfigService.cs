using System.Threading.Tasks;

namespace KingSmash.Services
{
    public interface IConfigService
    {
        void Initialize();
        Task FetchAsync();
        string GetString(string key, string defaultValue = "");
        int GetInt(string key, int defaultValue = 0);
        float GetFloat(string key, float defaultValue = 0f);
        bool GetBool(string key, bool defaultValue = false);
        string GetJson(string key, string defaultJson = "{}");
    }
}
