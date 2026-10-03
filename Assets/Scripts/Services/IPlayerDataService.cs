using System.Threading.Tasks;
using KingSmash.Save;

namespace KingSmash.Services
{
    public interface IPlayerDataService
    {
        Task<SaveData> LoadPlayerDataAsync(string playerId);
        Task SavePlayerDataAsync(string playerId, SaveData data);
        Task<bool> DeletePlayerDataAsync(string playerId);
    }
}
